using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class AnanlinVanity : LinAncientCurseCard
{
    public AnanlinVanity() : base(1) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DrawCount", 1m),
        new DynamicVar("DiscardCount", 2m),
    ];

    protected override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, this)) return;

        var drawCount = (int)DynamicVars["DrawCount"].BaseValue;
        if (drawCount > 0)
            await CardPileCmd.Draw(choiceContext, drawCount, Owner);

        var discardWanted = (int)DynamicVars["DiscardCount"].BaseValue;
        if (discardWanted <= 0) return;

        var selectable = PileType.Hand.GetPile(Owner).Cards.Where(c => !ReferenceEquals(c, this)).ToList();
        if (selectable.Count == 0) return;

        var count = System.Math.Min(discardWanted, selectable.Count);
        var toDiscard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, count, count),
            c => !ReferenceEquals(c, this),
            this)).ToList();

        foreach (var c in toDiscard)
            await CardPileCmd.Add(c, PileType.Discard);
    }
}
