using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     第二次点燃（1 费 技能・罕见）：
///     随机将本回合被余火烧掉的 1 张牌返回手牌；升级后返回的牌本次费用变为 0。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class SecondKindling()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        if (!YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin))
            return;

        var candidates = hairpin.BurnedCardsThisTurn;
        if (candidates.Length == 0)
            return;

        var picked = owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (picked == null)
            return;

        await CardPileCmd.Add(picked, PileType.Hand);

        if (IsUpgraded)
            picked.EnergyCost.SetThisTurnOrUntilPlayed(0, reduceOnly: true);
    }
}
