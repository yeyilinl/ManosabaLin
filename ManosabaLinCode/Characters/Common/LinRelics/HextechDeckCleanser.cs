using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物 1：每回合自动抽牌前（<c>BeforeHandDraw</c>），可以选择「所有牌」（手牌 + 抽牌堆 + 弃牌堆 + 消耗堆）里的任意张牌进入弃牌堆。
///     <para>
///         实现要点：<c>CardSelectCmd.FromSimpleGrid</c> 本身带 <c>PlayerChoiceSynchronizer</c> 同步
///         ⇒ 联机安全；选择结果只用于牌堆搬移，不写任何机器本地状态。
///         <c>MinSelect = 0</c> ⇒ 玩家可以直接确认不选（<c>RequireManualConfirmation</c> 为 true 时 0 张也能确认）。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechDeckCleanser : ManosabaRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner) return;

        var all = new List<CardModel>();
        foreach (var pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust })
        {
            var pile = pileType.GetPile(Owner);
            if (pile is null) continue;
            all.AddRange(pile.Cards);
        }

        if (all.Count == 0) return;

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, all.Count);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, all, Owner, prefs)).ToArray();
        if (selected.Length == 0) return;

        Flash();
        await CardPileCmd.Add(selected, PileType.Discard, CardPilePosition.Random, null, false);
    }
}
