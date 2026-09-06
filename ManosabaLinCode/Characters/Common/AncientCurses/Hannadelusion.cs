using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class Hannadelusion : LinAncientCurseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("Chance", 10m); }
    }

    protected override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Pile is { Type: PileType.Hand }) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var chance = (float)(DynamicVars["Chance"].BaseValue / 100m);
        if (rng.NextFloat() >= chance) return;

        await RunGoldChange(rng);
    }

    internal async Task RunGoldChange(MegaCrit.Sts2.Core.Random.Rng rng)
    {
        if (rng.NextFloat() < 0.5f)
            await PlayerCmd.GainGold(20, Owner);
        else
            await PlayerCmd.LoseGold(System.Math.Min(10, Owner.Gold), Owner);
    }
}
