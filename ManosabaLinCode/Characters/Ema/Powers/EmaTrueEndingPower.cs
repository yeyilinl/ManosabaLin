using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Emalin.Components;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MinionLib.RightClick;
using MinionLib.RightClick.Easy;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Ema.Powers;

[RegisterPower]
public class EmaTrueEndingPower : ManosabaPowerTemplate, IEasyRightClickablePower
{
    private const int AchieveCountTarget = 13;
    private const int InvokeCountTarget = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => int.Clamp(AchieveCountTarget - _achievedCards.Count, 0, AchieveCountTarget);


    private HashSet<string> _achievedCards = [];
    private Dictionary<string, int> _cardCounter = [];

    public IReadOnlySet<string> AchievedCards => _achievedCards;

    protected override void AfterCloned()
    {
        _achievedCards = [];
        _cardCounter = [];
    }

    public override async Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Pile == null && card.Owner.Creature == Owner && card.HasComponent<EmaTrueEndingTagComponent>())
        {
            if (_achievedCards.Add(card.Id.Entry))
            {
                InvokeDisplayAmountChanged();
                Flash();
                if (_achievedCards.Count == AchieveCountTarget)
                    await OnAchieve();
            }
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var id = card.Id.Entry;

        // 按卡名计数（含重放触发的 AfterCardPlayed），每个卡名只生效一次
        var count = _cardCounter.GetValueOrDefault(id, 0) + 1;
        _cardCounter[id] = count;

        // 已获得标记的卡不再重复触发
        if (card.HasComponent<EmaTrueEndingTagComponent>()) return;

        if (count == 1)
        {
            // 第1次打出：50%获得「魔女的记忆」标记，并获得重放与打出移除
            var rng = Owner.Player!.RunState.Rng.CombatCardGeneration;
            if (rng.NextFloat() < 0.5f)
                GrantTrueEndingTag(card);
        }
        else if (count == InvokeCountTarget)
        {
            // 第1次未获得，则第2次打出必定获得
            GrantTrueEndingTag(card);
        }
    }

    private static void GrantTrueEndingTag(CardModel card)
    {
        card.TryAddComponent(new EmaTrueEndingTagComponent());
        card.BaseReplayCount++;
    }


    private async Task OnAchieve()
    {
        if (!Owner.IsPlayer) return;
        var target = Owner.Player!.Deck.Cards.OfType<EmaEnding>().FirstOrDefault();

        target?.AddComponent(new EmaTrueEndingRewardComponent(1));
    }

    public async Task OnRightClick(PlayerChoiceContext choiceContext, RightClickContext clickContext)
    {
        if (!LocalContext.IsMe(clickContext.Player)) return;

        if (_achievedCards.Count == 0) return;

        var cards = _achievedCards
            .Select(cardId => ModelDb.GetById<CardModel>(new ModelId("CARD", cardId)))
            .ToList();

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0)
        {
            Cancelable = true,
            RequireManualConfirmation = false
        };

        var screen = NDeckCardSelectScreen.Create(cards, prefs);
        NOverlayStack.Instance!.Push(screen);
    }
}
