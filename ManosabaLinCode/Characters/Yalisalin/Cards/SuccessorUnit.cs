using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     后继机（0 费 技能・衍生）：
///     由「2887」生成，从亚里沙卡池与无色卡池中随机获得 1 张已升级的卡牌，然后从本场移除。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class SuccessorUnit()
    : ManosabaCardTemplate(0, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await ResolveSuccessor(choiceContext);

        // 「打出移除」：与「子弹」同款，直接在 OnPlay 里下移除命令，不挂组件。
        await CardPileCmd.RemoveFromCombat(this);
    }

    private async Task ResolveSuccessor(PlayerChoiceContext choiceContext)
    {
        if (Owner is not { } owner)
            return;

        var pools = new CardPoolModel[]
        {
            owner.Character.CardPool,
            ModelDb.CardPool<ColorlessCardPool>()
        };

        var options = CardCreationOptions
            .ForNonCombatWithUniformOdds(pools)
            .GetPossibleCards(owner);

        var card = CardFactory
            .GetDistinctForCombat(owner, options, 1, owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();

        if (card is null)
            return;

        CardCmd.Upgrade(card, CardPreviewStyle.None);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, owner);
    }
}
