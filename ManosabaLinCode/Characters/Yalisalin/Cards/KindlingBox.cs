namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     火种盒（0 费 技能・稀有）：
///     生成 2 张「火种」加入手牌；升级后改为生成 4 张。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class KindlingBox()
    : ManosabaCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    private const int BaseCount = 2;
    private const int UpgradedCount = 4;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromCard<KindlingSparkToken>(false); }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner || CombatState is not { } combatState)
            return;

        var count = IsUpgraded ? UpgradedCount : BaseCount;
        for (var i = 0; i < count; i++)
        {
            var token = combatState.CreateCard<KindlingSparkToken>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, owner);
        }
    }
}
