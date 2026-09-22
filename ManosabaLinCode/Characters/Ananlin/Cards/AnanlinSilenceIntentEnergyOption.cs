using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinSilenceIntentEnergyOption()
    : AnanlinNonRandomCardTemplate(-1, CardType.Skill, CardRarity.Token, TargetType.None, false)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var amount = DynamicVars.Energy.IntValue;
        if (amount <= 0) return;

        foreach (var player in Owner.Creature?.CombatState?.Players ?? [Owner])
            if (player.Creature.IsAlive)
                await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, player.Creature, amount, Owner.Creature, this);
    }
}
