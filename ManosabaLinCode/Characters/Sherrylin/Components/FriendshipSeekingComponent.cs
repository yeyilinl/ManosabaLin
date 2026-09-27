using ManosabaLin.Characters.Common.Components.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Component.Core;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Sherrylin.Components;

/// <summary>
///     「友谊的探寻」赠予的标记组件：持有者打出这张牌时，赠予者（雪莉）跟着打出一次同样的牌。
///     <para>
///         标记与监听逻辑都在组件内部（<b>不再使用</b> FriendshipSeekingPower），
///         因此赠礼牌离开战斗 / 换手都只跟着组件本身走。
///     </para>
///     <para>
///         雪莉回响的那一份按正常规则结算，所以会进入<b>雪莉自己的消耗堆</b>
///         —— 赠礼牌自带【消耗】，结算后由引擎送进消耗堆，
///         这里不能再调 <c>CardPileCmd.RemoveFromCombat</c>（那会把它从消耗堆里抹掉）。
///     </para>
/// </summary>
public sealed partial class FriendshipSeekingComponent : KeywordLikeComponent
{
    /// <summary>
    ///     赠予者的网络 ID。组件状态要参与序列化与校验，所以存 <c>NetId</c> 而不是 <see cref="Player" /> 引用。
    /// </summary>
    [ComponentState] private ulong GiverNetId { get; set; }

    public static FriendshipSeekingComponent Create(Player giver)
    {
        return new FriendshipSeekingComponent { GiverNetId = giver.NetId };
    }

    public override async Task OnPlayPostfix(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Card?.Owner is not { } holder) return;

        // 雪莉自己打出赠礼牌时不再回响，避免自我循环。
        if (holder.NetId == GiverNetId) return;

        if (holder.Creature is not { IsAlive: true } holderCreature) return;
        if (holderCreature.CombatState is not { } combatState) return;

        var giver = combatState.Players.FirstOrDefault(player => player.NetId == GiverNetId);
        if (giver is null || giver == holder) return;
        if (giver.Creature is not { IsAlive: true } giverCreature) return;

        // 只回应同一阵营的队友。
        if (giverCreature.Side != holderCreature.Side) return;

        var echo = combatState.CreateCard(cardPlay.Card.CanonicalInstance, giver);
        echo.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(echo, PileType.Hand, giver);
        await CardCmd.AutoPlay(choiceContext, echo, PickTarget(giver, echo));
    }

    private static Creature? PickTarget(Player giver, CardModel card)
    {
        if (giver.Creature.CombatState is not { } combatState) return null;

        if (card.TargetType is TargetType.AnyEnemy or TargetType.RandomEnemy or TargetType.AllEnemies)
        {
            var enemies = combatState.GetOpponentsOf(giver.Creature).Where(static enemy => enemy.IsAlive).ToList();
            return enemies.Count == 0 ? null : giver.RunState.Rng.CombatTargets.NextItem(enemies);
        }

        if (card.TargetType == TargetType.AnyPlayer)
        {
            var players = combatState.Creatures
                .Where(creature => creature.Side == giver.Creature.Side && creature.IsAlive)
                .ToList();
            return players.Count == 0 ? null : giver.RunState.Rng.CombatTargets.NextItem(players);
        }

        return null;
    }
}
