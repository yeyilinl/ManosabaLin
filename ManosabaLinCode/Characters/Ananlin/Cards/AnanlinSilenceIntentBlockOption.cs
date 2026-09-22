using MegaCrit.Sts2.Core.Commands;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinSilenceIntentBlockOption()
    : AnanlinNonRandomCardTemplate(-1, CardType.Skill, CardRarity.Token, TargetType.None, false)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var amount = DynamicVars.Block.BaseValue;
        if (amount <= 0) return;

        foreach (var player in Owner.Creature?.CombatState?.Players ?? [Owner])
            if (player.Creature.IsAlive)
                await CreatureCmd.GainBlock(player.Creature, DynamicVars.Block, cardPlay);
    }
}
