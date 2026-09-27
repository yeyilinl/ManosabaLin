using MinionLib.Component;
using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Sherrylin.Components;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;

namespace ManosabaLin.Characters.Sherrylin.Cards;

/// <summary>
///     友谊的探寻 - 1 费技能，罕见，多人专属。
///     <para>
///         选择 1 名队友：其随机获得 1 张雪莉卡池中带【消耗】的牌，该牌可以免费打出一次；
///         当该队友打出这张牌时，雪莉也会跟着打出一次同样的牌。
///     </para>
///     <para>升级后赠予的是升级版的牌。</para>
/// </summary>
[RegisterCard(typeof(SherrylinCardPool))]
public sealed class FriendshipSeeking() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        if (cardPlay.Target?.Player is not { } teammate) return;
        if (teammate == Owner || !teammate.Creature.IsAlive) return;

        // 雪莉卡池中带【消耗】的牌
        var candidates = ModelDb.CardPool<SherrylinCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(static c => c.Keywords.Contains(CardKeyword.Exhaust))
            .ToList();
        if (candidates.Count == 0) return;

        var template = Owner.RunState.Rng.CombatCardGeneration.NextItem(candidates);
        if (template is null) return;

        var gift = CombatState.CreateCard(template, teammate);
        if (IsUpgraded)
            CardCmd.Upgrade(gift);

        // 可以免费打出一次（带消耗，打完即进消耗堆）
        gift.SetToFreeThisCombat();

        // 标记 + 监听都由组件承担（不再施加 FriendshipSeekingPower）：
        // 队友打出带此组件的牌时，雪莉跟着打出一次同样的牌，那份牌会正常进入雪莉的消耗堆。
        if (gift is ComponentsCardModel components)
            components.AddComponent(FriendshipSeekingComponent.Create(Owner));

        await CardPileCmd.AddGeneratedCardToCombat(gift, PileType.Hand, teammate);
    }
}
