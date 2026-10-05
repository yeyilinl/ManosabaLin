using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Sherrylin.Cards.Emotions;
using ManosabaLin.Characters.Sherrylin.Powers;
using ManosabaLin.Characters.Sherrylin.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Sherrylin.Cards;

[RegisterCard(typeof(SherrylinCardPool))]
public sealed class TheFool : ManosabaCardTemplate
{
    private const string EffectHoverLocEntry = "MANOSABA_LIN_CARD_THE_FOOL_EFFECT";

    public TheFool() : base(4, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return CardEffectHoverTipFactory.FromCard(this, EffectHoverLocEntry);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WithPower>(100m),
        new PowerVar<RitualCeremonyPower>(1m),
        new PowerVar<EmotionPower>(3m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;
        var owner = source.Owner;
        var combatState = source.CombatState;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        // 1. 获得100层魔女化
        await PowerCmd.Apply<WithPower>(choiceContext, owner.Creature, 100, owner.Creature, source, false);

        // 2. 获得1层魔女仪式
        await PowerCmd.Apply<RitualCeremonyPower>(choiceContext, owner.Creature, 1, owner.Creature, source, false);

        // 3. 立刻进行翻案（消耗堆↔弃牌堆互换）
        var exhaustPile = PileType.Exhaust.GetPile(owner);
        var discardPile = PileType.Discard.GetPile(owner);
        var exhaustCards = exhaustPile.Cards.ToList();
        var discardCards = discardPile.Cards.ToList();

        var relic = owner.Relics.OfType<MagnifyingGlass>().FirstOrDefault();
        if (relic != null)
        {
            relic.HasTriggeredThisCombat = true;
            relic.CaseReversalCountThisTurn++;
        }

        foreach (var exhaustCard in exhaustCards)
        {
            await CardPileCmd.Add(exhaustCard, PileType.Discard, CardPilePosition.Random, skipVisuals: true);
            // 与遗物「正规翻案」口径一致：每张移入弃牌堆的消耗牌 +2 层【橘雪莉的魔法】
            await PowerCmd.Apply<XlmPower>(choiceContext, owner.Creature, 2, owner.Creature, source, false);
        }

        foreach (var discardCard in discardCards)
        {
            await CardPileCmd.Add(discardCard, PileType.Exhaust, CardPilePosition.Random, skipVisuals: true);
        }

        // 4. 造成「本回合【翻案】次数 × 3」的伤害，并获得等量格挡
        //    （伤害会被【橘雪莉的魔法】XlmPower 的额外伤害加成）
        var reversalCount = relic?.CaseReversalCountThisTurn ?? 0;
        var reversalDamage = reversalCount * 3;
        if (reversalDamage > 0 && combatState != null)
        {
            foreach (var enemy in combatState.Enemies.Where(static e => e.IsAlive))
                await CreatureCmd.Damage(choiceContext, enemy, reversalDamage, ValueProp.Unpowered, source, cardPlay);

            await CreatureCmd.GainBlock(owner.Creature, reversalDamage, ValueProp.Move, cardPlay);
        }

        // 5. 获得3层【复杂的情绪】
        await PowerCmd.Apply<EmotionPower>(choiceContext, owner.Creature, 3, owner.Creature, source, false);

        // 6. 获得1张当前缺少的【无助】或【好奇】，和2张当前缺少的【复杂情绪】
        //    （「缺少」= 案卷堆 / 额外牌堆里没有）
        if (combatState != null)
        {
            var caseFilePile = MainFile.CaseFilePile.GetPile(owner);
            var rng = owner.RunState.Rng.CombatCardGeneration;

            // 6a. 1 张【无助】/【好奇】
            var hasHelpless = caseFilePile.Cards.Any(static c => c is EmotionHelplessness);
            var hasCuriosity = caseFilePile.Cards.Any(static c => c is EmotionCuriosity);

            if (!(hasHelpless && hasCuriosity))
            {
                CardModel? missingCard;
                if (!hasHelpless && !hasCuriosity)
                {
                    missingCard = rng.NextInt(2) == 0
                        ? combatState.CreateCard<EmotionHelplessness>(owner)
                        : combatState.CreateCard<EmotionCuriosity>(owner);
                }
                else
                {
                    missingCard = hasHelpless
                        ? combatState.CreateCard<EmotionCuriosity>(owner)
                        : combatState.CreateCard<EmotionHelplessness>(owner);
                }

                if (missingCard != null)
                    await CaseFilePileHelper.AddToCaseFilePile(
                        missingCard, owner, CardPilePosition.Top, choiceContext);
            }

            // 6b. 2 张【复杂情绪】（忧郁/烦躁恐惧/荒芜/恐怖厌恶/兴高采烈），
            //     同样只从案卷堆里缺少的那几种里随机取。
            var missingComplex = new List<CardModel>();
            if (!caseFilePile.Cards.Any(static c => c is EmotionMelancholy))
                missingComplex.Add(combatState.CreateCard<EmotionMelancholy>(owner)!);
            if (!caseFilePile.Cards.Any(static c => c is EmotionIrritatedFear))
                missingComplex.Add(combatState.CreateCard<EmotionIrritatedFear>(owner)!);
            if (!caseFilePile.Cards.Any(static c => c is EmotionDesolate))
                missingComplex.Add(combatState.CreateCard<EmotionDesolate>(owner)!);
            if (!caseFilePile.Cards.Any(static c => c is EmotionHorrorDisgust))
                missingComplex.Add(combatState.CreateCard<EmotionHorrorDisgust>(owner)!);
            if (!caseFilePile.Cards.Any(static c => c is EmotionElation))
                missingComplex.Add(combatState.CreateCard<EmotionElation>(owner)!);

            foreach (var card in missingComplex.OrderBy(_ => rng.NextFloat()).Take(2))
                await CaseFilePileHelper.AddToCaseFilePile(
                    card, owner, CardPilePosition.Top, choiceContext);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
