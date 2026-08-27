using ManosabaLin.Characters.Ananlin.Powers;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinFakeDeathAct()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get { yield return CardKeyword.Exhaust; }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<AnanlinPeaceOfMindPower>(),
        HoverTipFactory.FromPower<CrimsonbutterflyPower>(),
        HoverTipFactory.FromCard<BlankPage>(),
        HoverTipFactory.FromCard<MarginPage>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var lostPeace = await this.LosePeaceOfMind(choiceContext, int.MaxValue);

        await PowerCmd.Apply<CrimsonbutterflyPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        var power = await PowerCmd.Apply<AnanlinFakeDeathActPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
        if (power is null) return;

        power.LostPeace = lostPeace;
        power.RewardMarginPagesOnTrigger = IsUpgraded;
    }
}
