using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;

namespace ManosabaLin.Characters.Ema.Powers;

[RegisterPower]
public class InvestigationPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var lookCount = Amount;
        if (lookCount <= 0) return;

        // CardPile 里 index 0 就是堆顶（MoveToTopInternal 用 _cards.Insert(0, ...)），
        // 所以取列表开头才是「抽牌堆顶部」；Last() 取到的是堆底。
        var topCards = PileType.Draw.GetPile(player).Cards
            .Take(lookCount)
            .ToList();
        if (topCards.Count == 0) return;

        // 「任意张」：最小值 0（可以一张都不弃），最大值 = 检视的张数
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.DiscardSelectionPrompt, 0, topCards.Count);
        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext, topCards, player, prefs);

        // 只有被选中的牌进弃牌堆，没选的留在抽牌堆原位
        if (selected == null) return;
        foreach (var card in selected)
        {
            await CardPileCmd.Add(card, PileType.Discard);
        }
    }
}
