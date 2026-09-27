using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     不灭之炎（1 费 能力・稀有）：
///     当你的生命低于 15%（升级 30%）时，若你受到攻击，使攻击者获得 1 层【不灭之炎】。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class ImmortalFlame()
    : ManosabaCardTemplate(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    private const int BaseThresholdPercent = 15;
    private const int UpgradedThresholdPercent = 30;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("ThresholdPercent", BaseThresholdPercent)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<ImmortalFlamePower>();
            yield return HoverTipFactory.FromPower<ImmortalFlameBrandPower>();
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        var threshold = IsUpgraded ? UpgradedThresholdPercent : BaseThresholdPercent;
        await PowerCmd.Apply<ImmortalFlamePower>(
            choiceContext, owner.Creature, threshold, owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["ThresholdPercent"].BaseValue = UpgradedThresholdPercent;
    }
}
