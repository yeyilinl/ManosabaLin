using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 未命名222（2 费技能・普通）：
/// 选择 1 张手牌消耗；若消耗的是原罪诅咒，额外再消耗 1 张手牌；
/// 若消耗的全都不是原罪诅咒，随机添加一张带原罪的原罪诅咒入手。
/// 升级：改为选择 1~2 张手牌消耗。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class SinOffering() : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("SelectCount", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var maxSelect = DynamicVars["SelectCount"].IntValue;

        var chosen = (await CardSelectCmd.FromHand(
            choiceContext,
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, maxSelect),
            null,
            this)).ToList();
        if (chosen.Count == 0) return;

        foreach (var card in chosen)
            await CardCmd.Exhaust(choiceContext, card);

        if (chosen.Any(c => c.HasComponent<Originalsin>()))
        {
            // 额外再消耗 1 张手牌
            var extra = (await CardSelectCmd.FromHand(
                choiceContext,
                owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
                c => !chosen.Contains(c),
                this)).FirstOrDefault();
            if (extra is not null)
                await CardCmd.Exhaust(choiceContext, extra);
        }
        else
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            var sin = AncientSinCardCatalog.CreateRandom(CombatState, owner, rng);
            sin.TryAddComponent(new Originalsin());
            await CardPileCmd.AddGeneratedCardToCombat(sin, PileType.Hand, owner);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["SelectCount"].UpgradeValueBy(1);
    }
}