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
    private readonly Dictionary<CardModel, int> _immediatelyAppliedCostOffsets = [];
    private readonly HashSet<CardModel> _appliedTemporaryCostOffsets = [];
    private readonly List<YalisalinFireRightClickRequest> _rightClickQueue = [];
    private readonly List<YalisalinAppliedFireRightClick> _appliedRightClicks = [];
    private readonly List<int> _rightClickRecords = [];
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

    /// <summary>
    ///     玩家在余火选卡界面上做过的「添火/跳过」记录（本机 UI 侧写入）。
    ///
    ///     元素含义：<c>0</c> = 跳过一次添火；<c>n &gt; 0</c> = 把这次添火作用在 <c>ChoiceOptions[n - 1]</c> 上。
    ///     这份列表会随玩家选择一起同步给对手，两端用同一份索引回放，
    ///     因此它只是「待发送的输入」，不是 run state，不参与校验和。
    /// </summary>
    public IReadOnlyList<int> RightClickRecords => _rightClickRecords;

    /// <summary>
    ///     还没被「添火」或「跳过」消费掉的强化请求数量。
    ///     右键与「跳过本次添火」按钮都用它判断还能不能继续操作。
    /// </summary>
    public int RemainingRightClickCount => Math.Max(0, _rightClickQueue.Count - _rightClickRecords.Count);

    public IReadOnlyList<CardModel> ExclusiveBurnCards => _exclusiveBurnCards;

    private IEnumerable<YalisalinFireRightClickRequest> RemainingRightClickRequests =>
        _rightClickQueue.Skip(_rightClickRecords.Count);

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

    /// <summary>
    ///     记录一次「把队首添火作用到这张牌上」。此时**只登记、不生效**——
    ///     真正的效果要等到 <see cref="ApplyRightClickRecords" /> 在两端用同一份索引回放时才产生。
    /// </summary>
    public void RecordRightClickApplication(CardModel card)
    {
        var index = ChoiceOptions.IndexOf(card);
        if (index < 0)
            return;

        _rightClickRecords.Add(index + 1);
    }

    /// <summary>
    ///     记录一次「跳过本次添火」：只推进队首，不产生任何效果。
    /// </summary>
    public void RecordRightClickSkip()
    {
        _rightClickRecords.Add(0);
    }

    /// <summary>
    ///     按同步过来的右键记录回放添火强化。
    ///
    ///     本机走的是「自己刚发出去的那份列表」，远端走的是收到的那份，两者内容必须一致，
    ///     因此 <see cref="AppliedRightClicks" /> 与各种费用偏移都会在两端得到完全相同的结果。
    /// </summary>
    public void ApplyRightClickRecords(IReadOnlyList<int> records)
    {
        foreach (var code in records)
        {
            if (_rightClickQueue.Count == 0)
                break;

            if (code <= 0)
            {
                _rightClickQueue.RemoveAt(0);
                continue;
            }

            var index = code - 1;
            if (index < 0 || index >= ChoiceOptions.Count)
            {
                _rightClickQueue.RemoveAt(0);
                continue;
            }

            ApplyNextRightClick(ChoiceOptions[index]);
        }
    }

    private void ApplyNextRightClick(CardModel card)
    {
        var request = _rightClickQueue[0];
        _rightClickQueue.RemoveAt(0);
        _appliedRightClicks.Add(new YalisalinAppliedFireRightClick(request.Kind, card, request.Source));

        if (request.Kind == YalisalinFireRightClickKind.UnneededGoodChildCostUp)
        {
            AddTemporaryCostOffset(card, 1);
            // 「不被需要的好孩子」的添火效果要求 +1 是一次真实的临时费用变化，
            // 所以在这里就立刻落到卡牌本次费用上，而不是等这张牌被打出时才生效。
            // 注意：这一步现在发生在选择界面关闭之后（回放阶段），界面里的费用不会即时刷新。
            ApplyCostOffsetImmediately(card, 1);
        }
    }

    public string SelectionPromptText
    {
        get
        {
            if (RemainingRightClickCount == 0)
                return Text("selectionScreenPrompt");

            // 未悬停：汇总显示所有待应用强化的效果，让玩家直接看到右键能做什么。
            // 同一类型只显示一次，避免第五卡多次计数时重复刷屏。
            var seen = new HashSet<YalisalinFireRightClickKind>();
            return string.Join(
                "；",
                RemainingRightClickRequests
                    .Where(request => seen.Add(request.Kind))
                    .Select(request => request.Prompt));
        }
    }

    public string SelectionPromptTextFor(CardModel? hoveredCard)
    {
        if (RemainingRightClickCount == 0)
            return Text("selectionScreenPrompt");

        if (hoveredCard == null)
            return SelectionPromptText;

        // 悬停某候选牌：逐条列出当前所有待应用强化对该牌的实际效果
        var seen = new HashSet<YalisalinFireRightClickKind>();
        return string.Join(
            "；",
            RemainingRightClickRequests
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
        if (_appliedTemporaryCostOffsets.Contains(card))
            return 0;

        // 已经即时落到卡牌真实费用上的部分，不再重复计入待应用偏移。
        return GetTemporaryCostOffset(card) - _immediatelyAppliedCostOffsets.GetValueOrDefault(card);
    }

    public void ApplyTemporaryCostOffset(CardModel card)
    {
        if (!_appliedTemporaryCostOffsets.Add(card))
            return;

        // 即时应用过的部分不再重复加，只补差额（例如 SeparatedEnds 的 -1）。
        var offset = GetTemporaryCostOffset(card) - _immediatelyAppliedCostOffsets.GetValueOrDefault(card);
        if (offset != 0)
            card.EnergyCost.AddThisTurnOrUntilPlayed(offset);
    }

    /// <summary>
    ///     把一个费用偏移立刻应用到卡牌本次的真实费用上（本次 / 打出前失效），
    ///     并记账，避免 <see cref="ApplyTemporaryCostOffset" /> 随后重复计入。
    /// </summary>
    private void ApplyCostOffsetImmediately(CardModel card, int amount)
    {
        if (amount == 0)
            return;

        card.EnergyCost.AddThisTurnOrUntilPlayed(amount);
        _immediatelyAppliedCostOffsets[card] =
            _immediatelyAppliedCostOffsets.GetValueOrDefault(card) + amount;
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
