using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Hiro.Cards;

namespace ManosabaLin.Characters.Yalisalin.Components;

public enum YalisalinFireComponentBurnMode
{
    Exhaust,
    RemoveFromCombat,
    None
}

public enum YalisalinFireRightClickKind
{
    PainKeeper,
    UnneededGoodChildCostUp,
    FifthSelfProof
}

public readonly record struct YalisalinFireRightClickRequest(
    YalisalinFireRightClickKind Kind,
    string Prompt,
    CardModel? Source = null);

public readonly record struct YalisalinAppliedFireRightClick(
    YalisalinFireRightClickKind Kind,
    CardModel Card,
    CardModel? Source = null);

public sealed class YalisalinFireComponentContext
{
    private const string LocPrefix = "ManosabaLin.YalisalinFireComponent";

    private readonly Dictionary<CardModel, PileType> _originPiles = [];
    private readonly Dictionary<CardModel, int> _temporaryCostOffsets = [];
    private readonly HashSet<CardModel> _appliedTemporaryCostOffsets = [];
    private readonly List<YalisalinFireRightClickRequest> _rightClickQueue = [];
    private readonly List<YalisalinAppliedFireRightClick> _appliedRightClicks = [];
    private readonly List<CardModel> _exclusiveBurnCards = [];
    private readonly List<CardModel> _burnedCards = [];

    internal YalisalinFireComponentContext(
        Player owner,
        CardPlay cardPlay,
        YalisalinFireComponentCapability component,
        bool sourceAlreadyPlaying = true,
        bool countsAsManualUse = true)
    {
        Owner = owner;
        CardPlay = cardPlay;
        Component = component;
        SourceCard = cardPlay.Card;
        Target = cardPlay.Target;
        SourceAlreadyPlaying = sourceAlreadyPlaying;
        CountsAsManualUse = countsAsManualUse;
    }

    public Player Owner { get; }
    public CardPlay CardPlay { get; }
    public YalisalinFireComponentCapability Component { get; }
    public CardModel SourceCard { get; }
    public Creature? Target { get; set; }
    public List<CardModel> ConnectionPool { get; } = [];
    public List<CardModel> ChoiceOptions { get; } = [];
    public Dictionary<string, object> CustomData { get; } = [];

    public CardModel? LinkedCard { get; set; }
    public CardModel? ChosenCard { get; set; }
    public CardModel? BurnedCard { get; set; }
    public IReadOnlyList<CardModel> BurnedCards => _burnedCards;
    public IReadOnlyList<YalisalinAppliedFireRightClick> AppliedRightClicks => _appliedRightClicks;
    public IReadOnlyList<YalisalinFireRightClickRequest> PendingRightClicks => _rightClickQueue;
    public IReadOnlyList<CardModel> ExclusiveBurnCards => _exclusiveBurnCards;

    public bool ShouldResolve { get; set; } = true;
    public bool ShouldPrompt { get; set; } = true;
    public bool ShouldAutoPlayLinkedChoice { get; set; } = true;
    public bool ShouldAutoPlaySourceChoice { get; set; }
    public bool ShouldSkipSourceCardCore { get; set; }
    public bool SkipBurnVisuals { get; set; }
    public bool SourceAlreadyPlaying { get; }
    public bool CountsAsManualUse { get; set; }
    public bool ChoiceCompleted { get; internal set; }
    public bool BurnOnlyExclusiveCards { get; set; }
    public YalisalinFireComponentBurnMode BurnMode { get; set; } = YalisalinFireComponentBurnMode.Exhaust;

    public void AddConnectionCandidate(CardModel card)
    {
        if (card == SourceCard) return;
        if (SamePlaceTruth.IsSelectionLocked(card)) return;
        if (ConnectionPool.Contains(card)) return;

        ConnectionPool.Add(card);
        _originPiles[card] = card.Pile?.Type ?? PileType.None;
    }

    public void ReplaceConnectionPool(IEnumerable<CardModel> cards)
    {
        ConnectionPool.Clear();
        foreach (var card in cards)
            AddConnectionCandidate(card);
    }

    public void AddChoiceOption(CardModel card)
    {
        if (SamePlaceTruth.IsSelectionLocked(card)) return;
        if (ChoiceOptions.Contains(card)) return;

        ChoiceOptions.Add(card);
        _originPiles.TryAdd(card, card.Pile?.Type ?? PileType.None);
    }

    public void AddExclusiveBurnCard(CardModel card)
    {
        AddChoiceOption(card);
        if (!_exclusiveBurnCards.Contains(card))
            _exclusiveBurnCards.Add(card);
    }

    public void AddRightClickRequest(YalisalinFireRightClickRequest request)
    {
        _rightClickQueue.Add(request);
    }

    public bool TryApplyNextRightClick(CardModel card)
    {
        if (_rightClickQueue.Count == 0)
            return false;

        var request = _rightClickQueue[0];
        _rightClickQueue.RemoveAt(0);
        _appliedRightClicks.Add(new YalisalinAppliedFireRightClick(request.Kind, card, request.Source));

        if (request.Kind == YalisalinFireRightClickKind.UnneededGoodChildCostUp)
            AddTemporaryCostOffset(card, 1);

        return true;
    }

    /// <summary>
    /// 跳过当前队首的这次添火强化（不应用到任何牌），让下一个允许的添火效果顶上。
    /// 返回是否成功（队列非空才能跳过）。
    /// </summary>
    public bool SkipNextRightClick()
    {
        if (_rightClickQueue.Count == 0)
            return false;

        _rightClickQueue.RemoveAt(0);
        return true;
    }

    public string SelectionPromptText
    {
        get
        {
            if (_rightClickQueue.Count == 0)
                return Text("selectionScreenPrompt");

            // 未悬停：汇总显示所有待应用强化的效果，让玩家直接看到右键能做什么。
            // 同一类型只显示一次，避免第五卡多次计数时重复刷屏。
            var seen = new HashSet<YalisalinFireRightClickKind>();
            return string.Join(
                "；",
                _rightClickQueue
                    .Where(request => seen.Add(request.Kind))
                    .Select(request => request.Prompt));
        }
    }

    public string SelectionPromptTextFor(CardModel? hoveredCard)
    {
        if (_rightClickQueue.Count == 0)
            return Text("selectionScreenPrompt");

        if (hoveredCard == null)
            return SelectionPromptText;

        // 悬停某候选牌：逐条列出当前所有待应用强化对该牌的实际效果
        var seen = new HashSet<YalisalinFireRightClickKind>();
        return string.Join(
            "；",
            _rightClickQueue
                .Where(request => seen.Add(request.Kind))
                .Select(request => RightClickPromptFor(request.Kind, hoveredCard)));
    }

    private static string RightClickPromptFor(YalisalinFireRightClickKind kind, CardModel card)
    {
        switch (kind)
        {
            case YalisalinFireRightClickKind.PainKeeper:
                return card.Type switch
                {
                    CardType.Attack => Text("rightClick.painKeeper.hover.attack"),
                    CardType.Skill => Text("rightClick.painKeeper.hover.skill"),
                    CardType.Power => Text("rightClick.painKeeper.hover.power"),
                    _ => Text("rightClick.generic.hover")
                };
            case YalisalinFireRightClickKind.UnneededGoodChildCostUp:
                return Text("rightClick.unneededGoodChild.prompt");
            case YalisalinFireRightClickKind.FifthSelfProof:
                return card.Type switch
                {
                    CardType.Attack => Text("rightClick.fifthSelfProof.hover.attack"),
                    CardType.Skill => Text("rightClick.fifthSelfProof.hover.skill"),
                    CardType.Power => Text("rightClick.fifthSelfProof.hover.power"),
                    _ => Text("rightClick.generic.hover")
                };
            default:
                return Text("rightClick.generic.hover");
        }
    }

    public static string Text(string suffix, params (string Name, decimal Value)[] variables)
    {
        var loc = new LocString("cards", $"{LocPrefix}.{suffix}");
        foreach (var (name, value) in variables)
            loc.Add(name, value);

        return loc.GetFormattedText();
    }

    public void AddTemporaryCostOffset(CardModel card, int amount)
    {
        if (amount == 0) return;

        _temporaryCostOffsets[card] = _temporaryCostOffsets.GetValueOrDefault(card) + amount;
    }

    public int GetTemporaryCostOffset(CardModel card)
    {
        return _temporaryCostOffsets.GetValueOrDefault(card);
    }

    public int GetPendingTemporaryCostOffset(CardModel card)
    {
        return _appliedTemporaryCostOffsets.Contains(card)
            ? 0
            : GetTemporaryCostOffset(card);
    }

    public void ApplyTemporaryCostOffset(CardModel card)
    {
        if (!_appliedTemporaryCostOffsets.Add(card))
            return;

        var offset = GetTemporaryCostOffset(card);
        if (offset != 0)
            card.EnergyCost.AddThisTurnOrUntilPlayed(offset);
    }

    public int GetEffectiveCost(CardModel card, int additionalTemporaryOffset = 0)
    {
        if (card.EnergyCost.CostsX)
            return Math.Max(0, card.Owner.PlayerCombatState?.Energy ?? 0);

        return Math.Max(
            0,
            card.EnergyCost.GetWithModifiers(CostModifiers.All)
            + GetPendingTemporaryCostOffset(card)
            + additionalTemporaryOffset);
    }

    public void MarkBurned(CardModel card)
    {
        if (!_burnedCards.Contains(card))
            _burnedCards.Add(card);
    }

    public PileType GetOriginPile(CardModel card)
    {
        return _originPiles.GetValueOrDefault(card, card.Pile?.Type ?? PileType.None);
    }
}
