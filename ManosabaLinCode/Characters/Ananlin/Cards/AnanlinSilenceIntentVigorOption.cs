using ManosabaLin.Characters.Ananlin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinSilenceIntentVigorOption()
    : AnanlinNonRandomCardTemplate(-1, CardType.Skill, CardRarity.Token, TargetType.None, false)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VigorPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var amount = DynamicVars["VigorPower"].BaseValue;
        if (amount <= 0) return;

        foreach (var player in Owner.Creature?.CombatState?.Players ?? [Owner])
            if (player.Creature.IsAlive)
                await PowerCmd.Apply<VigorPower>(choiceContext, player.Creature, amount, Owner.Creature, this);
    }
}
