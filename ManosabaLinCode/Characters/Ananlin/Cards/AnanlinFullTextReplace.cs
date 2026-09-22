using ManosabaLin.Characters.Ananlin.Powers;
using ManosabaLin.Characters.Ananlin.Relics;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinFullTextReplace()
    : ManosabaCardTemplate(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromCard<BlankPage>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            BuildNameOptions(),
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, 1))).FirstOrDefault();
        if (selected is null) return;

        var toExhaust = GetCardsInMainCombatPiles()
            .Where(card => card.Id == selected.Id)
            .ToArray();
        if (toExhaust.Length == 0) return;

        var exhausted = 0;
        foreach (var card in toExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
            exhausted++;
        }

        // 每消耗1张牌，将1张【空白书页+】加入手牌
        for (var i = 0; i < exhausted; i++)
            await this.AddBlankPageToHand(true);

        // 若消耗2张，获得1张当前缄默替换意图牌（数值=当前意图池且固定）
        for (var i = 0; i < exhausted / 2; i++)
            await AnanlinSilenceIntentManager.AddRandomReplacementIntentCardToHand(choiceContext, Owner);

        // 若消耗3张，改写一次敌人意图
        for (var i = 0; i < exhausted / 3; i++)
            await AnanlinSilenceIntentManager.ForceBrainwash(choiceContext, Owner);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }

    private IReadOnlyList<CardModel> BuildNameOptions()
    {
        var seen = new HashSet<ModelId>();
        return GetCardsInMainCombatPiles()
            .Where(card => seen.Add(card.Id))
            .OrderBy(card => card.Title.ToString())
            .ToArray();
    }

    private IEnumerable<CardModel> GetCardsInMainCombatPiles()
    {
        return PileType.Hand.GetPile(Owner).Cards
            .Concat(PileType.Draw.GetPile(Owner).Cards)
            .Concat(PileType.Discard.GetPile(Owner).Cards);
    }
}
