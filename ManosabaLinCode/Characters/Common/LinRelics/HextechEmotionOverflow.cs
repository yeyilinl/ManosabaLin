using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Sherrylin.Cards.Emotions;
using ManosabaLin.Characters.Sherrylin.Orbs;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物 2：情绪球**因球位已满被挤出**时的处理。
///     <list type="bullet">
///         <item>
///             <b>持续型</b>（悲伤 / 愤怒 / 快乐 / 怅然 / 雀跃 / 好奇 / 友谊）⇒ <b>脱离球位</b>：
///             球从 <c>OrbQueue</c> 里被摘下来，<b>真正挂到血条下方</b>（<see cref="EmotionHangDisplay" />），
///             效果由<b>球对象自己</b>继续跑，直到<b>下个玩家回合开始</b>才释放。
///             （拦截点在 <c>OrbCmd.Channel</c> 的 Prefix，见 <c>HextechOrbOverflowPatch</c>。）
///             <para>
///                 ⚠️ <b>这里绝不能再造一个能力来接管效果</b>，也<b>不能把效果逻辑搬到别处重写</b>：
///                 引擎的钩子枚举（<c>CombatState.IterateHookListeners</c>）本来只认
///                 <c>OrbQueue.Orbs</c> 里的球，但引擎给了模组正规出口
///                 <c>ModHelper.SubscribeForCombatStateHooks</c> —— 把球注册成监听者，
///                 它离开球位后照样收钩子。所以「换个地方挂着的还是那颗球、效果一点没变」。
///                 见 <see cref="HangingEmotionOrbs" />。
///             </para>
///         </item>
///         <item>
///             <b>反伤型</b>（厌恶 / 骇厌）⇒ 正常被挤出，由本遗物手工复刻那次激发：
///             所有<b>正打算攻击</b>的敌人各用自己的攻击打<b>激发者（你）</b>一次，
///             随即结算对应的反伤，最后把受到的伤害等量回复。
///         </item>
///         <item>
///             <b>延迟型</b>（恐惧 / 惊讶 / 恼惧 / 凄惶 / 无助）⇒ 正常被挤出，把
///             「回合结束 / 下回合开始」才结算的收益<b>立刻结算一次</b>。
///         </item>
///     </list>
///     <para>
///         被挤出这件事发生在 <c>OrbCmd.Channel</c>（球位满 ⇒ 先 <c>EvokeNext</c> 队首）；
///         因为球在 <c>AfterOrbEvoked</c> 之前就已经离开 <c>OrbQueue</c>，所以那一次激发
///         <b>完全靠本遗物手工复刻</b>，球自己的钩子不会再跑一遍（不会重复结算）。
///     </para>
///     <para>
///         反伤全程<b>不调用</b> <c>PerformMove</c> / <c>SetMoveImmediate</c> ⇒ 敌人的原本意图不变，
///         它自己的回合照常把这一下打出来（我们只借用它的意图数值）。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechEmotionOverflow : ManosabaRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    internal static bool IsActiveFor(Player? player)
        => player?.Relics.OfType<HextechEmotionOverflow>().Any() == true;

    /// <summary>
    ///     下个玩家回合开始 ⇒ 把「挂着的」持续型情绪球全部释放，
    ///     与球体自身 <c>AfterTurnStartOrbTrigger ⇒ EvokeNext</c>「到下回合开始消散」对齐。
    /// </summary>
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        HangingEmotionOrbs.Release(player);
        return Task.CompletedTask;
    }

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb,
        IEnumerable<Creature> targets)
    {
        if (orb.Owner != Owner) return;
        if (orb is not IEmotionOrb emotionOrb) return;

        var emotionCard = emotionOrb.GetEmotionCard();

        // 取走标记：只有「被挤出去的那一次」才算激发。
        // ⚠️ 持续型压根不会被挤出去（Channel 的 Prefix 已经把它摘下来挂着了），
        // 走到这里的持续型只可能是它自己在下个回合开始消散 ⇒ 不结算任何东西。
        var squeezed = HextechOrbEvokeRules.TryTakeSqueezedOut(orb);
        if (!squeezed) return;

        if (emotionCard is EmotionDisgust or EmotionHorrorDisgust)
        {
            await ResolveDisgust(choiceContext, emotionCard);
            return;
        }

        await ResolveDelayedPayout(choiceContext, emotionCard);
    }

    /// <summary>
    ///     厌恶 / 骇厌：所有**正打算攻击**的敌人各用自己的攻击打<b>你</b>一次，
    ///     随后按该情绪原本的反伤结算，最后把受到的伤害等量回复。
    ///     <para>
    ///         厌恶 ⇒ 打出这一下的敌人自己吃下等量伤害；
    ///         骇厌 ⇒ <b>敌方全体</b>吃下等量伤害并抽 1，另把「下回合才打出的等量手牌数 1 点伤害」立刻结算一次。
    ///     </para>
    ///     <para>⚠️ 没有任何敌人「正打算攻击」时取不到伤害量 ⇒ 本项不生效。</para>
    ///     <para>
    ///         ⚠️ 这是**真实承伤**（会受格挡/能力影响），因此低血时主动挤球可能致死；
    ///         好处是玩家完全掌控触发时机（只有主动 Channel 满位球才会走到这里）。
    ///     </para>
    /// </summary>
    private async Task ResolveDisgust(PlayerChoiceContext choiceContext, CardModel emotionCard)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState is null) return;

        // NextMove 必须先判空：IntendsToAttack 内部直接解引用 NextMove。
        var attackers = combatState.Enemies
            .Where(static c => c is { IsAlive: true, Monster: { NextMove: not null } monster }
                && monster.IntendsToAttack)
            .ToList();
        if (attackers.Count == 0) return;

        var isHorror = emotionCard is EmotionHorrorDisgust;
        var totalReceived = 0m;
        var flashed = false;

        foreach (var enemy in attackers)
        {
            if (enemy.Monster is not { } monster) continue;
            if (monster.NextMove is not { } move) continue;

            var incoming = move.Intents
                .OfType<AttackIntent>()
                .Sum(intent => intent.GetTotalDamage([enemy], monster.Creature));
            if (incoming <= 0) continue;

            if (!flashed)
            {
                Flash();
                flashed = true;
            }

            // 只能打「激发的这个人」——即便原意图是 AOE，这里也只把伤害指向你。
            await DamageCmd.Attack(incoming)
                .FromMonster(monster)
                .Targeting(Owner.Creature)
                .Execute(choiceContext);

            // 只补「记录」与首动标记：不碰 NextMove ⇒ 敌人原本意图不变。
            monster.MoveStateMachine?.OnMovePerformed(move);
            CombatManager.Instance.History.MonsterPerformedMove(combatState, monster, move, [Owner.Creature]);

            totalReceived += incoming;

            await ReflectDisgust(choiceContext, combatState, enemy, incoming, isHorror);
        }

        if (totalReceived > 0)
            await CreatureCmd.Heal(Owner.Creature, totalReceived);

        if (isHorror)
            await HorrorDelayedPayout(choiceContext, combatState);
    }

    /// <summary>厌恶 / 骇厌的反伤结算（逐次复刻球体的 <c>AfterDamageReceived</c>）。</summary>
    private async Task ReflectDisgust(PlayerChoiceContext choiceContext, ICombatState combatState,
        Creature attacker, decimal amount, bool isHorror)
    {
        if (!isHorror)
        {
            // 厌恶：只把等量伤害打回"造成这次伤害的那个敌人"。
            await CreatureCmd.Damage(choiceContext, attacker, amount, ValueProp.Move, null, null);
            return;
        }

        // 骇厌：敌方全体吃等量伤害，再抽 1（每次承伤都结算一次）。
        foreach (var enemy in combatState.HittableEnemies.Where(static c => c.IsAlive).ToList())
            await CreatureCmd.Damage(choiceContext, enemy, amount, ValueProp.Move, null, null);

        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    /// <summary>
    ///     骇厌被延后的那一半：「下回合开始随机对敌人造成等于手牌数的 1 点伤害」⇒ 立刻结算一次。
    /// </summary>
    private async Task HorrorDelayedPayout(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        var handCount = PileType.Hand.GetPile(Owner).Cards.Count;
        var enemies = combatState.HittableEnemies.Where(static c => c.IsAlive).ToList();
        if (handCount <= 0 || enemies.Count == 0) return;

        // 多人下一切"随机"必须走 RunState.Rng（联机 desync 铁律）。
        var rng = Owner.RunState.Rng.CombatTargets;

        for (var i = 0; i < handCount; i++)
        {
            var target = enemies[rng.NextInt(enemies.Count)];
            await CreatureCmd.Damage(choiceContext, target, 1m, ValueProp.Move, null, null);
        }
    }

    /// <summary>
    ///     延迟型：把原本「回合结束 / 下回合开始」才结算的收益立刻结算一次。
    ///     <para>恐惧 / 惊讶 / 恼惧 / 凄惶 / 无助 —— 各自复刻对应球体的那段结算代码。</para>
    /// </summary>
    private async Task ResolveDelayedPayout(PlayerChoiceContext choiceContext, CardModel emotionCard)
    {
        if (emotionCard is not (EmotionFear or EmotionSurprise or EmotionIrritatedFear
            or EmotionDesolate or EmotionHelplessness))
        {
            return;
        }

        var combatState = Owner.Creature.CombatState;
        if (combatState is null) return;

        Flash();

        switch (emotionCard)
        {
            case EmotionFear:
            {
                var halfBlock = Math.Floor(Owner.Creature.Block / 2m);
                if (halfBlock <= 0) return;

                var allies = combatState.GetTeammatesOf(Owner.Creature)
                    .Where(static c => c.IsAlive)
                    .Append(Owner.Creature)
                    .ToList();

                var target = Owner.RunState.Rng.CombatCardSelection.NextItem(allies);
                if (target is not null)
                    await CreatureCmd.GainBlock(target, halfBlock, ValueProp.Unpowered, null);

                return;
            }

            case EmotionSurprise:
            {
                var handCount = PileType.Hand.GetPile(Owner).Cards.Count;
                if (handCount > 0)
                    await CardPileCmd.Draw(choiceContext, handCount, Owner);

                return;
            }

            case EmotionIrritatedFear:
            {
                await CreatureCmd.Damage(choiceContext, Owner.Creature, 3m, ValueProp.Unpowered, null, null);

                var blockAmount = Owner.Creature.Block;
                if (blockAmount <= 0) return;

                var allies = combatState.GetTeammatesOf(Owner.Creature)
                    .Where(static c => c.IsAlive)
                    .Append(Owner.Creature);

                foreach (var ally in allies)
                    await CreatureCmd.GainBlock(ally, blockAmount, ValueProp.Unpowered, null);

                return;
            }

            case EmotionDesolate:
            {
                var half = Math.Floor(Owner.Creature.Block / 2m);
                if (half <= 0) return;

                await CreatureCmd.Heal(Owner.Creature, half);
                await CreatureCmd.GainBlock(Owner.Creature, half, ValueProp.Unpowered, null);

                return;
            }

            case EmotionHelplessness:
            {
                if (Owner.Creature.GetPower<DexterityPower>() is not { Amount: < 0 } dexDown) return;

                var amount = -dexDown.Amount;
                await PowerCmd.Remove(dexDown);

                await PowerCmd.Apply<ThornsPower>(
                    choiceContext, Owner.Creature, amount, Owner.Creature, null, false);

                var enemies = combatState.Enemies.Where(static c => c.IsAlive).ToList();
                if (enemies.Count == 0) return;

                var target = enemies[Owner.RunState.Rng.CombatCardSelection.NextInt(enemies.Count)];
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext, target, -amount, Owner.Creature, null, false);

                return;
            }
        }
    }
}

/// <summary>
///     情绪球在「被挤出球位」这件事上的分类 —— 全项目只此一处定义，供遗物与 <c>OrbCmd.Channel</c> 补丁共用。
/// </summary>
internal static class EmotionOverflowRules
{
    /// <summary>
    ///     7 种「持续型」：效果<b>完全由球自身钩子</b>提供（不需要能力、也不需要把逻辑搬到别处）。
    ///     因此球位满时<b>不把它挤掉</b>，而是把它从球位摘下来**挂到血条下方**，
    ///     由它自己继续生效到下个玩家回合开始（见 <see cref="HangingEmotionOrbs" />）。
    /// </summary>
    public static bool IsPersistent(CardModel? emotionCard)
        => emotionCard is EmotionSadness or EmotionAnger or EmotionJoy or EmotionMelancholy
            or EmotionElation or EmotionCuriosity or EmotionFriendship;
}

/// <summary>
///     「这个球是不是刚被挤出去的」的记账。
///     <para>
///         挤出标记由 <c>HextechOrbOverflowPatch</c> 在 <c>OrbCmd.Channel</c> 发现球位已满、
///         且队首是「非持续型」情绪球时打上；<see cref="HextechEmotionOverflow" /> 在
///         <c>AfterOrbEvoked</c> 里取走。用「取走即清空」的语义，避免同一标记被后续无关的激发重复消费。
///     </para>
///     <para>
///         ⚠️ 之前那套「给持续型多开一格（<c>OrbCmd.AddSlots</c>）+ 球消散时归还」的记账已删除
///         （用户 2026-09-29 裁定要的是**脱离球位**，不是占着球位不让出去）。
///     </para>
/// </summary>
internal static class HextechOrbEvokeRules
{
    private static OrbModel? _squeezedOutOrb;

    public static void MarkSqueezedOut(OrbModel orb) => _squeezedOutOrb = orb;

    public static bool TryTakeSqueezedOut(OrbModel orb)
    {
        var matched = ReferenceEquals(_squeezedOutOrb, orb);
        _squeezedOutOrb = null;
        return matched;
    }
}
