using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Burntthermometerpaper()
    : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Life", 2), new DynamicVar("Consume", 3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars["Life"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            this,
            cardPlay);

        if (hairpin.IsFireColorFull(target))
        {
            await hairpin.ConsumeFireColor(choiceContext, target, DynamicVars["Consume"].IntValue, this);
            this.SetHeatWord(strong: false);
            await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this, strong: false);
        }
        else
        {
            while (!hairpin.IsFireColorFull(target) && hairpin.TryAddFireColor(target, 1, this))
            {
            }

            // 补满后升温变为强升温
            this.SetHeatWord(strong: true);
            await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this, strong: true);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
