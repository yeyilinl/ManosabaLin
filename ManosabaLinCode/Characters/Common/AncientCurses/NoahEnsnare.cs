using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class NoahEnsnare : LinAncientCurseCard
{
    public NoahEnsnare() : base(1) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("PaintCount", 1m); }
    }

    protected override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, this)) return;
        if (CombatState is not { } combatState) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var count = (int)DynamicVars["PaintCount"].BaseValue;
        if (count < 1) count = 1;

        for (var i = 0; i < count; i++)
        {
            var paint = combatState.CreateCard<SelfControlledPaint>(Owner);
            var paintPile = rng.NextFloat() < 0.5f ? PileType.Draw : PileType.Discard;
            await CardPileCmd.AddGeneratedCardToCombat(paint, paintPile, Owner, CardPilePosition.Random);
        }
    }
}
