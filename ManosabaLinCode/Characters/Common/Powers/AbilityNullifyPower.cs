namespace ManosabaLin.Characters.Common.Powers;

/// <summary>
///     【能力失效】：移除目标当前身上全部增益能力，持续一个回合后原样归还。
///     复用「嫌疑」满层时对敌人使用的同一套处理方式（快照 → 移除 → 回合结束归还），
///     归还时机取敌方的回合结束（失效区间覆盖「本回合剩余时间 + 敌人整个回合」），
///     并为「敌人没轮到就被打死」的情况补一个玩家回合开始时的兜底。
/// </summary>
[RegisterPower]
public sealed class AbilityNullifyPower : ManosabaPowerTemplate
{
    private List<Snapshot> _removedPowers = [];
    private bool _buffsRemoved;
    private bool _restoring;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await RemoveBuffs();
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        await RestoreAndRemove();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner)
            return;

        await RestoreAndRemove();
    }

    private async Task RemoveBuffs()
    {
        if (_buffsRemoved)
            return;

        _buffsRemoved = true;

        var removed = new List<Snapshot>();
        foreach (var power in Owner.Powers.ToList().Where(static p => p.Type == PowerType.Buff))
        {
            removed.Add(new Snapshot(power.Id, power.Amount));
            await PowerCmd.Remove(power);
        }

        _removedPowers = removed;
    }

    private async Task RestoreAndRemove()
    {
        if (!_buffsRemoved || _restoring)
            return;

        _restoring = true;
        _buffsRemoved = false;

        var context = new ThrowingPlayerChoiceContext();
        foreach (var (powerId, amount) in _removedPowers)
        {
            if (!Owner.IsAlive)
                break;

            var powerModel = ModelDb.GetById<PowerModel>(powerId);
            if (powerModel is null)
                continue;

            await PowerCmd.Apply(context, powerModel.ToMutable(0), Owner, amount, Owner, null);
        }

        _removedPowers = [];
        await PowerCmd.Remove(this);
    }

    private sealed record Snapshot(ModelId PowerId, int Amount);
}
