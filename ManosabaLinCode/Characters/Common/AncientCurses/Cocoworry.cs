using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 泽度可可的忧泯：抽到此卡时获得 1 点能量，并将手牌中最高稀有度的卡与
/// 抽牌堆 / 弃牌堆 / 消耗堆中低一等稀有度的卡交换位置。
/// 手牌全是同稀有度或只有 1 张时随机换一张；若所有牌堆的卡稀有度都一样，
/// 则换成基础稀有度；连基础卡都没有就不生效（能量照常获得）。
/// </summary>
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
