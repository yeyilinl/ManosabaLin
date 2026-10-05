using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Emalin.Components;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Common.HiroKeywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using System.Collections.Generic;
using System.Linq;
using MinionLib.Component.Interfaces;

namespace ManosabaLin.Characters.Hiro.Cards;

[RegisterCard(typeof(HirolinCardPool))]
public sealed class JusticeEnforcer() : ManosabaCardTemplate(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<ICardComponent> CanonicalComponents => [new Executorofjustice()];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<OriginalsinjusticePower>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;
        var owner = source.Owner;

        source.AddComponent(new Executorofjustice());

        var perjury = owner.Creature.GetPower<PerjuryPower>();
        var suspect = owner.Creature.GetPower<SuspectPower>();
        var justice = owner.Creature.GetPower<JusticePower>();
        var with = owner.Creature.GetPower<WithPower>();

        var perjuryAmt = perjury?.Amount ?? 0;
        var suspectAmt = suspect?.Amount ?? 0;
        var justiceAmt = justice?.Amount ?? 0;
        var withAmt = with?.Amount ?? 0;

        var handCards = PileType.Hand.GetPile(owner).Cards.Where(c => c != source).ToList();
        var rebirthCards = handCards.Where(c => c.HasModKeyword(TransmigrationRules.TransmigrationCardKeyword)).ToList();

        if (rebirthCards.Count > 0)
        {
            var pool = owner.Character.CardPool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.Type != CardType.Status && c.Type != CardType.Curse)
                .ToList();

            // 卡面：「失去本卡记载资源」⇒【轮回】牌就是本卡记载资源，手牌里全部消耗；
            //        「手牌每有1张【轮回】牌就生成1张【轮回】牌并回复1点生命」。
            foreach (var card in rebirthCards)
            {
                await CardCmd.Exhaust(choiceContext, card);

                if (pool.Count > 0)
                {
                    var randomId = owner.RunState.Rng.Shuffle.NextItem(pool);
                    var generated = CombatState.CreateCard(randomId, owner);
                    generated.AddModKeyword(TransmigrationRules.TransmigrationCardKeyword);
                    await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, owner, CardPilePosition.Bottom);
                }

                await CreatureCmd.Heal(owner.Creature, 1);
            }
        }

        perjury = owner.Creature.GetPower<PerjuryPower>();
        suspect = owner.Creature.GetPower<SuspectPower>();
        perjuryAmt = perjury?.Amount ?? 0;
        suspectAmt = suspect?.Amount ?? 0;

        if (perjuryAmt > 0)
        {
            var enemies = owner.Creature.CombatState.Creatures
                .Where(c => c.IsEnemy && c.IsAlive).ToList();

            if (enemies.Count > 0)
            {
                // 联机下必须用同步 RNG：原为 new System.Random()（进程本地随机），
                // 伪证层数各段伤害会打到不同敌人，造成结算分歧（RitsuLib StateDivergence → 踢客机）。
                var rng = owner.RunState.Rng.CombatTargets;
                for (var i = 0; i < perjuryAmt; i++)
                {
                    // 卡面：「每层【伪证】对随机敌人造成2+【魔女化】一半的伤害」
                    await CreatureCmd.Damage(choiceContext, rng.NextItem(enemies) ?? enemies[0], 2 + (int)(withAmt / 2), ValueProp.Unpowered, null, null);
                }
            }
            await PowerCmd.Apply<JusticePower>(choiceContext, owner.Creature, perjuryAmt, owner.Creature, source, false);
            await PowerCmd.Remove(perjury);
        }

        if (suspectAmt > 0)
        {
            // 卡面：「每层【嫌疑】回复1点能量」（不再折算成【魔女化】）
            var suspectToRemove = owner.Creature.GetPower<SuspectPower>();
            if (suspectToRemove != null)
            {
                var amount = suspectToRemove.Amount;
                await PlayerCmd.GainEnergy(amount, owner);
                await PowerCmd.Remove(suspectToRemove);
            }
        }

        // 卡面：「按照【正义】层数全体回血」⇒【正义】同样是本卡记载资源，
        //        层数需含本段刚由【伪证】转换而来的部分，结算完（全体回血）后全部失去。
        justice = owner.Creature.GetPower<JusticePower>();
        justiceAmt = justice?.Amount ?? 0;

        if (justiceAmt > 0)
        {
            var allies = owner.Creature.CombatState.Creatures
                .Where(c => c.IsAlive && !c.IsEnemy)
                .ToList();

            foreach (var ally in allies)
                await CreatureCmd.Heal(ally, justiceAmt);
        }

        justice = owner.Creature.GetPower<JusticePower>();
        if (justice != null)
            await PowerCmd.Remove(justice);

        // 卡面：「若失去【魔女化】大于100，获得1层【原罪】」
        // ⇒ 先失去全部【魔女化】（失去本卡记载资源），再看失去量是否大于 100。
        if (withAmt > 100)
        {
            await PowerCmd.Apply<OriginalsinjusticePower>(
                choiceContext, owner.Creature, 1, owner.Creature, source, false
            );
        }

        if (with != null)
            await PowerCmd.Remove(with);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
