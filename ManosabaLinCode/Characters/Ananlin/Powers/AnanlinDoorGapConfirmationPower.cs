using ManosabaLin.Characters.Ananlin.Cards;
using ManosabaLin.Characters.Ananlin.Relics;

namespace ManosabaLin.Characters.Ananlin.Powers;

[RegisterPower]
public sealed class AnanlinDoorGapConfirmationPower : ManosabaPowerTemplate
{
    private CardModel? _canonicalCard;
    private int _bonusBlock;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    protected override bool IsVisibleInternal => false;

    internal void Track(CardModel card, int bonusBlock)
    {
        _canonicalCard = card.CanonicalInstance;
        _bonusBlock = bonusBlock;
        Amount = 1;
    }

    public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        if (_canonicalCard is null || !HasLostPeaceThisTurn(player))
        {
            await PowerCmd.Remove(this);
            return;
        }

        // 直接打出被记录的牌本身（不生成复制）
        var handCard = PileType.Hand.GetPile(player).Cards
            .FirstOrDefault(card => card.CanonicalInstance == _canonicalCard);
        if (handCard is null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        if (_bonusBlock > 0)
        {
            var bonus = await PowerCmd.Apply<AnanlinDoorGapBlockBonusPower>(
                choiceContext,
                Owner,
                _bonusBlock,
                Owner,
                handCard);
            bonus?.Track(handCard, _bonusBlock);
        }

        Flash();
        await AnanlinCardHelpers.ResolveAsFreeCardEffect(
            choiceContext,
            handCard,
            skipCardPileVisuals: false,
            removeFromCombatAfterPlay: false);
        await PowerCmd.Remove(this);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
            await PowerCmd.Remove(this);
    }

    private bool HasLostPeaceThisTurn(Player player)
    {
        return player.Relics.OfType<AnansSketchbook>().FirstOrDefault()?.PeaceLostThisTurn == true;
    }
}
