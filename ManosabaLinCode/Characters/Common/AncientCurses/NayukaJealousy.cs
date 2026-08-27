using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 黑部奈叶香的猜忌：每当你的手牌进入弃牌堆 / 抽牌堆 / 消耗牌堆时，有 10% 概率
/// 将其改道为随机进入消耗堆或弃牌堆。改道后的进入不再次触发判定。不在手牌时生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class NayukaJealousy : LinAncientCurseCard
{
    /// <summary>正在被改道的卡，防止改道后的进入再次触发判定。</summary>
    private static readonly HashSet<CardModel> Redirecting = [];

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

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        if (rng.NextFloat() >= 0.1f) return;

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
