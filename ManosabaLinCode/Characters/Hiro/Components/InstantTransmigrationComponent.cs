using MinionLib.Component;
using MinionLib.Component.Core;
using ManosabaLin.Characters.Common.Components.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Hiro.Components;

/// <summary>
///     即刻轮回 - 由「即刻轮回」牌给「手牌原卡」与「进队友抽牌堆的复制品」同时加上的组件。
///     <para>
///         当带本组件的卡被打出（手动，非自动）时，自动再打出<b>其他友方</b>的
///         抽牌堆 / 弃牌堆 / 手牌中至多 2 张<b>同名且同样带本组件</b>的卡。
///     </para>
///     <para>
///         这些追加打出都归属「打出第一张即刻轮回卡的人」：实现上为触发者各造一份归属他的副本
///         并自动打出（引擎的自动打出按卡牌归属者记玩家，这样才符合「算他打的」）。
///     </para>
/// </summary>
public sealed partial class InstantTransmigrationComponent : KeywordLikeComponent
{
    private const int MaxEchoes = 2;

    public override async Task AfterCardPlayedPostfix(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var card = Card;
        if (card is null || !ReferenceEquals(cardPlay.Card, card)) return;

        // 自动打出不再连锁，避免无限循环
        if (cardPlay.IsAutoPlay) return;
        if (card.Owner is not { } trigger) return;

        var combatState = trigger.Creature.CombatState;
        if (combatState is null) return;

        var echoes = new List<CardModel>();
        foreach (var other in combatState.Players)
        {
            if (other == trigger) continue;
            if (other.Creature.Side != trigger.Creature.Side || !other.Creature.IsAlive) continue;

            foreach (var pileType in new[] { PileType.Draw, PileType.Discard, PileType.Hand })
            {
                echoes.AddRange(pileType.GetPile(other).Cards.Where(c =>
                    c.Id == card.Id
                    && c is ComponentsCardModel components
                    && components.GetComponent<InstantTransmigrationComponent>() is not null));
            }

            if (echoes.Count >= MaxEchoes) break;
        }

        var toPlay = echoes.Take(MaxEchoes).ToList();
        if (toPlay.Count == 0) return;

        var isFirst = true;
        foreach (var echo in toPlay)
        {
            var copy = combatState.CreateCard(echo.CanonicalInstance, trigger);
            copy.SetToFreeThisTurn();

            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, trigger);
            await CardCmd.AutoPlay(choiceContext, copy, cardPlay.Target, skipCardPileVisuals: !isFirst);
            await CardPileCmd.RemoveFromCombat(copy);

            isFirst = false;
            await Cmd.Wait(0.1f);
        }
    }
}
