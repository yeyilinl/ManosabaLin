using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class ArisaGuilt : LinAncientCurseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("MagicAmount", 1m); }
    }

    protected override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource,
        ComponentContext componentContext)
    {
        if (Pile?.Type != PileType.Hand) return;
        if (dealer != Owner.Creature) return;
        if (result.TotalDamage <= 0) return;

        var amount = DynamicVars["MagicAmount"].BaseValue;
        if (amount <= 0m) return;

        if (Owner.Creature.HasPower<YlsmPower>())
            await PowerCmd.Apply<YlsmPower>(choiceContext, target, amount, Owner.Creature, this);
        else
            await PowerCmd.Apply<YlsmPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
    }
}
