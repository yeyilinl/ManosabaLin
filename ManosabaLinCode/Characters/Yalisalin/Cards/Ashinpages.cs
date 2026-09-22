using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Ashinpages()
    : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        if (hairpin.HasAnySealedFire())
        {
            hairpin.TryCopySealedFire();
            await YalisalinSealedFirePower.Sync(choiceContext, Owner, this);
            return;
        }

        if (hairpin.TryGetEarliestFireColor(target, out var color))
        {
            hairpin.GrantSealedFire(color);
            await YalisalinSealedFirePower.Sync(choiceContext, Owner, this);
        }

        await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
