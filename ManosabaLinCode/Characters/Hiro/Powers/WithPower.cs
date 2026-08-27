using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ananlin.Cards;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Ema.Vfx;
using ManosabaLin.Characters.Hiro.Cards;
using ManosabaLin.Characters.Sherrylin.Cards;
using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Emalin;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using AnanlinCharacter = ManosabaLin.Characters.Ananlin.Ananlin;

namespace ManosabaLin.Characters.Hiro.Powers;

[RegisterPower]
public sealed class WithPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner) return 1m;
        if (props.HasFlag(ValueProp.Unpowered)) return 1m;
        return 1m + Amount / 200m;
    }

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner) return 0m;
        if (props.HasFlag(ValueProp.Unpowered)) return 0m;
        return Amount / 50;
    }

    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
    {
        if (Amount < 300 || card.Owner.Creature != Owner || card.Type != CardType.Skill)
            return cardLocation;
        return new CardLocation(cardLocation.player, PileType.Exhaust, cardLocation.position);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var source = this;
        if (cardPlay.Card.Owner.Creature != source.Owner) return;

        if (source.Amount >= 200 && !source.ShouldIgnoreWitchificationHpLoss())
            if (cardPlay.Card.Type == CardType.Skill || cardPlay.Card.Type == CardType.Power)
            {
                var hpLoss = 1m;
                if (source.Amount >= 300)
                    hpLoss += source.Amount / 100m;

                await CreatureCmd.Damage(
                    context,
                    source.Owner,
                    hpLoss,
                    ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                    source.Owner,
                    cardPlay.Card,
                    null);
            }

        if (source.Amount >= 300 && cardPlay.Card.Type == CardType.Attack)
            await CreatureCmd.Heal(source.Owner, 3m);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        var source = this;
        if (source.ShouldIgnoreWitchificationHpLoss()) return;

        if (source.Amount >= 200)
            await CreatureCmd.Damage(
                choiceContext,
                source.Owner,
                13m,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                source.Owner,
                null,
                null);
    }

    private bool ShouldIgnoreWitchificationHpLoss()
    {
        return Owner.GetPower<Powerthreethree>() != null;
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power != this) return;
        await CheckAndGiveCharacterReward();

        // 艾玛（Emalin）魔女化满 100：挂上额外翅膀特效。
        // 层数回落 < 100 不移除，只有能力被移除（战斗结束）才清理，
        // 从而保证每个战斗房间独立重置。
        if (Amount >= 100 && Owner?.Player?.Character is Emalin.Emalin)
            EnsureWings();
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        RemoveWings(oldOwner);
    }

    /// <summary>翅膀特效场景路径（自 MonosabaVfx 迁入）。</summary>
    private const string WingScenePath = "res://ManosabaLin/scenes/Emalin/ema_wing.tscn";

    /// <summary>翅膀节点名，用于查找/清理。</summary>
    private const string WingNodeName = "EmaWitchWingsVfx";

    /// <summary>把翅膀特效挂到 Owner 的 BackVfxContainer（画在所有角色立绘后面的官方容器）。</summary>
    private void EnsureWings()
    {
        if (Owner == null) return;
        var container = Owner.GetBackVfxContainer();
        if (container == null) return;

        if (container.GetNodeOrNull<EmaFormVfx>(WingNodeName) != null) return;

        var scene = PreloadManager.Cache.GetScene(WingScenePath);
        if (scene == null)
        {
            MainFile.Logger.Warn($"[WithPower] 翅膀场景未找到: {WingScenePath}");
            return;
        }

        var vfx = scene.Instantiate<EmaFormVfx>();
        if (vfx == null) return;

        vfx.Name = WingNodeName;
        vfx.Target = Owner;
        container.AddChildSafely(vfx);
        MainFile.Logger.Info("[WithPower] 艾玛魔女化满 100，翅膀特效已挂载到 BackVfxContainer");
    }

    /// <summary>从指定生物身上移除翅膀特效（能力移除/战斗结束时调用）。</summary>
    private static void RemoveWings(Creature owner)
    {
        if (owner == null) return;
        var container = owner.GetBackVfxContainer();
        if (container == null) return;

        container.GetNodeOrNull<EmaFormVfx>(WingNodeName)?.QueueFreeSafely();
    }

    private async Task CheckAndGiveCharacterReward()
    {
        if (Amount < 100) return;
        if (Owner?.Player == null) return;

        var characterType = Owner.Player.Character.GetType();

        if (characterType == typeof(Hiro))
            await GiveDeathRewind();
        else if (characterType == typeof(Emalin.Emalin))
            await GiveWitchKillerCard();
        else if (characterType == typeof(Sherrylin.Sherrylin))
            await GiveSuperStrength();
        else if (characterType == typeof(AnanlinCharacter))
            await GiveBrainwash();
    }

    private async Task GiveDeathRewind()
    {
        var deck = Owner.Player.Deck;
        if (deck.Cards.Any(c => c is DeathRewind)) return;

        var cardModel = ModelDb.GetById<CardModel>(ModelDb.GetId<DeathRewind>());
        if (cardModel == null) return;

        var permanentCard = Owner.Player.RunState.CreateCard(cardModel, Owner.Player);
        await CardPileCmd.Add(permanentCard, PileType.Deck);
        CardCmd.PreviewCardPileAdd(new CardPileAddResult { success = true, cardAdded = permanentCard });

        if (Owner.CombatState != null)
        {
            var tempCard = Owner.CombatState.CreateCard(cardModel, Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Hand, Owner.Player);
        }
    }

    private async Task GiveWitchKillerCard()
    {
        var deck = Owner.Player.Deck;
        if (deck.Cards.Any(c => c is EmaWitchKillerCard)) return;

        var cardModel = ModelDb.GetById<CardModel>(ModelDb.GetId<EmaWitchKillerCard>());
        if (cardModel == null) return;

        var permanentCard = Owner.Player.RunState.CreateCard(cardModel, Owner.Player);
        await CardPileCmd.Add(permanentCard, PileType.Deck);
        CardCmd.PreviewCardPileAdd(new CardPileAddResult { success = true, cardAdded = permanentCard });

        if (Owner.CombatState != null)
        {
            var tempCard = Owner.CombatState.CreateCard(cardModel, Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Hand, Owner.Player);
        }
    }

    private async Task GiveSuperStrength()
    {
        var deck = Owner.Player.Deck;
        if (deck.Cards.Any(c => c is SuperStrength)) return;

        var cardModel = ModelDb.GetById<CardModel>(ModelDb.GetId<SuperStrength>());
        if (cardModel == null) return;

        var permanentCard = Owner.Player.RunState.CreateCard(cardModel, Owner.Player);
        await CardPileCmd.Add(permanentCard, PileType.Deck);
        CardCmd.PreviewCardPileAdd(new CardPileAddResult { success = true, cardAdded = permanentCard });

        if (Owner.CombatState != null)
        {
            var tempCard = Owner.CombatState.CreateCard(cardModel, Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Hand, Owner.Player);
        }
    }

    private async Task GiveBrainwash()
    {
        var deck = Owner.Player.Deck;
        if (deck.Cards.Any(c => c is AnanlinBrainwash)) return;

        var cardModel = ModelDb.GetById<CardModel>(ModelDb.GetId<AnanlinBrainwash>());
        if (cardModel == null) return;

        var permanentCard = Owner.Player.RunState.CreateCard(cardModel, Owner.Player);
        await CardPileCmd.Add(permanentCard, PileType.Deck);
        CardCmd.PreviewCardPileAdd(new CardPileAddResult { success = true, cardAdded = permanentCard });

        if (Owner.CombatState != null)
        {
            var tempCard = Owner.CombatState.CreateCard(cardModel, Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Hand, Owner.Player);
        }
    }
}
