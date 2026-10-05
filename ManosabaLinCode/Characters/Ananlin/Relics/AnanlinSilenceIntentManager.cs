using System.Reflection;
using HarmonyLib;
using ManosabaLin.Characters.Ananlin.Cards;
using ManosabaLin.Characters.Ananlin.Powers;
using ManosabaLin.Characters.Common.LinRelics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ManosabaLin.Characters.Ananlin.Relics;

internal static class AnanlinSilenceIntentManager
{
    private const string EnergyMoveId = "MANOSABA_LIN_ANANLIN_SILENT_ENERGY_MOVE";
    private const string DrawMoveId = "MANOSABA_LIN_ANANLIN_SILENT_DRAW_MOVE";
    private const string BlockMoveId = "MANOSABA_LIN_ANANLIN_SILENT_BLOCK_MOVE";
    private const string VigorMoveId = "MANOSABA_LIN_ANANLIN_SILENT_VIGOR_MOVE";
    private const int BaseEnergy = 1;
    private const int BaseDraw = 1;
    private const int BaseBlock = 6;
    private const int BaseVigor = 2;

    private enum ReplacementIntentKind
    {
        Energy,
        Draw,
        Block,
        Vigor
    }

    private static readonly ReplacementIntentKind[] ReplacementIntentCycle =
    [
        ReplacementIntentKind.Energy,
        ReplacementIntentKind.Draw,
        ReplacementIntentKind.Block,
        ReplacementIntentKind.Vigor
    ];

    private static readonly FieldInfo? OnPerformField = AccessTools.Field(typeof(MoveState), "_onPerform");
    private static readonly Dictionary<Creature, MoveState> PendingBuffMoves = [];
    private static readonly Dictionary<Creature, HashSet<string>> UsedMovesByPhase = [];
    private static readonly Dictionary<Creature, string> PhaseKeys = [];
    private static readonly Dictionary<MoveState, MoveState> BaseMovesByReplacement = [];
    private static readonly Dictionary<Player, HashSet<ReplacementIntentKind>> UsedReplacementIntentsByPlayer = [];
    private static readonly Dictionary<Player, int> RewritesThisCombatByPlayer = [];
    private static readonly Dictionary<Player, int> SilenceGrowthByPlayer = [];
    private static readonly Dictionary<Player, int> SecondTamperAllowanceByPlayer = [];

    [ThreadStatic] private static bool _isApplyingReplacement;

    private sealed record ReplacementIntentChoice(ReplacementIntentKind Kind, MoveState Move);

    internal static async Task<int> Trigger(PlayerChoiceContext choiceContext, Player owner)
    {
        return (await TriggerAndGetTargets(choiceContext, owner)).Count;
    }

    internal static async Task<IReadOnlyList<Creature>> TriggerAndGetTargets(PlayerChoiceContext choiceContext, Player owner)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState is null) return [];

        var enemies = combatState.Enemies
            .Where(static c => c.IsAlive && c.Monster?.NextMove is not null)
            .ToArray();
        var replacementTargets = new List<Creature>();

        foreach (var enemy in enemies)
        {
            if (enemy.Monster is not { } monster) continue;

            // 二次篡改：当前意图已是替换意图的敌人，仅当带【已缄默】且本回合有授权时，可再改写一次（授权按玩家回合刷新）
            if (IsReplacementMove(monster.NextMove))
            {
                if (!HasSecondTamperAllowance(owner)
                    || enemy.GetPower<AnanlinSilencedPower>() is not { } silenced
                    || ResolveBaseMove(monster.NextMove) is null)
                    continue;

                // 先立刻触发第一次篡改后的意图效果，再选择替换意图
                ConsumeSecondTamper(owner);
                await monster.PerformMove();

                // 二次篡改后，魔法影响次数归零（描述显示"已被魔法影响0次"）；标记本身留待敌方回合开始自行移除
                silenced.MagicInfluenceRemaining = 0;

                if (enemy.IsAlive)
                    replacementTargets.Add(enemy);
                continue;
            }

            var currentMove = ResolveBaseMove(monster.NextMove);
            if (currentMove is null || !CanForceNow(monster, currentMove)) continue;

            // 缄默压制：敌人永久失去1点力量，并留下【被缄默】标记（无效果，敌人回合开始移除）
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -1m, owner.Creature, null);
            await PowerCmd.Apply<AnanlinSilencedPower>(choiceContext, enemy, 1m, owner.Creature, null);

            // 牢房才是家：本回合给予敌人【已【已缄默】时，进入【牢房】
            if (owner.Creature.GetPower<AnanlinPrisonIsHomePower>() is { } prison)
                await prison.TryEnterCellOnSilencedGiven(choiceContext);

            MarkForced(monster, currentMove);
            monster.SetMoveImmediate(currentMove, forceTransition: true);
            await monster.PerformMove();

            if (enemy.IsAlive)
                replacementTargets.Add(enemy);
        }

        if (replacementTargets.Count == 0) return [];

        var (baseBonus, multiplier) = GetSilencePoolValues(owner);
        var selectedBuff = await ChoosePlayerBuffIntent(choiceContext, owner, baseBonus, multiplier);
        if (selectedBuff is null) return [];

        var rewrittenTargets = new List<Creature>();
        foreach (var enemy in replacementTargets.Where(static e => e.IsAlive))
        {
            if (enemy.Monster is not { } monster) continue;
            // 二次篡改目标当前意图仍是替换意图，需要允许覆盖既有替换
            var replaceExisting = IsReplacementMove(monster.NextMove);
            if (ApplyReplacementMove(monster, selectedBuff.Move, replaceExisting))
                rewrittenTargets.Add(enemy);
        }

        if (rewrittenTargets.Count > 0)
        {
            MarkReplacementIntentUsed(owner, selectedBuff.Kind);
            RewritesThisCombatByPlayer[owner] = GetRewritesThisCombat(owner) + rewrittenTargets.Count;
            // 缄默成长：每次缄默替换意图后，下一次缄默替换意图数值+1（洗脑不受影响）
            SilenceGrowthByPlayer[owner] = GetSilenceGrowth(owner) + 1;
            if (owner.Creature.GetPower<AnanlinSealedPagePower>() is { } sealedPage)
                await sealedPage.AfterSilenceRightClickRewrite(choiceContext);
        }

        RecordIntentRewrites(combatState, rewrittenTargets.Count);
        return rewrittenTargets;
    }

    internal static async Task<bool> ForceBrainwash(
        PlayerChoiceContext choiceContext,
        Player owner,
        Func<Task<bool>>? beforeApply = null)
    {
        return (await ForceBrainwashAndGetTargets(choiceContext, owner, beforeApply)).Count > 0;
    }

    internal static async Task<IReadOnlyList<Creature>> ForceBrainwashAndGetTargets(
        PlayerChoiceContext choiceContext,
        Player owner,
        Func<Task<bool>>? beforeApply = null)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState is null) return [];

        var targets = GetBrainwashTargets(owner);
        if (targets.Count == 0) return [];

        // 洗脑：默认使用初始替换意图数值（不随缄默成长）；但吃无声扩音（特殊卡写明：公共替换意图池倍率+1）。
        // 【连线】海克斯「洗脑共鸣」：持有该遗物时，洗脑改为**读取【缄默】的可成长替换池**
        // （即当前缄默成长值），与缄默共用同一池子。
        // ⚠️ 用户（海克斯联动第 5 条）裁定：洗脑不仅读取、还**推进**同一个通用意图池 ——
        // 与缄默一样，每次成功改写后让成长 +1（见下方 SilenceGrowthByPlayer[...] + 1）。
        var baseBonus = HextechBrainwashResonance.IsActiveFor(owner) ? GetSilenceGrowth(owner) : 0;
        var selectedBuff = await ChoosePlayerBuffIntent(choiceContext, owner, baseBonus, GetReplacementValueMultiplier(owner));
        if (selectedBuff is null) return [];

        if (beforeApply is not null && !await beforeApply())
            return [];

        var rewrittenTargets = new List<Creature>();
        foreach (var target in targets.Where(CanBrainwashTarget))
        {
            if (target.Monster is not { } monster) continue;
            if (ApplyReplacementMove(monster, selectedBuff.Move))
                rewrittenTargets.Add(target);
        }

        if (rewrittenTargets.Count <= 0) return [];

        MarkReplacementIntentUsed(owner, selectedBuff.Kind);
        RewritesThisCombatByPlayer[owner] = GetRewritesThisCombat(owner) + rewrittenTargets.Count;

        // 【连线】海克斯「洗脑共鸣」：洗脑与缄默共用同一通用意图池，且洗脑也**推进**成长 +1
        // （与缄默 TriggerAndGetTargets 末尾的 SilenceGrowthByPlayer[...] + 1 一致）。
        if (HextechBrainwashResonance.IsActiveFor(owner))
            SilenceGrowthByPlayer[owner] = GetSilenceGrowth(owner) + 1;

        if (owner.Creature.GetPower<AnanlinSealedPagePower>() is { } sealedPage)
            await sealedPage.AfterSilenceRightClickRewrite(choiceContext);
        RecordIntentRewrites(combatState, rewrittenTargets.Count);
        return rewrittenTargets;
    }

    internal static int ApplyRandomReplacementIntentToAllEnemies(Player owner)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState is null) return 0;

        var targets = combatState.Enemies
            .Where(static enemy => enemy is { IsAlive: true, Monster: { NextMove: not null } })
            .ToArray();
        if (targets.Length == 0) return 0;

        var kind = owner.RunState.Rng.CombatCardGeneration.NextItem(ReplacementIntentCycle);
        var (baseBonus, multiplier) = GetSilencePoolValues(owner);
        var move = CreateReplacementMove(owner, kind, baseBonus, multiplier);

        var rewriteCount = 0;
        foreach (var target in targets)
        {
            if (target.Monster is { } monster && ApplyReplacementMove(monster, move, replaceExistingReplacement: true))
                rewriteCount++;
        }

        if (rewriteCount <= 0) return 0;

        MarkReplacementIntentUsed(owner, kind);
        RewritesThisCombatByPlayer[owner] = GetRewritesThisCombat(owner) + rewriteCount;
        RecordIntentRewrites(combatState, rewriteCount);
        return rewriteCount;
    }

    internal static int GetRewritesThisCombat(Player owner)
    {
        return RewritesThisCombatByPlayer.GetValueOrDefault(owner);
    }

    internal static bool CanForceBrainwash(Player owner)
    {
        return GetBrainwashTargets(owner).Any()
            && GetAvailableReplacementIntentKinds(owner).Any();
    }

    internal static bool CanTrigger(Player owner)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState is null) return false;

        var hasSecondTamper = HasSecondTamperAllowance(owner);

        foreach (var enemy in combatState.Enemies.Where(static c => c.IsAlive))
        {
            if (enemy.Monster is not { NextMove: { } nextMove } monster) continue;

            // 二次篡改：当前意图已是替换意图、带【已缄默】且本回合有授权 → 可再改写一次
            if (IsReplacementMove(nextMove))
            {
                if (hasSecondTamper && enemy.GetPower<AnanlinSilencedPower>() is not null)
                    return true;
                continue;
            }

            var currentMove = ResolveBaseMove(nextMove);
            if (currentMove is not null && CanForceNow(monster, currentMove))
                return true;
        }

        return false;
    }

    private static IReadOnlyList<Creature> GetBrainwashTargets(Player owner)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState is null) return Array.Empty<Creature>();

        return combatState.Enemies
            .Where(CanBrainwashTarget)
            .ToArray();
    }

    private static bool CanBrainwashTarget(Creature enemy)
    {
        return enemy is { IsAlive: true, Monster: { NextMove: { } nextMove } }
            && !IsReplacementMove(nextMove);
    }

    internal static bool TryApplyPendingBuffMove(MonsterModel monster)
    {
        if (_isApplyingReplacement) return false;
        var creature = monster.Creature;
        if (!PendingBuffMoves.Remove(creature, out var buffMove)) return false;

        return ApplyReplacementMove(monster, buffMove);
    }

    private static bool ApplyReplacementMove(
        MonsterModel monster,
        MoveState buffMove,
        bool replaceExistingReplacement = false)
    {
        if (_isApplyingReplacement) return false;
        if (!replaceExistingReplacement && IsReplacementMove(monster.NextMove)) return false;

        var baseMove = ResolveBaseMove(monster.NextMove);
        if (baseMove is null) return false;

        var replacement = CloneMoveWithPerform(
            buffMove,
            async targets =>
            {
                await buffMove.PerformMove(targets);
            },
            $"{buffMove.StateId}_{baseMove.StateId}",
            baseMove);

        BaseMovesByReplacement[replacement] = baseMove;

        _isApplyingReplacement = true;
        try
        {
            monster.SetMoveImmediate(replacement, forceTransition: true);
        }
        finally
        {
            _isApplyingReplacement = false;
        }

        return true;
    }

    internal static void ClearForNewCombat()
    {
        PendingBuffMoves.Clear();
        UsedMovesByPhase.Clear();
        PhaseKeys.Clear();
        BaseMovesByReplacement.Clear();
        UsedReplacementIntentsByPlayer.Clear();
        RewritesThisCombatByPlayer.Clear();
        SilenceGrowthByPlayer.Clear();
        SecondTamperAllowanceByPlayer.Clear();
    }

    internal static bool TryForgetRecordedAttack(Creature target)
    {
        if (target.Monster is not { } monster) return false;

        var phaseKey = GetPhaseKey(monster);
        if (!PhaseKeys.TryGetValue(target, out var knownPhase) || knownPhase != phaseKey)
            return false;
        if (!UsedMovesByPhase.TryGetValue(target, out var usedMoves) || usedMoves.Count == 0)
            return false;

        var currentBaseMove = ResolveBaseMove(monster.NextMove);
        if (currentBaseMove is not null
            && HasAttackIntent(currentBaseMove)
            && usedMoves.Remove(currentBaseMove.StateId))
        {
            return true;
        }

        var recordedAttackId = monster.MoveStateMachine?.States.Values
            .OfType<MoveState>()
            .Where(HasAttackIntent)
            .Select(static move => move.StateId)
            .Where(usedMoves.Contains)
            .Order(StringComparer.Ordinal)
            .FirstOrDefault();

        return recordedAttackId is not null && usedMoves.Remove(recordedAttackId);
    }

    internal static void RecordIntentRewrites(ICombatState? combatState, int count)
    {
        if (combatState is null || count <= 0) return;

        foreach (var player in combatState.Players)
        {
            player.Creature.GetPower<AnanlinJudgmentEvePower>()?.RecordRewrites(count);
            player.Creature.GetPower<AnanlinSilentAmplificationPower>()?.RecordRewrites(count);
        }
    }

    private static async Task<ReplacementIntentChoice?> ChoosePlayerBuffIntent(
        PlayerChoiceContext choiceContext,
        Player owner,
        int baseBonus,
        int multiplier)
    {
        if (owner.Creature.CombatState is not { } combatState) return null;

        var availableKinds = GetAvailableReplacementIntentKinds(owner).ToArray();
        if (availableKinds.Length == 0) return null;

        var options = availableKinds
            .Select(kind => CreateReplacementOptionCard(kind, combatState, owner, baseBonus, multiplier))
            .ToArray();

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            options,
            owner,
            new CardSelectorPrefs(
                new LocString("relics", "MANOSABA_LIN_RELIC_ANANS_SKETCHBOOK.selectionScreenPrompt"),
                1,
                1))).FirstOrDefault();

        var selectedKind = GetReplacementIntentKind(selected);
        if (selectedKind is null) return null;

        return new ReplacementIntentChoice(selectedKind.Value, CreateReplacementMove(owner, selectedKind.Value, baseBonus, multiplier));
    }

    private static IEnumerable<ReplacementIntentKind> GetAvailableReplacementIntentKinds(Player owner)
    {
        if (!UsedReplacementIntentsByPlayer.TryGetValue(owner, out var used))
            UsedReplacementIntentsByPlayer[owner] = used = [];

        if (used.Count >= ReplacementIntentCycle.Length)
            used.Clear();

        return ReplacementIntentCycle.Where(kind => !used.Contains(kind));
    }

    private static void MarkReplacementIntentUsed(Player owner, ReplacementIntentKind kind)
    {
        if (!UsedReplacementIntentsByPlayer.TryGetValue(owner, out var used))
            UsedReplacementIntentsByPlayer[owner] = used = [];

        if (used.Count >= ReplacementIntentCycle.Length)
            used.Clear();

        used.Add(kind);
    }

    private static CardModel CreateReplacementOptionCard(
        ReplacementIntentKind kind,
        ICombatState combatState,
        Player owner,
        int baseBonus,
        int multiplier)
    {
        CardModel card = kind switch
        {
            ReplacementIntentKind.Energy => combatState.CreateCard<AnanlinSilenceIntentEnergyOption>(owner),
            ReplacementIntentKind.Draw => combatState.CreateCard<AnanlinSilenceIntentDrawOption>(owner),
            ReplacementIntentKind.Block => combatState.CreateCard<AnanlinSilenceIntentBlockOption>(owner),
            ReplacementIntentKind.Vigor => combatState.CreateCard<AnanlinSilenceIntentVigorOption>(owner),
            _ => combatState.CreateCard<AnanlinSilenceIntentBlockOption>(owner)
        };

        ApplyReplacementOptionValue(card, kind, baseBonus, multiplier);
        return card;
    }

    private static void ApplyReplacementOptionValue(CardModel card, ReplacementIntentKind kind, int baseBonus, int multiplier)
    {
        switch (kind)
        {
            case ReplacementIntentKind.Energy:
                card.DynamicVars.Energy.BaseValue = (BaseEnergy + baseBonus) * multiplier;
                break;
            case ReplacementIntentKind.Draw:
                card.DynamicVars.Cards.BaseValue = (BaseDraw + baseBonus) * multiplier;
                break;
            case ReplacementIntentKind.Block:
                card.DynamicVars.Block.BaseValue = (BaseBlock + baseBonus) * multiplier;
                break;
            case ReplacementIntentKind.Vigor:
                card.DynamicVars["VigorPower"].BaseValue = (BaseVigor + baseBonus) * multiplier;
                break;
        }
    }

    private static ReplacementIntentKind? GetReplacementIntentKind(CardModel? card)
    {
        return card switch
        {
            AnanlinSilenceIntentEnergyOption => ReplacementIntentKind.Energy,
            AnanlinSilenceIntentDrawOption => ReplacementIntentKind.Draw,
            AnanlinSilenceIntentBlockOption => ReplacementIntentKind.Block,
            AnanlinSilenceIntentVigorOption => ReplacementIntentKind.Vigor,
            _ => null
        };
    }

    private static MoveState CreateReplacementMove(Player owner, ReplacementIntentKind kind, int baseBonus, int multiplier)
    {
        return kind switch
        {
            ReplacementIntentKind.Energy => new MoveState(
                EnergyMoveId,
                async _ => await ApplyEnergyToAllPlayers(owner, (BaseEnergy + baseBonus) * multiplier),
                new BuffIntent()),
            ReplacementIntentKind.Draw => new MoveState(
                DrawMoveId,
                async _ => await DrawForAllPlayers(owner, (BaseDraw + baseBonus) * multiplier),
                new BuffIntent()),
            ReplacementIntentKind.Block => new MoveState(
                BlockMoveId,
                async _ => await ApplyBlockNextTurnToAllPlayers(owner, (BaseBlock + baseBonus) * multiplier),
                new DefendIntent()),
            ReplacementIntentKind.Vigor => new MoveState(
                VigorMoveId,
                async _ => await ApplyVigorToAllPlayers(owner, (BaseVigor + baseBonus) * multiplier),
                new BuffIntent()),
            _ => new MoveState(
                BlockMoveId,
                async _ => await ApplyBlockNextTurnToAllPlayers(owner, (BaseBlock + baseBonus) * multiplier),
                new DefendIntent())
        };
    }

    private static Player[] GetLivingEffectPlayers(Player owner)
    {
        var players = owner.Creature.CombatState?.Players;
        if (players is null) return [owner];

        var livingPlayers = players
            .Where(static player => player.Creature.IsAlive)
            .ToArray();
        return livingPlayers.Length > 0 ? livingPlayers : [owner];
    }

    private static async Task ApplyEnergyToAllPlayers(Player owner, int amount)
    {
        var choiceContext = new BlockingPlayerChoiceContext();
        foreach (var player in GetLivingEffectPlayers(owner))
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, player.Creature, amount, owner.Creature, null);
    }

    private static async Task DrawForAllPlayers(Player owner, int amount)
    {
        var choiceContext = new BlockingPlayerChoiceContext();
        foreach (var player in GetLivingEffectPlayers(owner))
            await CardPileCmd.Draw(choiceContext, amount, player);
    }

    private static async Task ApplyBlockNextTurnToAllPlayers(Player owner, int amount)
    {
        var choiceContext = new BlockingPlayerChoiceContext();
        foreach (var player in GetLivingEffectPlayers(owner))
            await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, player.Creature, amount, owner.Creature, null);
    }

    private static async Task ApplyVigorToAllPlayers(Player owner, int amount)
    {
        var choiceContext = new BlockingPlayerChoiceContext();
        foreach (var player in GetLivingEffectPlayers(owner))
            await PowerCmd.Apply<VigorPower>(choiceContext, player.Creature, amount, owner.Creature, null);
    }

    private static int GetReplacementValueMultiplier(Player owner)
    {
        return owner.Creature.GetPower<AnanlinSilentAmplificationPower>()?.ReplacementValueMultiplier ?? 1;
    }

    internal static int GetSilenceGrowth(Player owner)
    {
        return SilenceGrowthByPlayer.GetValueOrDefault(owner);
    }

    /// <summary>缄默替换意图数值 +N（删去重音、牢房【牢房】回合末等用）。</summary>
    internal static void IncreaseSilenceGrowth(Player owner, int amount = 1)
    {
        SilenceGrowthByPlayer[owner] = GetSilenceGrowth(owner) + amount;
    }

    // ========== 二次篡改授权（魔女囚犯） ==========
    // 让缄默本回合可以再改写一次敌人意图。授权次数按玩家回合刷新（AnansSketchbook.AfterPlayerTurnStart）。
    internal static int GetSecondTamperAllowance(Player owner)
    {
        return SecondTamperAllowanceByPlayer.GetValueOrDefault(owner);
    }

    internal static bool HasSecondTamperAllowance(Player owner)
    {
        return GetSecondTamperAllowance(owner) > 0;
    }

    /// <summary>授权一次二次篡改：缄默本回合可再多改写一次敌人意图。</summary>
    internal static void GrantSecondTamper(Player owner)
    {
        SecondTamperAllowanceByPlayer[owner] = GetSecondTamperAllowance(owner) + 1;
    }

    private static void ConsumeSecondTamper(Player owner)
    {
        var remaining = GetSecondTamperAllowance(owner) - 1;
        if (remaining <= 0)
            SecondTamperAllowanceByPlayer.Remove(owner);
        else
            SecondTamperAllowanceByPlayer[owner] = remaining;
    }

    internal static void ResetSecondTamperAllowances()
    {
        SecondTamperAllowanceByPlayer.Clear();
    }

    /// <summary>随机触发当前缄默意图池里一张卡的效果（对持有者执行，不弹 UI、不改怪物意图）。</summary>
    internal static async Task TriggerRandomReplacementEffect(PlayerChoiceContext choiceContext, Player owner)
    {
        var kind = owner.RunState.Rng.CombatCardGeneration.NextItem(ReplacementIntentCycle);
        var (baseBonus, multiplier) = GetSilencePoolValues(owner);
        switch (kind)
        {
            case ReplacementIntentKind.Energy:
                await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, owner.Creature, (BaseEnergy + baseBonus) * multiplier, owner.Creature, null);
                break;
            case ReplacementIntentKind.Draw:
                await CardPileCmd.Draw(choiceContext, (BaseDraw + baseBonus) * multiplier, owner);
                break;
            case ReplacementIntentKind.Block:
                await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, owner.Creature, (BaseBlock + baseBonus) * multiplier, owner.Creature, null);
                break;
            case ReplacementIntentKind.Vigor:
                await PowerCmd.Apply<VigorPower>(choiceContext, owner.Creature, (BaseVigor + baseBonus) * multiplier, owner.Creature, null);
                break;
        }
    }

    /// <summary>将随机一张当前缄默替换意图牌加入手牌，数值 = 当前意图池数值且固定（全文替换用）。</summary>
    internal static async Task<CardModel?> AddRandomReplacementIntentCardToHand(PlayerChoiceContext choiceContext, Player owner)
    {
        if (owner.Creature.CombatState is not { } combatState) return null;

        var kind = owner.RunState.Rng.CombatCardGeneration.NextItem(ReplacementIntentCycle);
        var (baseBonus, multiplier) = GetSilencePoolValues(owner);
        var card = CreateReplacementOptionCard(kind, combatState, owner, baseBonus, multiplier);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, owner);
        return card;
    }

    /// <summary>缄默替换意图池数值：初始值 + 本场缄默成长次数，倍率来自无声扩音。</summary>
    private static (int BaseBonus, int Multiplier) GetSilencePoolValues(Player owner)
    {
        return (GetSilenceGrowth(owner), GetReplacementValueMultiplier(owner));
    }

    private static bool CanForceNow(MonsterModel monster, MoveState move)
    {
        // 不再限制：每个动作一阶段内最多被强制一次的限制已移除，恒可强制。
        return true;
    }

    private static void MarkForced(MonsterModel monster, MoveState move)
    {
        if (!UsedMovesByPhase.TryGetValue(monster.Creature, out var used))
            UsedMovesByPhase[monster.Creature] = used = [];

        used.Add(move.StateId);
    }

    private static string GetPhaseKey(MonsterModel monster)
    {
        var moveIds = GetPhaseMoveIds(monster);
        return $"{monster.Id.Entry}:{string.Join("|", moveIds.Order(StringComparer.Ordinal))}";
    }

    private static HashSet<string> GetPhaseMoveIds(MonsterModel monster)
    {
        var machine = monster.MoveStateMachine;
        if (machine is null) return [];

        return machine.States.Values
            .OfType<MoveState>()
            .Where(static m => m.IsMove && m.ShouldAppearInLogs && m.StateId != MonsterModel.stunnedMoveId)
            .Select(static m => m.StateId)
            .ToHashSet();
    }

    private static MoveState? ResolveBaseMove(MoveState? move)
    {
        if (move is null) return null;
        return BaseMovesByReplacement.GetValueOrDefault(move) ?? move;
    }

    private static bool IsReplacementMove(MoveState? move)
    {
        return move is not null && BaseMovesByReplacement.ContainsKey(move);
    }

    private static bool HasAttackIntent(MoveState move)
    {
        return move.Intents.Any(static intent => intent is AttackIntent);
    }

    private static MoveState CloneMoveWithPerform(
        MoveState source,
        Func<IReadOnlyList<Creature>, Task> perform,
        string suffix,
        MoveState transitionSource)
    {
        // The wrapper replaces transitionSource for one turn, so its next state must follow that move's graph.
        var fallbackFollowUp = transitionSource.FollowUpStateId is null
            ? transitionSource
            : null;

        var clone = new MoveState(
            $"{source.StateId}_{suffix}",
            perform,
            source.Intents.ToArray())
        {
            FollowUpState = transitionSource.FollowUpState ?? fallbackFollowUp,
            FollowUpStateId = transitionSource.FollowUpStateId,
            MustPerformOnceBeforeTransitioning = transitionSource.MustPerformOnceBeforeTransitioning
        };

        if (OnPerformField?.GetValue(source) is not null)
            return clone;

        return clone;
    }
}
