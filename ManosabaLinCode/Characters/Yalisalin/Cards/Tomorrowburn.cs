using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Tomorrowburn()
    : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Preserve", 2),
        new DynamicVar("Repeats", 2),
        new CardsVar(1),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        hairpin.GainPreserveHighestFireColor(DynamicVars["Preserve"].IntValue);
        for (var i = 0; i < DynamicVars["Repeats"].IntValue; i++)
        {
            var result = await hairpin.ConsumeFireColorDetailed(choiceContext, target, 1, this);
            if (result.PreservedHighest.Count == 0)
                continue;

            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }

        await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
        // 重复{Repeats}次的消耗1格火色：升级 2 -> 3
        DynamicVars["Repeats"].UpgradeValueBy(1);
    }
}
