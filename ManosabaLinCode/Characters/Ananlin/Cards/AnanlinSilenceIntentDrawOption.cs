using MegaCrit.Sts2.Core.Commands;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinSilenceIntentDrawOption()
    : AnanlinNonRandomCardTemplate(-1, CardType.Skill, CardRarity.Token, TargetType.None, false)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var amount = DynamicVars.Cards.IntValue;
        if (amount <= 0) return;

        foreach (var player in Owner.Creature?.CombatState?.Players ?? [Owner])
            if (player.Creature.IsAlive)
                await CardPileCmd.Draw(choiceContext, amount, player);
    }
}
