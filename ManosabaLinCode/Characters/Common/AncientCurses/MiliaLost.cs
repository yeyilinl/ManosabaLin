using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 佐伯米莉亚的迷失：抽到此卡时，将弃牌堆的所有卡加入抽牌堆并打乱你的抽牌堆。
/// 弃牌堆为空时仍触发洗牌。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class MiliaLost : LinAncientCurseCard
{
    protected override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, this)) return;

        var discard = PileType.Discard.GetPile(Owner).Cards.ToList();
        if (discard.Count > 0)
            await CardPileCmd.Add(discard, PileType.Draw, CardPilePosition.Random);

        await CardPileCmd.Shuffle(choiceContext, Owner);
    }
}
