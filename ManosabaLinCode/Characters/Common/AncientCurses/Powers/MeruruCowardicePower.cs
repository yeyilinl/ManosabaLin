namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class MeruruCowardicePower : LinCurseConditionalPower<MeruruCowardice>
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override decimal ModifyBlockAdditive(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || !IsCurseInHand()) return 0m;

        var penalty = 2m;
        if (Owner?.Player is { } player)
        {
            var curse = PileType.Hand.GetPile(player).Cards.OfType<MeruruCowardice>().FirstOrDefault();
            if (curse != null && curse.DynamicVars.TryGetValue("Penalty", out var penaltyVar))
                penalty = penaltyVar.BaseValue;
        }

        if (target.Block > 15)
            penalty += 1m;

        return -penalty;
    }
}
