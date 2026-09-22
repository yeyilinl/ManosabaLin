using ManosabaLin.Characters.Ananlin.Powers;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinCrossOutTypo()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
        new PowerVar<SilentPower>("Silence", 2m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<SilentPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var selected = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            null,
            this)).FirstOrDefault();
        if (selected is null) return;

        await CardCmd.Exhaust(choiceContext, selected);

        await this.AddSilence(choiceContext, DynamicVars["Silence"].IntValue);

        // 若敌人意图为不攻击，获得下回合格挡
        if (this.Sketchbook() is { } sketchbook && !sketchbook.HasAnyEnemyAttackIntent())
            await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, Owner.Creature, DynamicVars.Block.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
