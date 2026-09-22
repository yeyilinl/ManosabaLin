using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 循环法典（3 费能力・稀有）：
/// 获得 [循环法典]：每当你宽恕/自惩时，若该原罪卡因此进入弃牌堆/消耗堆，改为放入抽牌堆顶部；
/// 并按当前牌组内原罪卡数量，随机触发等量次的原罪卡卡面效果。
/// 升级：费用 -1。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class XunHuanFaDian() : ManosabaCardTemplate(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<XunHuanFaDianPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<XunHuanFaDianPower>(); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await PowerCmd.Apply<XunHuanFaDianPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}