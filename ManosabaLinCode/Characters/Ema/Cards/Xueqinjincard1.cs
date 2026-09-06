using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using System;

namespace ManosabaLin.Characters.Ema.Cards;

[RegisterCard(typeof(EmalinCardPool))]
public sealed class Xueqinjincard1 : ManosabaCardTemplate
{
    internal static readonly Type[] RandomEstrangementCardTypes =
    [
        typeof(BalloonFragments),
        typeof(StabbingBlade),
        typeof(ShatteredResonance),
        typeof(WitchCleansing),
        typeof(ChainedTrust),
        typeof(PawnRealization),
        typeof(NoahEstrangement),
        typeof(MargaretEstrangement),
        typeof(CocoEstrangement),
        typeof(AnnEstrangement),
        typeof(Hiroshuyuancard),
        typeof(Lyshuyuan),
    ];

    public Xueqinjincard1() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<BondPower>();
            yield return HoverTipFactory.FromCard<Xueqinjincard2>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;
        var owner = source.Owner;
        var creature = owner.Creature;
        var bond = creature.GetPower<BondPower>();

        // 如果亲近 > 7，本卡打出后回到手牌并变形为 Xueqinjincard2，跳过所有效果。
        // 注意：不能在打出中途直接 Transform——替代卡会卡在 Play 牌堆、原卡被移除，
        // 导致战斗状态损坏并软锁死；变形统一放到打出结束（AfterCardChangedPilesLate，Play→Hand）执行。
        if (bond != null && bond.Affinity > 7)
            return;

        // 疏远 +1
        if (bond != null) bond.Estrangement++;

        // 造成 3 点伤害
        if (cardPlay.Target != null)
        {
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, 3m, ValueProp.Move, this, cardPlay);
        }

        // 选择一张手牌变成随机疏远牌
        var handPile = PileType.Hand.GetPile(owner);
        var handCards = handPile.Cards.ToList();
        if (handCards.Count == 0) return;

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1, 1);
        var selected = await CardSelectCmd.FromHand(
            choiceContext, owner, prefs, null, this);
        var picked = selected.FirstOrDefault();
        if (picked == null) return;

        var combatState = source.CombatState;
        if (combatState == null) return;

        var rng = owner.RunState.Rng.CombatCardSelection;
        var chosenType = rng.NextItem(RandomEstrangementCardTypes);
        if (chosenType == null) return;

        var createCardMethod = typeof(ICombatState).GetMethod("CreateCard", [typeof(Player)]);
        if (createCardMethod == null) return;

        var genericMethod = createCardMethod.MakeGenericMethod(chosenType);
        if (genericMethod.Invoke(combatState, [owner]) is not CardModel estrangementCard) return;

        estrangementCard.AddKeyword(CardKeyword.Retain);
        await CardCmd.Transform(picked, estrangementCard);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override CardLocation GetResultLocationForCardPlayC()
    {
        var bond = Owner.Creature.GetPower<BondPower>();
        if (bond != null && bond.Affinity > 7)
            return new CardLocation(Owner, PileType.Hand, CardPilePosition.Bottom);

        return base.GetResultLocationForCardPlayC();
    }

    protected override async Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? source,
        ComponentContext componentContext)
    {
        if (card != this || oldPileType != PileType.Play || card.Pile?.Type != PileType.Hand) return;

        var bond = Owner.Creature.GetPower<BondPower>();
        if (bond == null || bond.Affinity <= 7) return;

        await CardCmd.TransformTo<Xueqinjincard2>(this);
    }
}
