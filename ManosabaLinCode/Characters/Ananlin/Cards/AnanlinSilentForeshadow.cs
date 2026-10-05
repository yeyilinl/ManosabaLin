using ManosabaLin.Characters.Ananlin.Powers;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinSilentForeshadow() : ManosabaCardTemplate(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new PowerVar<SilentPower>("Silence", 1m),
        new CardsVar(1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SilentPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        if (this.Sketchbook() is not { } sketchbook) return;

        await sketchbook.AddSilence(choiceContext, DynamicVars["Silence"].IntValue, this);

        // 若本回合尚未打出过攻击牌，抽 Cards 张。
        if (sketchbook.AttacksPlayedThisTurn == 0)
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["Silence"].UpgradeValueBy(1m);
    }
}
