using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 点火（2 费能力・Ancient）：亚里沙的专属魔法卡，魔女化 100 时获得。
/// 回合开始或回合结束时，按层数 N 随机攻击 N 次（敌我均可）：
/// 每次对目标造成等于当前层数的伤害；命中敌方则随机友方 +1 格挡、敌方无易伤则 +1 易伤；
/// 命中友方则改为造成 1 点伤害，然后本能力 +2 层。
/// 升级：费用 -1。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Ignite() : ManosabaCardTemplate(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<IgnitePower>(1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<IgnitePower>(); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await PowerCmd.Apply<IgnitePower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
