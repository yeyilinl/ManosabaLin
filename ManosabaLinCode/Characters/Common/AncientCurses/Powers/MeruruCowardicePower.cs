namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

/// <summary>
/// 怯懦的隐藏能力：手牌中存在怯懦时，获得格挡 -2（获得前格挡大于 15 时 -3）。
/// 获得量被减成负数时，多余部分从现有格挡里扣除。
/// </summary>
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

        var penalty = target.Block > 15 ? 3m : 2m;
        return -penalty;
    }
}
