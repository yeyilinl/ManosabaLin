using ManosabaLin.Characters.Ananlin.Powers;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinUnnoticed()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy),
        IAnanlinPeaceOfMindSpecialCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new CardsVar(1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AnanlinPeaceOfMindPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (cardPlay.Target is not { } target) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        if (!HasAnyEnemyAttackIntent())
        {
            await this.GainPeaceOfMind(choiceContext);
            var bonusDraws = this.PeaceOfMindAmount() * DynamicVars.Cards.IntValue;
            if (bonusDraws > 0)
                await CardPileCmd.Draw(choiceContext, bonusDraws, Owner);
            return;
        }

        // 有敌人攻击意图：消耗掉全部安心，每消耗1层额外造成一次6点伤害
        var peace = Owner.Creature.GetPower<AnanlinPeaceOfMindPower>();
        var lost = Math.Max(0, (int)(peace?.Amount ?? 0));
        if (peace is { Amount: > 0 })
            await PowerCmd.ModifyAmount(choiceContext, peace, -peace.Amount, Owner.Creature, this);

        if (lost <= 0) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(lost)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    private bool HasAnyEnemyAttackIntent()
    {
        return this.Sketchbook()?.HasAnyEnemyAttackIntent() == true;
    }
}
