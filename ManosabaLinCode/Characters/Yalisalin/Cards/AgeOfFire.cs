using ManosabaLin.Characters.Common.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     火之时代（1 费 能力・稀有）：
///     打出时，将【传火】【灭火】【夺火】三张卡加入手牌。
///     当你打出其中任意一张时，另外两张会从本场战斗中移除
///     —— 你选中的那条路，就是这一局的火之时代。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class AgeOfFire()
    : ManosabaCardTemplate(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    /// <summary>
    ///     三张分支牌的效果悬浮提示。
    ///     本来这里是 <c>HoverTipFactory.FromCard&lt;T&gt;(false)</c> 的牌面预览，但传火/灭火/夺火
    ///     改成「卡面只留风味文本」后（描述只剩 1 / 2 / 3），预览里也只会显示「1 / 2 / 3」，
    ///     看不到实际效果 ⇒ 改为直接用它们的 <c>_EFFECT</c> 键生成效果提示框。
    /// </summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return CardEffectHoverTipFactory.FromCard<LinkTheFire>("MANOSABA_LIN_CARD_LINK_THE_FIRE_EFFECT");
            yield return CardEffectHoverTipFactory.FromCard<ExtinguishFlame>("MANOSABA_LIN_CARD_EXTINGUISH_FLAME_EFFECT");
            yield return CardEffectHoverTipFactory.FromCard<UsurpTheFlame>("MANOSABA_LIN_CARD_USURP_THE_FLAME_EFFECT");
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner || CombatState is not { } combatState)
            return;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        var branches = new CardModel[]
        {
            combatState.CreateCard<LinkTheFire>(owner),
            combatState.CreateCard<ExtinguishFlame>(owner),
            combatState.CreateCard<UsurpTheFlame>(owner)
        };

        foreach (var branch in branches)
            await CardPileCmd.AddGeneratedCardToCombat(branch, PileType.Hand, owner);
    }

    /// <summary>
    ///     打出三张分支牌中的任意一张时，把另外两张（所有同名副本）从本场战斗中移除。
    ///     只处理打出者自己的牌堆，不会动队友手里的副本。
    /// </summary>
    internal static async Task RemoveOtherBranches(
        PlayerChoiceContext choiceContext,
        Player owner,
        CardModel played)
    {
        var others = owner.Piles
            .SelectMany(static pile => pile.Cards)
            .Where(card => !ReferenceEquals(card, played) && IsBranch(card))
            .ToList();

        foreach (var other in others)
            await CardPileCmd.RemoveFromCombat(other);
    }

    private static bool IsBranch(CardModel card)
    {
        return card is LinkTheFire or ExtinguishFlame or UsurpTheFlame;
    }
}
