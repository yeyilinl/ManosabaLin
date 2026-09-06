namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinEmaregretWitchOnHpLossPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner) return;
        if (delta >= 0m) return;
        if (Owner.CombatState is not { } combatState) return;

        var enemies = combatState.Enemies.Where(c => c.IsAlive).ToList();
        if (enemies.Count == 0) return;

        var rng = Owner.Player?.RunState.Rng.CombatCardGeneration;
        if (rng is null) return;
        var enemy = rng.NextItem(enemies);
        if (enemy is null) return;

        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<EmaWitchFactorPower>(ctx, enemy, Math.Abs(delta), Owner, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}
