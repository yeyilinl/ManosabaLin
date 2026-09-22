using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Twodifferenttestimonies()
    : ManosabaCardTemplate(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new DynamicVar("Hits", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        YalisalinFireColor? previousColor = null;
        var hits = IsUpgraded ? 3 : 2;
        DynamicVars["Hits"].BaseValue = hits;

        for (var i = 0; i < hits; i++)
        {
            await YalisalinFireColorCardHelpers.Attack(choiceContext, cardPlay, this, target, DynamicVars.Damage.BaseValue);
            var result = await YalisalinFireColorSystem.ConsumeFireColorDetailed(choiceContext, Owner, target, 1, this);
            var currentColor = result.Consumed.LastOrDefault().Color;
            if (result.Consumed.Count == 0)
                continue;

            if (previousColor != null && previousColor.Value != currentColor)
                await YalisalinFireColorSystem.ResolveExtraFireColorReward(choiceContext, Owner, currentColor, this);

            previousColor = currentColor;
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["Hits"].UpgradeValueBy(1);
    }
}
