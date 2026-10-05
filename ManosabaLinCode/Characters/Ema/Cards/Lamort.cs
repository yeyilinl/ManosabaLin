using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Emalin.Enchantments;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Combat;
using System;
using System.Reflection;
using ManosabaLin.Characters.Hiro.Powers;

namespace ManosabaLin.Characters.Ema.Cards;

[RegisterCard(typeof(EmalinCardPool))]
public sealed class Lamort : ManosabaCardTemplate
{
    private const string EffectHoverLocEntry = "MANOSABA_LIN_CARD_LAMORT_EFFECT";

    public Lamort() : base(3, CardType.Power, CardRarity.Ancient, TargetType.Self)
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
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;
        var owner = source.Owner;
        var creature = owner.Creature;
        var combatState = source.CombatState;

        await CreatureCmd.TriggerAnim(creature, "Cast", owner.Character.CastAnimDelay);

        await PowerCmd.Apply<WithPower>(
            choiceContext, creature,
            source.DynamicVars["WithPower"].BaseValue,
            creature, source, false);

        var allCreatures = combatState.Allies
            .Concat(combatState.Enemies)
            .Where(c => c.IsAlive)
            .ToList();

        foreach (var target in allCreatures)
        {
            var hpQuarter = (int)(target.CurrentHp / 4);
            if (hpQuarter > 0)
            {
                await PowerCmd.Apply<EmaWitchFactorPower>(
                    choiceContext, target, hpQuarter,
                    creature, source, false);
            }
        }

        await PowerCmd.Apply<RitualCeremonyPower>(
            choiceContext, creature,
            source.DynamicVars["RitualCeremonyPower"].BaseValue,
            creature, source, false);

        var createCardMethod = typeof(ICombatState).GetMethod("CreateCard", new Type[] { typeof(Player) });

        for (int i = 0; i < 3; i++)
        {
            var affinityCard = (CardModel)createCardMethod.MakeGenericMethod(typeof(Emamqinjin)).Invoke(combatState, new object[] { owner });
            var estrangementCard = (CardModel)createCardMethod.MakeGenericMethod(typeof(Emamshuyuan)).Invoke(combatState, new object[] { owner });
            affinityCard.SetToFreeThisTurn();
            estrangementCard.SetToFreeThisTurn();

            var options = new List<CardModel> { affinityCard, estrangementCard };
            var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1, 1);
            var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, options, owner, prefs);
            var picked = selected.FirstOrDefault();
            if (picked == null) continue;

            await CardPileCmd.AddGeneratedCardToCombat(picked, PileType.Hand, owner);
            await Cmd.Wait(0.1f);

            Creature? autoTarget = null;
            if (picked.TargetType == TargetType.AnyEnemy)
            {
                autoTarget = combatState.GetOpponentsOf(creature)
                    .Where(c => c.IsAlive)
                    .FirstOrDefault();
            }

            await CardCmd.AutoPlay(choiceContext, picked, autoTarget);
        }

        // 卡面：「给予当前所有牌随机【审判】组件」
        // （【审判】组件 = 赞同 AgreementTrialComponent / 反驳 RebuttalTrialComponent / 疑问 DoubtTrialComponent，
        //   由对应附魔 Rebuttal / Agreement / Doubt 具现而来）
        var enchantTypes = new Type[] { typeof(Rebuttal), typeof(Agreement), typeof(Doubt) };
        var rng = owner.RunState.Rng.CombatCardSelection;

        var rebuttalCanonical = ModelDb.Enchantment<Rebuttal>();
        var agreementCanonical = ModelDb.Enchantment<Agreement>();
        var doubtCanonical = ModelDb.Enchantment<Doubt>();

        var allOwnedCards = new[] { PileType.Hand, PileType.Draw, PileType.Discard }
            .SelectMany(pileType => pileType.GetPile(owner).Cards)
            .Distinct()
            .ToList();

        foreach (var card in allOwnedCards)
        {
            var chosenEnchant = rng.NextItem(enchantTypes);
            if (chosenEnchant == typeof(Rebuttal))
                CardCmd.Enchant(rebuttalCanonical.ToMutable(), card, 1m);
            else if (chosenEnchant == typeof(Agreement))
                CardCmd.Enchant(agreementCanonical.ToMutable(), card, 1m);
            else
                CardCmd.Enchant(doubtCanonical.ToMutable(), card, 1m);
        }

        // 卡面：「选择获得3点【羁绊】，生成等量零费稀有【羁绊】牌」
        // ⇒ 上面 3 次选择 = 3 点【羁绊】；再生成 3 张 0 费稀有羁绊牌放入抽牌堆。
        for (int i = 0; i < 3; i++)
        {
            var rareBondCard = CreateRandomRareBondCard(combatState, owner, createCardMethod);

            // 永久 0 费
            if (!rareBondCard.EnergyCost.CostsX && rareBondCard.EnergyCost.Canonical > 0)
                rareBondCard.EnergyCost.UpgradeBy(-rareBondCard.EnergyCost.Canonical);

            CardCmd.PreviewCardPileAdd(
                await CardPileCmd.AddGeneratedCardToCombat(rareBondCard, PileType.Draw, owner, CardPilePosition.Random));
        }
    }

    /// <summary>卡面里的【羁绊】牌中，稀有度 Rare 的 6 张。</summary>
    private static readonly Type[] RareBondCardTypes =
    [
        typeof(StabbingBlade),
        typeof(NoahEstrangement),
        typeof(SwapBodySuccess),
        typeof(DollGift),
        typeof(CocoAffinity),
        typeof(BondSettlement),
    ];

    private CardModel CreateRandomRareBondCard(ICombatState combatState, Player owner, MethodInfo createCardMethod)
    {
        var rng = owner.RunState.Rng.CombatCardSelection;
        var chosenType = rng.NextItem(RareBondCardTypes);
        var genericMethod = createCardMethod.MakeGenericMethod(chosenType);
        return (CardModel)genericMethod.Invoke(combatState, new object[] { owner });
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
