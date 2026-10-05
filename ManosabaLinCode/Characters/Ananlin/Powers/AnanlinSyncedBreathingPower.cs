using ManosabaLin.Characters.Ananlin.Cards;

namespace ManosabaLin.Characters.Ananlin.Powers;

[RegisterPower]
public sealed class AnanlinSyncedBreathingPower : ManosabaPowerTemplate
{
    /// <summary>连续达成时获得的格挡（固定 6，不随升级变化）。</summary>
    private const int BlockGain = 6;

    private CardModel? _sourceCard;
    private CardType? _lastType;
    private int _streak;
    private int _requiredStreak = 3;
    private int _bonusDraws;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    protected override bool IsVisibleInternal => false;

    internal void Arm(CardModel sourceCard, int bonusDraws, int requiredStreak)
    {
        _sourceCard = sourceCard;
        _bonusDraws = bonusDraws;
        _requiredStreak = Math.Max(2, requiredStreak);
        Amount = 1;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == _sourceCard) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;

        if (_lastType == cardPlay.Card.Type)
            _streak++;
        else
        {
            _lastType = cardPlay.Card.Type;
            _streak = 1;
        }

        if (_streak < _requiredStreak) return;

        Flash();
        await CreatureCmd.GainBlock(Owner, BlockGain, ValueProp.Move, null);

        // 「每有 1 层【安心】，从抽牌堆或弃牌堆将 N 张【同类型】的牌加入手牌」
        // —— 同类型 = 达成这次连续的那个牌类型。
        if (_bonusDraws > 0 && _lastType is { } type)
        {
            await _sourceCard!.PullMatchingCardsToHand(
                choiceContext,
                _bonusDraws,
                card => AnanlinCardHelpers.IsPlayableCombatCard(card) && card.Type == type);
        }

        // ⚠️ 达成后**重置**连续计数而不是移除能力：卡面是「本回合当你连续打出 N 张同类型牌时」
        //（不含「下一次」），即本回合内可以**反复**达成。回合结束由 AfterSideTurnEnd 移除。
        _streak = 0;
        _lastType = null;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
            await PowerCmd.Remove(this);
    }
}
