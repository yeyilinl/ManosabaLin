using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.HiroKeywords;
using ManosabaLin.Characters.Hiro.Capabilities;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MinionLib.Component.Core;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models.Capabilities;
using Godot;

namespace ManosabaLin.Characters.Hiro.Cards;

[RegisterCard(typeof(HirolinCardPool))]
public sealed class SamePlaceTrace() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    private const string CopyCountKey = "CopyCount";
    private const string AutoPlayProgressKey = "AutoPlayProgress";
    private const string AutoPlayRequirementKey = "AutoPlayRequirement";
    private const string ClueCountKey = "ClueCount";
    private const string RequiredCluesKey = "RequiredClues";
    private const int AutoPlayRequirement = 3;
    private const int RequiredClues = 3;

    private int _autoPlayProgress;
    private int _clueCount;
    private bool _completionTriggered;

    /// <summary>
    /// 本次打出时刚被加入抽牌堆的副本标记：这些副本不能立刻被本次打出的【轮回】触发，
    /// 只能触发原本就在抽牌堆的卡牌。下一次手动打出本牌时统一清除该标记。
    /// </summary>
    private bool _justAddedToDrawPile;

    internal bool JustAddedToDrawPile => _justAddedToDrawPile;

    internal void ClearJustAddedToDrawPileFlag() => _justAddedToDrawPile = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return TransmigrationRules.TransmigrationCardKeyword;
        }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard<SamePlaceTruth>();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new IntVar(CopyCountKey, 1);
            yield return new CardsVar(1);
            yield return new EnergyVar(1);
            yield return new IntVar(AutoPlayProgressKey, _autoPlayProgress);
            yield return new IntVar(AutoPlayRequirementKey, AutoPlayRequirement);
            yield return new IntVar(ClueCountKey, _clueCount);
            yield return new IntVar(RequiredCluesKey, RequiredClues);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        // 手动打出时：让此前加入抽牌堆的副本恢复为可被【轮回】触发；
        // 本次刚加入的副本在下方 AddTemporaryCopiesToDrawPile 里打上“刚加入”标记，
        // 使【轮回】只能触发原本就在抽牌堆的卡牌。
        if (!cardPlay.IsAutoPlay)
        {
            foreach (var trace in GetAllSamePlaceTraceCards())
            {
                trace.ClearJustAddedToDrawPileFlag();
            }
        }

        await AddTemporaryCopiesToDrawPile();
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        if (!cardPlay.IsAutoPlay)
        {
            return;
        }

        await GrantTransmigrationToHandCard(choiceContext);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        await AdvanceAutoPlayProgress(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars[CopyCountKey].BaseValue++;
    }

    private async Task AddTemporaryCopiesToDrawPile()
    {
        var progress = SnapshotProgress();
        SetProgressForAll(progress.AutoPlayProgress, progress.ClueCount, progress.CompletionTriggered);

        for (var i = 0; i < DynamicVars[CopyCountKey].IntValue; i++)
        {
            var copy = CombatState.CreateCard<SamePlaceTrace>(Owner);
            for (var upgradeLevel = 0; upgradeLevel < CurrentUpgradeLevel; upgradeLevel++)
            {
                copy.UpgradeInternal();
            }

            copy.SetProgressLocal(progress.AutoPlayProgress, progress.ClueCount, progress.CompletionTriggered);
            // 刚加入抽牌堆：本次打出的【轮回】不能立刻触发它
            copy._justAddedToDrawPile = true;
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Draw, Owner, CardPilePosition.Random);
        }
    }

    private async Task GrantTransmigrationToHandCard(PlayerChoiceContext choiceContext)
    {
        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
            card => !ReferenceEquals(card, this) && !TransmigrationRules.HasTransmigration(card),
            this);

        foreach (var selectedCard in selectedCards)
        {
            selectedCard.AddModKeyword(TransmigrationRules.TransmigrationCardKeyword);
            // 真相能力：给予卡牌轮回关键词时自动追加真相组件
            if (TruthPower.HasTruthPower(Owner))
            {
                selectedCard.GetOrCreateCapability<TruthComponentCapability>();
            }
        }
    }

    private async Task AdvanceAutoPlayProgress(PlayerChoiceContext choiceContext)
    {
        var progress = SnapshotProgress();
        if (progress.CompletionTriggered)
        {
            SetProgressForAll(progress.AutoPlayProgress, progress.ClueCount, true);
            return;
        }

        var autoPlayProgress = progress.AutoPlayProgress + 1;
        var clueCount = progress.ClueCount;

        if (autoPlayProgress >= AutoPlayRequirement)
        {
            autoPlayProgress -= AutoPlayRequirement;
            clueCount++;
        }

        var completed = clueCount >= RequiredClues;
        SetProgressForAll(autoPlayProgress, clueCount, completed);
        if (!completed)
        {
            return;
        }

        await RemoveAllSamePlaceTrace(choiceContext);
        await GainAncientPlaceholder();
    }

    private async Task RemoveAllSamePlaceTrace(PlayerChoiceContext choiceContext)
    {
        // 先打断当前正在结算中的同一处痕迹（Play 牌堆）：立即移出战斗并手动清理视觉节点，
        // 防止引擎在结算完成后把它放回弃牌堆，保证生成旧识疑影后战斗内没有一张同一处痕迹。
        foreach (var trace in GetAllSamePlaceTraceCards()
                     .Where(card => card.Pile is { IsCombatPile: true } && card.Pile.Type == PileType.Play)
                     .ToList())
        {
            var node = NCard.FindOnTable(trace);
            await CardPileCmd.RemoveFromCombat(trace, skipVisuals: true);
            if (node != null && GodotObject.IsInstanceValid(node) && !node.IsQueuedForDeletion())
            {
                node.QueueFree();
            }
        }

        // 其余战斗牌堆（手牌/抽牌堆/弃牌堆/消耗牌堆）中的同一处痕迹：
        // 按本地化“移除所有同一处痕迹”，全部移出战斗（不再进入消耗牌堆）。
        // 手牌需要先移除对应手牌 UI 槽位，避免残留空手牌位。
        foreach (var trace in GetAllSamePlaceTraceCards()
                     .Where(card => card.Pile?.Type is PileType.Hand or PileType.Draw or PileType.Discard or PileType.Exhaust)
                     .ToList())
        {
            if (trace.Pile?.Type == PileType.Hand && NPlayerHand.Instance?.GetCardHolder(trace) is { } holder)
            {
                NPlayerHand.Instance.RemoveCardHolder(holder);
            }

            await CardPileCmd.RemoveFromCombat(trace, skipVisuals: true);
        }
    }

    private async Task GainAncientPlaceholder()
    {
        // 注意：RemoveAllSamePlaceTrace 会把正在结算中的这张同一处痕迹也移出战斗，
        // 它的 CombatState 随即变为 null，不能再用 this.CombatState，否则空引用。
        if (Owner?.Creature.CombatState is not { } combatState)
        {
            return;
        }

        var ancientCard = combatState.CreateCard<SamePlaceTruth>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(ancientCard, PileType.Hand, Owner);
    }

    private (int AutoPlayProgress, int ClueCount, bool CompletionTriggered) SnapshotProgress()
    {
        var traceCards = GetAllSamePlaceTraceCards();
        if (traceCards.Count == 0)
        {
            return (_autoPlayProgress, _clueCount, _completionTriggered);
        }

        return (
            traceCards.Max(card => card._autoPlayProgress),
            traceCards.Max(card => card._clueCount),
            traceCards.Any(card => card._completionTriggered));
    }

    private void SetProgressForAll(int autoPlayProgress, int clueCount, bool completionTriggered)
    {
        foreach (var trace in GetAllSamePlaceTraceCards())
        {
            trace.SetProgressLocal(autoPlayProgress, clueCount, completionTriggered);
        }
    }

    private void SetProgressLocal(int autoPlayProgress, int clueCount, bool completionTriggered)
    {
        _autoPlayProgress = autoPlayProgress;
        _clueCount = clueCount;
        _completionTriggered = completionTriggered;
        RefreshProgressVars();
    }

    private void RefreshProgressVars()
    {
        DynamicVars[AutoPlayProgressKey].BaseValue = _autoPlayProgress;
        DynamicVars[ClueCountKey].BaseValue = _clueCount;
    }

    private List<SamePlaceTrace> GetAllSamePlaceTraceCards()
    {
        var traceCards = Owner.PlayerCombatState.AllCards.OfType<SamePlaceTrace>().ToList();
        if (!traceCards.Contains(this))
        {
            traceCards.Add(this);
        }

        return traceCards;
    }
}
