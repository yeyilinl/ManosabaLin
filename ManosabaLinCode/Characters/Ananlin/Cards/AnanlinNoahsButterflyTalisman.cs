using ManosabaLin.Characters.Ananlin.Powers;
using ManosabaLin.Characters.Ananlin.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinNoahsButterflyTalisman() : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get { yield return CardKeyword.Exhaust; }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CrimsonbutterflyPower>(),
        HoverTipFactory.FromPower<SilentPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        var anyAttackIntent = CombatState.GetOpponentsOf(Owner.Creature)
            .Any(enemy => enemy.IsAlive && HasAttackIntent(enemy));

        if (anyAttackIntent)
        {
            // 给予所有人【蝴蝶】，并随机立刻生效1种替换意图效果
            await PowerCmd.Apply<CrimsonbutterflyPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            await AnanlinSilenceIntentManager.TriggerRandomReplacementEffect(choiceContext, Owner);
            return;
        }

        // 若没有一个攻击意图，缄默替换意图数值+1，下回合将本牌返回手牌
        AnanlinSilenceIntentManager.IncreaseSilenceGrowth(Owner);

        var returnPower = await PowerCmd.Apply<AnanlinDelayedCardReturnPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
        returnPower?.AddCard(this);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    private bool HasAttackIntent(Creature target)
    {
        if (this.Sketchbook() is { } sketchbook)
            return sketchbook.IsAttackIntent(target);

        return target.Monster?.NextMove?.Intents.Any(static intent => intent is AttackIntent) == true;
    }
}
