using ManosabaLin.Characters.Ananlin.Powers;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinConfirmDoorGap()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self),
        IAnanlinPeaceOfMindSpecialCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(2m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AnanlinPeaceOfMindPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var candidates = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card != this && AnanlinCardHelpers.IsPlayableCombatCard(card))
            .ToList();
        if (candidates.Count == 0) return;

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).FirstOrDefault();
        if (selected is null) return;

        if (this.PeaceOfMindAmount() <= 0)
            await this.GainPeaceOfMind(choiceContext);

        var bonusBlock = this.PeaceOfMindAmount() * DynamicVars.Block.IntValue;
        var power = await PowerCmd.Apply<AnanlinDoorGapConfirmationPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
        power?.Track(selected, bonusBlock);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：该牌打出时额外获得的格挡 2 → 4（不再减费）。
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
