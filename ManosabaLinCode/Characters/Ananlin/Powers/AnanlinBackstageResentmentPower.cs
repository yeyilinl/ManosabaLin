using ManosabaLin.Characters.Ananlin.Capabilities;
using ManosabaLin.Characters.Ananlin.Cards;
using ManosabaLin.Characters.Ananlin.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Ananlin.Powers;

[RegisterPower]
public sealed class AnanlinBackstageResentmentPower : ManosabaPowerTemplate
{
    private const int InitialSilenceCost = 13;

    private enum AuditionEffect
    {
        GeneratedCard,
        MarginPage,
        IntentRewrite,
        DrawAndMarkCard
    }

    [SavedProperty] public bool FirstAuditionEffectTriggersAgain { get; set; }
    [SavedProperty] public int RequiredSilenceCost { get; set; }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override int DisplayAmount
    {
        get
        {
            RefreshRequiredSilenceCostVars();
            return CurrentRequiredSilenceCost;
        }
    }

    /// <summary>
    ///     描述随「来源牌是否升级」切键（升级也是一种达成条件）。两侧通道都切，做法对齐「共犯」/
    ///     被缚的普罗米修斯。
    /// </summary>
    public override LocString Description
    {
        get
        {
            var description = new LocString("powers",
                FirstAuditionEffectTriggersAgain ? $"{Id.Entry}.descriptionEnhanced" : $"{Id.Entry}.description");
            description.Add(new IntVar("RequiredSilenceCost", CurrentRequiredSilenceCost));
            description.Add(new IntVar("SilenceCostIncrease", AnanlinBrainwashBacklashPower.BrainwashSilenceCostIncrease));
            return description;
        }
    }

    protected override string SmartDescriptionLocKey =>
        FirstAuditionEffectTriggersAgain
            ? $"{Id.Entry}.smartDescriptionEnhanced"
            : $"{Id.Entry}.smartDescription";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("RequiredSilenceCost", InitialSilenceCost),
        new IntVar("SilenceCostIncrease", AnanlinBrainwashBacklashPower.BrainwashSilenceCostIncrease)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AnanlinPeaceOfMindPower>(),
        HoverTipFactory.FromPower<SilentPower>(),
        HoverTipFactory.FromPower<AnanlinBrainwashPower>(),
        HoverTipFactory.FromPower<AnanlinBrainwashBacklashPower>(),
        HoverTipFactory.FromCard<BlankPage>(),
        HoverTipFactory.FromCard<MarginPage>()
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        var count = Math.Max(0, (int)(Owner.GetPower<AnanlinPeaceOfMindPower>()?.Amount ?? 0));
        if (count <= 0) return;

        // 消耗当前安心层数，再随机执行等量试镜效果
        var peace = Owner.GetPower<AnanlinPeaceOfMindPower>();
        if (peace is { Amount: > 0 })
            await PowerCmd.ModifyAmount(choiceContext, peace, -count, Owner, null);

        Flash();
        var first = RollEffect(player);
        await ExecuteEffect(choiceContext, first);
        if (FirstAuditionEffectTriggersAgain)
            await ExecuteEffect(choiceContext, first);

        for (var i = 1; i < count; i++)
            await ExecuteEffect(choiceContext, RollEffect(player));
    }

    private static AuditionEffect RollEffect(Player player)
    {
        var values = Enum.GetValues<AuditionEffect>();
        return player.RunState.Rng.CombatCardGeneration.NextItem(values);
    }

    private async Task ExecuteEffect(PlayerChoiceContext choiceContext, AuditionEffect effect)
    {
        switch (effect)
        {
            case AuditionEffect.GeneratedCard:
                await AddTemporaryRecordedCard();
                break;
            case AuditionEffect.MarginPage:
                await AddMarginPage(upgraded: true);
                break;
            case AuditionEffect.IntentRewrite:
                await SpendSilenceAndRewrite(choiceContext);
                break;
            case AuditionEffect.DrawAndMarkCard:
                await DrawAndMarkCard(choiceContext);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
        }
    }

    private async Task AddTemporaryRecordedCard()
    {
        if (Owner.Player is not { } player) return;
        if (Owner.CombatState is null) return;

        var sketchbook = player.Relics.OfType<AnansSketchbook>().FirstOrDefault();
        var card = sketchbook?.RollCombatCardFromRecordedPools();
        if (card is null)
        {
            await AddBlankPage(upgraded: false);
            return;
        }

        card.SetFreeIgnoringCardPlayConditions();
        card.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
    }

    private async Task AddMarginPage(bool upgraded)
    {
        if (Owner.Player is not { } player) return;
        if (Owner.CombatState is not { } combatState) return;

        var page = combatState.CreateCard<MarginPage>(player);
        if (upgraded)
            CardCmd.Upgrade(page);

        await CardPileCmd.AddGeneratedCardToCombat(page, PileType.Hand, player);
    }

    private async Task AddBlankPage(bool upgraded)
    {
        if (Owner.Player is not { } player) return;
        if (Owner.CombatState is not { } combatState) return;

        var blessedObject = player.Relics.OfType<BlessedObject>().FirstOrDefault();
        var page = blessedObject is not null
            ? blessedObject.CreateBlankPageOrReplacement(combatState, upgraded, player)
            : combatState.CreateCard<BlankPage>(player);
        if (blessedObject is null && upgraded)
            CardCmd.Upgrade(page);

        await CardPileCmd.AddGeneratedCardToCombat(page, PileType.Hand, player);
    }

    private async Task SpendSilenceAndRewrite(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is not { } player) return;

        var silenceCost = CurrentRequiredSilenceCost;
        var sketchbook = player.Relics.OfType<AnansSketchbook>().FirstOrDefault();
        if (sketchbook is null
            || sketchbook.CurrentSilence < silenceCost
            || !AnanlinSilenceIntentManager.CanForceBrainwash(player))
        {
            await AddBlankPage(upgraded: false);
            return;
        }

        var rewritten = await AnanlinSilenceIntentManager.ForceBrainwash(
            choiceContext,
            player,
            async () => await sketchbook.SpendSilence(choiceContext, silenceCost, null) >= silenceCost);
        if (!rewritten)
        {
            await AddBlankPage(upgraded: false);
            return;
        }

        IncreaseRequiredSilenceCost();
    }

    private int CurrentRequiredSilenceCost =>
        IsCanonical ? InitialSilenceCost : Math.Max(InitialSilenceCost, RequiredSilenceCost);

    private void IncreaseRequiredSilenceCost()
    {
        RequiredSilenceCost = CurrentRequiredSilenceCost + AnanlinBrainwashBacklashPower.BrainwashSilenceCostIncrease;
        RefreshRequiredSilenceCostVars();
        InvokeDisplayAmountChanged();
    }

    private void RefreshRequiredSilenceCostVars()
    {
        if (DynamicVars.TryGetValue("RequiredSilenceCost", out var requiredSilenceCost))
            requiredSilenceCost.BaseValue = CurrentRequiredSilenceCost;

        if (DynamicVars.TryGetValue("SilenceCostIncrease", out var silenceCostIncrease))
            silenceCostIncrease.BaseValue = AnanlinBrainwashBacklashPower.BrainwashSilenceCostIncrease;
    }

    private async Task DrawAndMarkCard(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is not { } player) return;

        await CardPileCmd.Draw(choiceContext, 1, player);
        var candidates = PileType.Hand.GetPile(player).Cards
            .Where(card => card != null && !card.Keywords.Contains(CardKeyword.Unplayable))
            .ToArray();
        if (candidates.Length == 0) return;

        var selected = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
        selected?.GetOrCreateCapability<AnanlinAuditionPeaceCapability>();
    }
}
