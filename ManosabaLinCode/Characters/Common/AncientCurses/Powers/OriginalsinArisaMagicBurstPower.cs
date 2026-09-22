namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinArisaMagicBurstPower : ManosabaPowerTemplate
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
        if (power is not YlsmPower) return;
        if (power.Amount < 10m) return;
        if (applier != Owner) return;
        if (power.Owner == Owner) return;
        if (!Owner.HasPower<YlsmPower>()) return;

        // 达到10层时立刻触发目标身上所有【紫藤亚里沙的魔法】：每层造成1点不可阻挡伤害
        var stacks = power.Amount;
        if (stacks > 0m)
            await CreatureCmd.Damage(
                choiceContext,
                power.Owner,
                stacks,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                cardSource,
                null);

        // 移除目标身上所有的【紫藤亚里沙的魔法】
        power.RemoveInternal();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}
