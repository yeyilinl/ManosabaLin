using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class NayukaJealousy : LinAncientCurseCard
{

    private static readonly HashSet<CardModel> Redirecting = [];

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("Chance", 10m); }
    }

    protected override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? source,
        ComponentContext componentContext)
    {
        if (Pile is { Type: PileType.Hand }) return;
        if (Redirecting.Contains(card)) return;

        var newPile = card.Pile?.Type;
        if (newPile is not (PileType.Discard or PileType.Draw or PileType.Exhaust)) return;
        if (card.Owner != Owner) return;

        var chance = (float)(DynamicVars["Chance"].BaseValue / 100m);
        var rng = Owner.RunState.Rng.CombatCardGeneration;
        if (rng.NextFloat() >= chance) return;

        var target = rng.NextFloat() < 0.5f ? PileType.Discard : PileType.Exhaust;
        if (target == newPile)
            target = newPile == PileType.Discard ? PileType.Exhaust : PileType.Discard;

        Redirecting.Add(card);
        try
        {
            await CardPileCmd.Add(card, target, CardPilePosition.Random);
        }
        finally
        {
            Redirecting.Remove(card);
        }
    }
}
