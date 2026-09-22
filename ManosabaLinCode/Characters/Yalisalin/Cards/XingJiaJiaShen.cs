using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 刑枷加身（2 费能力・罕见）：
/// 获得 [刑枷]：每回合第一张原罪诅咒卡牌打出后，减一费并添加消耗，然后加入手牌。
/// 升级：作用于每回合前两张。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class XingJiaJiaShen() : ManosabaCardTemplate(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<XingJiaJiaShenPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<XingJiaJiaShenPower>(); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var power = await PowerCmd.Apply<XingJiaJiaShenPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        if (power is not null)
            power.CountPerTurn = IsUpgraded ? 2 : 1;
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：作用于每回合前两张（在 OnPlay 时按 IsUpgraded 设置 CountPerTurn）
    }
}