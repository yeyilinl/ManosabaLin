namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinArisaMagicBurstPower : ManosabaPowerTemplate
{
    private bool _triggered;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (_triggered) return;
        if (amount < 10m) return;
        if (power is not YlsmPower) return;
        if (applier != Owner) return;
        if (power.Owner == Owner) return;
        if (!Owner.HasPower<YlsmPower>()) return;

        _triggered = true;
        await CreatureCmd.Damage(
            choiceContext,
            power.Owner,
            1m,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            cardSource,
            null);
        if (Owner.Player is { } player)
            await PlayerCmd.GainEnergy(1m, player);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}
