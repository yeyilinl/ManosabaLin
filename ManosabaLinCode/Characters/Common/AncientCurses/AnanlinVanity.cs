using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common.AncientCurses.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 夏目安安的虚妄：抽到此卡时抽 1 张并弃 2 张手牌（可弃自己）；
/// 回合结束时无论在哪（含消耗堆）都重新加入抽牌堆。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class AnanlinVanity : LinAncientCurseCard
{
    protected override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, this)) return;

        await CardPileCmd.Draw(choiceContext, 1, Owner);

        var handCount = PileType.Hand.GetPile(Owner).Cards.Count;
        if (handCount == 0) return;

        var count = System.Math.Min(2, handCount);
        var toDiscard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, count, count),
            null,
            this)).ToList();

        foreach (var c in toDiscard)
            await CardPileCmd.Add(c, PileType.Discard);
    }

    protected override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants,
        ComponentContext componentContext)
    {
        if (side != Owner.Creature.Side) return;
        if (Owner.Creature.IsDead) return;
        if (Pile?.Type != PileType.Draw)
            await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Random);
    }
}
