using System.Collections.Generic;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common.AncientCurses.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class Emaregret : LinAncientCurseCard
{
    public Emaregret() : base(1) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("HpLoss", 1m); }
    }

    protected override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player,
        ComponentContext componentContext)
    {
        if (player != Owner) return;
        if (Owner.Creature.GetPower<OriginalsinEmaregretBlockLossPower>() is null)
            await PowerCmd.Apply<OriginalsinEmaregretBlockLossPower>(
                choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Pile?.Type != PileType.Hand) return;

        var creature = Owner.Creature;
        if (creature.IsDead) return;

        var amount = DynamicVars["HpLoss"].BaseValue;
        if (amount <= 0m) return;

        if (creature.Block > 13)
        {
            await CreatureCmd.LoseBlock(choiceContext, creature, amount, creature);
        }
        else
        {
            await CreatureCmd.Damage(
                choiceContext,
                creature,
                amount,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this,
                null);
        }
    }
}
