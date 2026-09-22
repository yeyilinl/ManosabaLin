using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Ananlin.Powers;

/// <summary>
/// 牢房才是家：每当你本回合给予敌人【已缄默】，进入【牢房】；进入【牢房】时获得该能力数值的能量。
/// </summary>
[RegisterPower]
public sealed class AnanlinPrisonIsHomePower : ManosabaPowerTemplate
{
    private bool _enteredCellThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 每回合开始：若身上没有【牢房】能力，则重新计算（允许本回合再次因给予【已缄默】进入牢房）
        _enteredCellThisTurn = false;
    }

    /// <summary>本回合给予敌人【已缄默】时调用：若本回合尚未进入过【牢房】，则进入并获得能量。</summary>
    internal async Task TryEnterCellOnSilencedGiven(PlayerChoiceContext choiceContext)
    {
        if (_enteredCellThisTurn) return;
        if (Owner.GetPower<AnanlinCellPower>() is not null) return;

        _enteredCellThisTurn = true;
        Flash();
        await PowerCmd.Apply<AnanlinCellPower>(choiceContext, Owner, 1, Owner, null);

        if (Amount > 0)
            await PlayerCmd.GainEnergy((int)Amount, Owner.Player);
    }
}