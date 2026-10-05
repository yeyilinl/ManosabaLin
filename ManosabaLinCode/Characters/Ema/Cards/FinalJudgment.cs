using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Emalin.Enchantments;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using ManosabaLin.Characters.Emalin.Components;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Component.Interfaces;

namespace ManosabaLin.Characters.Ema.Cards;

[RegisterCard(typeof(EmalinCardPool))]
public sealed class FinalJudgment : ManosabaCardTemplate
{
    public FinalJudgment() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

  
    protected override IEnumerable<ICardComponent> CanonicalComponents => [new Hatedperson()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var creature = owner.Creature;
        var rng = owner.RunState.Rng.CombatTargets;

        // ===== 读取所有状态 =====
        var bond = creature.GetPower<BondPower>();
        var affinity = bond?.Affinity ?? 0;
        var estrangement = bond?.Estrangement ?? 0;
        var bondTotal = affinity + estrangement;

        // 数遍审判附魔
        var allPiles = new[] { PileType.Draw, PileType.Hand, PileType.Discard };
        var trialCards = new List<(CardModel card, PileType pile)>();
        foreach (var pileType in allPiles)
        {
            var pile = pileType.GetPile(owner);
            foreach (var card in pile.Cards)
            {
                if (card.Enchantment is Rebuttal or Agreement or Doubt)
                    trialCards.Add((card, pileType));
            }
        }
        var trialCount = trialCards.Count;

        // 读取魔女化
        var withPower = creature.GetPower<WithPower>();
        var witchAmount = (int)(withPower?.Amount ?? 0);

        // 读取场上敌人
        var enemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();

        await CreatureCmd.TriggerAnim(creature, "Cast", owner.Character.CastAnimDelay);

        // ===== 第一幕：羁绊 =====
        // 卡面：「对全体敌人造成2倍(羁绊+审判)伤害
        //        友方获得3倍「亲近」的格挡，全体敌人获得「疏远」层易伤。」
        // ⇒ 亲近/疏远各自独立结算，不再二选一。
        var bondDamage = (bondTotal + trialCount) * 2;
        if (bondDamage > 0)
        {
            foreach (var enemy in enemies)
                await CreatureCmd.Damage(choiceContext, enemy, bondDamage, ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
        }

        if (affinity > 0)
        {
            foreach (var ally in CombatState.Allies.Where(a => a.IsAlive))
                await CreatureCmd.GainBlock(ally, affinity * 3, ValueProp.Move, cardPlay);
        }

        if (estrangement > 0)
        {
            foreach (var enemy in enemies)
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, estrangement, creature, this, false);
        }

        if (bond != null)
        {
            bond.Affinity = 0;
            bond.Estrangement = 0;
        }

        // ===== 第二幕：审判 =====
        foreach (var (card, _) in trialCards)
        {
            if (card.Enchantment is Rebuttal)
            {
                if (enemies.Count > 0)
                {
                    var target = rng.NextItem(enemies);
                    await CreatureCmd.Damage(choiceContext, target, 1m, ValueProp.Unpowered, null, null);
                }
            }
            else if (card.Enchantment is Agreement)
            {
                foreach (var ally in CombatState.Allies.Where(a => a.IsAlive))
                    await CreatureCmd.GainBlock(ally, 3m, ValueProp.Move, cardPlay);
            }
            else if (card.Enchantment is Doubt)
            {
                await CreatureCmd.GainBlock(creature, 1m, ValueProp.Move, cardPlay);
            }
        }

        // 卡面：「抽【审判】牌张卡牌，每50【魔女化】获得1点能量」
        // ⇒ 抽牌只按【审判】牌数；【魔女化】只单独折算能量（不再额外附带抽牌/加成）。
        var drawCount = trialCount;
        if (drawCount > 0)
            await CardPileCmd.Draw(choiceContext, drawCount, owner);

        // 移除所有审判附魔
        foreach (var (card, pileType) in trialCards)
        {
            var template = card.CanonicalInstance;
            var upgradeLevel = card.CurrentUpgradeLevel;
            await CardPileCmd.RemoveFromCombat(card);
            var newCard = CombatState.CreateCard(template, owner);
            for (int i = 0; i < upgradeLevel; i++)
                CardCmd.Upgrade(newCard);
            await CardPileCmd.AddGeneratedCardToCombat(newCard, pileType, owner);
        }

        // ===== 第三幕：魔女化 =====
        var totalEnergy = witchAmount / 50;

        if (totalEnergy > 0)
            await PlayerCmd.GainEnergy(totalEnergy, owner);

        if (withPower != null)
            await PowerCmd.Remove(withPower);

        // ===== 第四幕：嫌疑 =====
        foreach (var enemy in enemies)
        {
            var suspect = enemy.GetPower<SuspectPower>();
            if (suspect == null || suspect.Amount <= 0) continue;

            // 卡面：「对每个有【嫌疑】的敌人造成3倍【嫌疑】伤害」
            var suspectDamage = (int)suspect.Amount * 3;
            if (suspectDamage > 0)
                await CreatureCmd.Damage(choiceContext, enemy, suspectDamage, ValueProp.Unpowered | ValueProp.Move, this, null);

            await PowerCmd.Remove(suspect);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}