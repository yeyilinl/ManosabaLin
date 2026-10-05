using System.Collections.Generic;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common.AncientCurses.Powers;
using ManosabaLin.Characters.Common.Powers;
using TempStrength = ManosabaLin.Characters.Common.Powers.TempStrength;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class SherryVoid : LinAncientCurseCard
{
    public SherryVoid() : base(1) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("Threshold", 13m); }
    }

    protected override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player,
        ComponentContext componentContext)
    {
        if (player != Owner) return;
        if (Owner.Creature.GetPower<SherryVoidPower>() is null)
            await PowerCmd.Apply<SherryVoidPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Pile?.Type != PileType.Hand) return;
        if (Owner.Creature.GetPower<SherryVoidPower>() is not { } power) return;
        if (!power.ConsumeHalvedPlay()) return;

        await PowerCmd.Apply<TempStrength>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
