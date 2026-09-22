using ManosabaLin.Characters.Ananlin.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Ananlin.Powers;

/// <summary>
/// 不作答：回合结束时，若本回合没有打出过攻击牌，【缄默】替换意图数值+1。
/// </summary>
[RegisterPower]
public sealed class AnanlinNoAnswerPower : ManosabaPowerTemplate
{
    private bool _playedAttackThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        _playedAttackThisTurn = false;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return Task.CompletedTask;
        if (cardPlay.Card.Type == CardType.Attack)
            _playedAttackThisTurn = true;

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        if (_playedAttackThisTurn) return;

        Flash();
        if (Owner.Player is { } player)
            AnanlinSilenceIntentManager.IncreaseSilenceGrowth(player);
    }
}