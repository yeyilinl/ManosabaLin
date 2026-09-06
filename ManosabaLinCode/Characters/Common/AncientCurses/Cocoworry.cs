using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class Cocoworry : LinAncientCurseCard
{

    protected override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, this)) return;

        await PlayerCmd.GainEnergy(1m, Owner);
        await TrySwapRarity();
    }

    private async Task TrySwapRarity()
    {
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (hand.Count == 0) return;

        var pools = PileType.Draw.GetPile(Owner).Cards
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .Concat(PileType.Exhaust.GetPile(Owner).Cards)
            .ToList();
        if (pools.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;

        CardModel handCard;
        CardModel? poolCard;

        var handRanks = hand.Select(c => (int)c.Rarity).Distinct().ToList();
        if (handRanks.Count > 1)
        {
            var maxRank = handRanks.Max();
            handCard = rng.NextItem(hand.Where(c => (int)c.Rarity == maxRank).ToList())!;

            var lower = pools.Where(c => (int)c.Rarity == maxRank - 1).ToList();
            if (lower.Count > 0)
            {
                poolCard = rng.NextItem(lower);
            }
            else
            {
                var basics = pools.Where(c => c.Rarity == CardRarity.Basic).ToList();
                if (basics.Count == 0) return;
                poolCard = rng.NextItem(basics);
            }
        }
        else
        {
            handCard = rng.NextItem(hand)!;

            var poolRanks = pools.Select(c => (int)c.Rarity).Distinct().ToList();
            if (poolRanks.Count <= 1)
            {
                var basics = pools.Where(c => c.Rarity == CardRarity.Basic).ToList();
                if (basics.Count == 0) return;
                poolCard = rng.NextItem(basics);
            }
            else
            {
                poolCard = rng.NextItem(pools);
            }
        }

        if (poolCard is null) return;

        var poolPile = poolCard.Pile?.Type ?? PileType.Discard;
        await CardPileCmd.Add(poolCard, PileType.Hand);
        await CardPileCmd.Add(handCard, poolPile);
    }
}
