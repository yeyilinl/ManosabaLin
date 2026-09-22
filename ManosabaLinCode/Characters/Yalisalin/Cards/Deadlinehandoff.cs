using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Deadlinehandoff()
    : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new DynamicVar("Repeats", 3),
        new DynamicVar("Consume", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        var consume = IsUpgraded ? 3 : 2;
        DynamicVars["Consume"].BaseValue = consume;

        for (var i = 0; i < DynamicVars["Repeats"].IntValue; i++)
        {
            if (!hairpin.IsFireColorFull(target))
            {
                await YalisalinFireColorCardHelpers.Attack(choiceContext, cardPlay, this, target, DynamicVars.Damage.BaseValue);
                await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this, strong: true);
                continue;
            }

            await hairpin.ConsumeFireColor(choiceContext, target, consume, this);
            await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this, strong: false);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["Consume"].UpgradeValueBy(1);
    }
}
