using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 忤逆之证（2 费能力・罕见）：
/// 获得 [忤逆之证]：每次对原罪自惩时 +1 层，每层获得 1 点力量。
/// 升级：费用 -1。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class NiNiZhiZheng() : ManosabaCardTemplate(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NiNiZhiZhengPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<NiNiZhiZhengPower>(); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await PowerCmd.Apply<NiNiZhiZhengPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}