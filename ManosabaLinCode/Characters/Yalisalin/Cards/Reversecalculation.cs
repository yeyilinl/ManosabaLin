using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Reversecalculation()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        if (IsUpgraded)
        {
            foreach (var color in hairpin.SealEarliestDistinctFireColors(target, 2))
                await YalisalinFireColorSystem.ResolveExtraFireColorReward(choiceContext, Owner, color, this);
            await YalisalinSealedFirePower.Sync(choiceContext, Owner, this);
            return;
        }

        if (hairpin.TrySealEarliestFireColor(target, out var sealedColor))
        {
            await YalisalinFireColorSystem.ResolveExtraFireColorReward(choiceContext, Owner, sealedColor, this);
            await YalisalinSealedFirePower.Sync(choiceContext, Owner, this);
        }
    }
}
