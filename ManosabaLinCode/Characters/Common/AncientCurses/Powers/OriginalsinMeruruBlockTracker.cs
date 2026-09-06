namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinMeruruBlockTracker : ManosabaPowerTemplate
{
    private decimal _gainedThisTurn;
    private decimal _lastTurnGained;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public decimal LastTurnGained => _lastTurnGained;

    public override Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature == Owner && amount > 0m)
            _gainedThisTurn += amount;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return Task.CompletedTask;
        _lastTurnGained = _gainedThisTurn;
        _gainedThisTurn = 0m;
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return Task.CompletedTask;
        _gainedThisTurn = 0m;
        return Task.CompletedTask;
    }
}
