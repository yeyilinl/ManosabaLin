using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;

namespace ManosabaLin.Characters.Yalisalin.Components;

/// <summary>
///     余火选卡界面里「<b>本次语境下打不出</b>」的牌。
///     <para>
///         这些牌仍然显示在选项里，但要像手牌里条件不满足的卡一样在能量处显示 ×
///         （临时无法打出），而不是被静默替换成另一张 —— 玩家选中它们就按「打不出」结算。
///     </para>
///     <para>
///         界面显示由 <c>ManosabaLin.Patches.YalisalinFireComponentUnplayablePatch</c> 读这里；
///     只影响渲染，不参与 run state，也不参与联机校验和。
///     </para>
/// </summary>
internal static class YalisalinFireComponentUnplayableRegistry
{
    private static readonly HashSet<CardModel> Cards = new(ReferenceEqualityComparer.Instance);

    public static bool Contains(CardModel card)
    {
        return Cards.Contains(card);
    }

    /// <summary>在 <c>using</c> 作用域内把这些牌标成「临时无法打出」，离开作用域自动恢复。</summary>
    public static IDisposable Begin(IEnumerable<CardModel> cards)
    {
        var added = new List<CardModel>();
        foreach (var card in cards)
        {
            if (card == null) continue;
            if (Cards.Add(card))
                added.Add(card);
        }

        return new Scope(added);
    }

    private sealed class Scope(List<CardModel> added) : IDisposable
    {
        public void Dispose()
        {
            foreach (var card in added)
                Cards.Remove(card);

            added.Clear();
        }
    }
}
