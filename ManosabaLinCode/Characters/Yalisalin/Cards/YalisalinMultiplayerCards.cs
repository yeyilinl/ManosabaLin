using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     共燃（1 费 攻击・罕见・多人专属）：
///     造成伤害；本次余火额外随机连接一名队友弃牌堆里的一张攻击牌，
///     那张连接的牌被烧掉时，你和它原本的主人各自动打出它一次。
///     升级：伤害 +3。
/// </summary>
/// <remarks>
///     「额外连接」沿用发夹「所有牌 &lt; 13 时额外连接 1 张随机牌」的既有通道：
///     在 <see cref="ModifyFireComponentChoiceOptions" /> 里把自己挑的那张牌塞进
///     <see cref="YalisalinFireComponentContext.AddExclusiveBurnCard" />（= 选项 + 独占烧牌），
///     但它<b>不</b>设置 <c>BurnOnlyExclusiveCards</c>，所以源卡/连接牌的那一套烧牌照旧，
///     这张队友的牌是「额外」多烧一张。
/// </remarks>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class CombustionShared()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy),
        IYalisalinFireComponentModifier
{
    internal const string LinkedCardKey = "YalisalinCombustionSharedLinked";

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (cardPlay.Target is not { } target)
            return;

        await YalisalinFireColorCardHelpers.Attack(
            choiceContext, cardPlay, this, target, DynamicVars.Damage.BaseValue);
    }

    public void ModifyFireComponentChoiceOptions(YalisalinFireComponentContext context)
    {
        if (context.SourceCard != this)
            return;

        if (PickTeammateDiscardAttack(context.Owner) is not { } linked)
            return;

        context.CustomData[LinkedCardKey] = linked;
        context.AddExclusiveBurnCard(linked);
    }

    public async Task BeforeFireComponentBurned(
        PlayerChoiceContext choiceContext,
        YalisalinFireComponentContext context)
    {
        if (context.CustomData.GetValueOrDefault(LinkedCardKey) is not CardModel linked)
            return;
        if (!ReferenceEquals(context.BurnedCard, linked))
            return;
        if (linked.HasBeenRemovedFromState)
            return;

        // 「你和有这张牌的队友自动打出一次」＝ 这张牌在两头各响一次。
        // 走原版 AutoPlay（无目标时按目标类型自动选），放在烧掉<b>之前</b>，
        // 因为 Burn() 之后牌已经进消耗堆。
        await CardCmd.AutoPlay(choiceContext, linked, null);
        await CardCmd.AutoPlay(choiceContext, linked, null);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    /// <summary>从所有队友的弃牌堆里随机抽一张可打出的攻击牌（走联机 RNG）。</summary>
    private static CardModel? PickTeammateDiscardAttack(Player owner)
    {
        if (owner.Creature.CombatState is not { } combatState)
            return null;

        var candidates = combatState.Players
            .Where(player => player != owner && player.Creature.IsAlive)
            .SelectMany(player => PileType.Discard.GetPile(player).Cards)
            .Where(static card => card.Type == CardType.Attack)
            .Where(static card => !card.HasBeenRemovedFromState)
            .Where(static card => !card.Keywords.Contains(CardKeyword.Unplayable))
            .Distinct()
            .ToArray();

        return candidates.Length == 0
            ? null
            : owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
    }
}

/// <summary>
///     代你燎原（2 费 攻击・罕见・多人专属）：
///     选择一名队友；本回合他对敌人造成伤害时，给该敌人增加「本次伤害命中段数」等量的火色。
///     升级：费用 2 → 1。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class StokeForYou()
    : ManosabaCardTemplate(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (cardPlay.Target is not { } ally)
            return;
        if (ally == Owner.Creature || ally.Side != Owner.Creature.Side || !ally.IsAlive)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<StokeForYouPower>(
            choiceContext, ally, 1m, Owner.Creature, this, false);
        if (power is not null)
            power.Caster = Owner;
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}

/// <summary>
///     薪火相传（3 费 能力・稀有・多人专属）：
///     选择一名队友；本场战斗每当他使用指向敌人的「指向性」技能牌时，给那个敌人 1 格火色
///     （按「予燎」结算：超出量表的格数立即触发消耗效果）。
///     升级：费用 3 → 2。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class TorchPassing()
    : ManosabaCardTemplate(3, CardType.Power, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (cardPlay.Target is not { } ally)
            return;
        if (ally == Owner.Creature || ally.Side != Owner.Creature.Side || !ally.IsAlive)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<TorchPassingPower>(
            choiceContext, ally, 1m, Owner.Creature, this, false);
        if (power is not null)
            power.Caster = Owner;
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}

/// <summary>
///     共罪（1 费 技能・罕见・多人专属）：获得 1 层【共罪】。
///     每当有队友的【嫌疑】达到 12 层（引擎的坏结局阈值）时：移除他已吃到的坏结局，
///     其【嫌疑】减半、你拿走另一半；你与他各获得 1 层【魔女仪式】，然后移除 1 层【共罪】。
///     升级：救下队友后，额外移除他所有的「减力量」。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class SharedGuilt()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var power = await PowerCmd.Apply<SharedGuiltPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        if (power is not null)
            power.ClearsStrengthDown = IsUpgraded;
    }
}

/// <summary>
///     余烬分赠（1 费 攻击・稀有・多人专属；自带余火）：
///     恢复全体友方 6 点生命。
///     当此牌被余火烧掉时：下回合返回手牌；并且给每个队友一张「他所在角色卡池、带余火、
///     本回合 0 费且虚无」的牌。
///     升级：治疗量 6 → 9。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class EmberDividend()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Rare, TargetType.Self),
        IYalisalinFireComponentModifier
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Heal", 6)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner.Creature.CombatState is not { } combatState)
            return;

        foreach (var creature in combatState.PlayerCreatures.Where(static creature => creature.IsAlive))
            await CreatureCmd.Heal(creature, DynamicVars["Heal"].BaseValue);
    }

    public async Task AfterFireComponentBurned(
        PlayerChoiceContext choiceContext,
        YalisalinFireComponentContext context)
    {
        if (!ReferenceEquals(context.BurnedCard, this))
            return;

        if (YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            hairpin.QueueHandReturn(this);

        await GiftToTeammates(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["Heal"].UpgradeValueBy(3);
    }

    private async Task GiftToTeammates(PlayerChoiceContext choiceContext)
    {
        if (Owner.Creature.CombatState is not { } combatState)
            return;

        foreach (var teammate in combatState.Players
                     .Where(player => player != Owner && player.Creature.IsAlive))
            await GiftOneCard(teammate);
    }

    private async Task GiftOneCard(Player teammate)
    {
        if (teammate.Creature.CombatState is not { } combatState)
            return;

        // 「其卡池」＝ 该队友自己所玩角色的卡池。
        var pool = teammate.Character.CardPool;
        if (pool == null)
            return;

        var candidates = pool
            .GetUnlockedCards(teammate.UnlockState, teammate.RunState.CardMultiplayerConstraint)
            .Where(static card => card.Rarity is not CardRarity.Basic
                and not CardRarity.Token
                and not CardRarity.Status
                and not CardRarity.Curse)
            .Where(static card => card.CanBeGeneratedInCombat)
            .ToArray();

        if (candidates.Length == 0)
            return;

        var canonical = Owner.RunState.Rng.CombatCardGeneration.NextItem(candidates);
        if (canonical == null)
            return;

        var gift = combatState.CreateCard(canonical, teammate);
        YalisalinFireComponentRules.TryAddFireComponent(gift);
        gift.SetToFreeThisTurn();
        CardCmd.ApplyKeyword(gift, CardKeyword.Ethereal);

        await CardPileCmd.AddGeneratedCardToCombat(gift, PileType.Hand, teammate);
    }
}
