using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
///     被缚的普罗米修斯（挂在你自己身上的延迟代价）：
///     持续到下回合结束时结算 —— 失去等同于你当前封存火色总数量的格挡；
///     在这两个回合里，每当你消耗敌人身上的火色时，其中 1 格会在正常结算之外额外封存给你。
///     层数 = 剩余回合数（打出时给 2）。
/// </summary>
[RegisterPower]
public sealed class BoundPrometheusPower : ManosabaPowerTemplate
{
    /// <summary>升级版结算时只失去一半格挡。</summary>
    public bool HalfBlockLoss { get; set; }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;
        if (Owner is not { IsAlive: true } owner)
            return;

        await PowerCmd.ModifyAmount(choiceContext, this, -1m, owner, null, false);
        if (Amount > 0)
            return;

        await Settle(choiceContext, owner);
        await PowerCmd.Remove(this);
    }

    /// <summary>结算：按结算瞬间的封存火色总数量扣除格挡（扣到 0 为止，不溢出到生命）。</summary>
    private async Task Settle(PlayerChoiceContext choiceContext, Creature owner)
    {
        if (owner.Player is not { } player)
            return;
        if (!YalisalinFireColorSystem.TryGetHairpin(player, out var hairpin))
            return;

        var sealedTotal = hairpin.TotalSealedFireCount;
        if (sealedTotal <= 0)
            return;

        var wanted = HalfBlockLoss ? (int)Math.Ceiling(sealedTotal / 2m) : sealedTotal;
        var loss = Math.Min(owner.Block, wanted);
        if (loss <= 0m)
            return;

        await CreatureCmd.LoseBlock(choiceContext, owner, loss, owner);
    }
}
