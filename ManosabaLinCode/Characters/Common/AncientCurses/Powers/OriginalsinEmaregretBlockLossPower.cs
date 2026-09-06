namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinEmaregretBlockLossPower : ManosabaPowerTemplate
{
    private decimal _lostThisTurn;
    private bool _hooked;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public decimal LostThisTurn => _lostThisTurn;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        HookBlockChanged();
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
            _lostThisTurn = 0m;
        HookBlockChanged();
        return Task.CompletedTask;
    }

    public void RecordLoseBlock(decimal amount)
    {
        if (amount > 0m)
            _lostThisTurn += amount;
    }

    private void HookBlockChanged()
    {
        if (_hooked || Owner is null) return;
        Owner.BlockChanged += OnBlockChanged;
        _hooked = true;
    }

    private void OnBlockChanged(int oldBlock, int newBlock)
    {
        if (newBlock < oldBlock)
            _lostThisTurn += oldBlock - newBlock;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (_hooked)
            oldOwner.BlockChanged -= OnBlockChanged;
        _hooked = false;
        return Task.CompletedTask;
    }
}
