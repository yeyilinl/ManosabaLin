using ManosabaLin.Characters.Ananlin.Relics;

namespace ManosabaLin.Characters.Ananlin.Powers;

/// <summary>
/// 【牢房】：每回合打出的第一张技能牌改为置于抽牌堆顶，且费用-1直到打出；
/// 回合结束时，若敌人【已缄默】，缄默替换意图数值+1；若打出攻击牌，离开【牢房】。
/// </summary>
[RegisterPower]
public sealed class AnanlinCellPower : ManosabaPowerTemplate
{
    private CardModel? _returnedSkill;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (_returnedSkill is not null) return cardLocation;
        if (card.Owner?.Creature != Owner) return cardLocation;
        if (card.Type != CardType.Skill) return cardLocation;
        if (cardLocation.pileType != PileType.Discard && cardLocation.pileType != PileType.Exhaust) return cardLocation;

        _returnedSkill = card;
        card.EnergyCost.AddThisTurnOrUntilPlayed(-1, reduceOnly: true);
        return new CardLocation(cardLocation.player, PileType.Draw, CardPilePosition.Top);
    }

    public override Task AfterModifyingCardPlayResultLocation(CardModel card, CardLocation cardLocation)
    {
        if (card == _returnedSkill)
            Flash();

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        // 打出攻击牌，离开【牢房】
        await PowerCmd.Remove(this);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;

        // 回合结束时，若敌人有【已缄默】，缄默替换意图数值+1
        var combatState = Owner.CombatState;
        if (combatState is not null && combatState.Enemies.Any(static e => e.IsAlive && e.GetPower<AnanlinSilencedPower>() is not null))
        {
            if (Owner.Player is { } player)
                AnanlinSilenceIntentManager.IncreaseSilenceGrowth(player);
        }

        // 然后离开【牢房】（离开后每回合重新计算）
        await PowerCmd.Remove(this);
    }
}