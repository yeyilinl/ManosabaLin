namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
///     「罪与罚」每回合只能使用一次的隐藏标记：
///     下个玩家回合开始时自动移除，从而恢复可打出状态。
/// </summary>
[RegisterPower]
public sealed class CrimeAndPunishmentUsedPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner)
            return;

        await PowerCmd.Remove(this);
    }
}
