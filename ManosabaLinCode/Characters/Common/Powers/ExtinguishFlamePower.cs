namespace ManosabaLin.Characters.Common.Powers;

/// <summary>
///     【灭火】：世界之火熄灭。
///     你与所有敌人造成的伤害减半；每回合开始时你获得等同于层数的格挡；
///     且你的格挡不再于回合开始时消失（原版「壁垒」同款规则）。
/// </summary>
[RegisterPower]
public sealed class ExtinguishFlamePower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    /// <summary>
    ///     你打出去的伤害、以及敌人打出来的伤害都减半；无关的第三方（其他友方）不受影响。
    /// </summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (props.HasFlag(ValueProp.Unpowered))
            return 1m;
        if (dealer is null)
            return 1m;
        if (dealer != Owner && !dealer.IsEnemy)
            return 1m;

        return 0.5m;
    }

    public override bool ShouldClearBlock(Creature creature)
    {
        return creature != Owner;
    }

    public override Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
    {
        if (ReferenceEquals(preventer, this))
            Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner)
            return;

        if (Owner is not { IsAlive: true } owner)
            return;

        if (Amount <= 0)
            return;

        await CreatureCmd.GainBlock(owner, Amount, ValueProp.Unpowered, null);
    }
}
