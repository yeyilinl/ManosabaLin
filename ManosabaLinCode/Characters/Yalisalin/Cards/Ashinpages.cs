using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     夹在书页里的灰：
///     获得格挡，并按你当前封存火焰能力里最高的一档，给所有敌人施加对应层数的易伤
///     —— 浅橙 1 层、亮黄 2 层、赤红 3 层、黑红碳化 4 层；没有封存火焰时不施加易伤。
///     卡面文案为「获得{Block}点格挡。按照你当前封存火焰能力的颜色，给予所有敌人对应层数的易伤。」
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Ashinpages()
    : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<VulnerablePower>(); }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        // 格挡部分：与原实现一致，只作用于自身。
        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block, cardPlay);

        if (!YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin))
            return;

        // 按最高封存火色决定层数：浅橙1 / 亮黄2 / 赤红3 / 黑红4
        var stacks = hairpin.GetHighestSealedFireStacks();
        if (stacks <= 0)
            return;

        if (owner.Creature.CombatState is not { } combatState)
            return;

        foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive).ToList())
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext, enemy, stacks, owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
