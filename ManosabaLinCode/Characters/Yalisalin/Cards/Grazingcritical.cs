using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Grazingcritical()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        var wasFull = hairpin.IsFireColorFull(target);
        await hairpin.ConsumeFireColor(choiceContext, target, 1, this);
        if (wasFull && !hairpin.IsFireColorFull(target))
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
            hairpin.TryAddFireColor(target, 1, this);
        }

        await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
