using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Unseenkindling()
    : ManosabaCardTemplate(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Move), new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        var hadFireColor = hairpin.TargetHasFireColor(target);
        hairpin.TryAddFireColor(target, hadFireColor ? 1 : 2, this);

        if (hadFireColor)
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        if (IsUpgraded)
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this);
    }

}
