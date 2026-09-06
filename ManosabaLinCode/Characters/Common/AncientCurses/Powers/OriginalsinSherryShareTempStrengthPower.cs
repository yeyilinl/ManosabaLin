using ManosabaLin.Characters.Common.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinSherryShareTempStrengthPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0m) return;
        if (power.Owner != Owner) return;
        if (power is not (TemporaryStrengthPower or TempStrength)) return;
        if (Owner.CombatState is not { } combatState) return;

        var allies = combatState.Allies.Where(c => c.IsAlive && c != Owner).ToList();
        if (allies.Count == 0) return;

        var rng = Owner.Player?.RunState.Rng.CombatCardGeneration;
        if (rng is null) return;
        var ally = rng.NextItem(allies);
        if (ally is null) return;

        await PowerCmd.Apply<TempStrength>(choiceContext, ally, 1m, Owner, cardSource);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}
