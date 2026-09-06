using System.Collections.Generic;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common.AncientCurses.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class MeruruCowardice : LinAncientCurseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("Penalty", 2m); }
    }

    protected override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player,
        ComponentContext componentContext)
    {
        if (player != Owner) return;
        if (Owner.Creature.GetPower<MeruruCowardicePower>() is null)
            await PowerCmd.Apply<MeruruCowardicePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (Owner.Creature.GetPower<OriginalsinMeruruBlockTracker>() is null)
            await PowerCmd.Apply<OriginalsinMeruruBlockTracker>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
