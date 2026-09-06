using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 原罪市场（2 费技能・稀有）：
/// 展示战斗中全部带原罪的卡（手牌/抽牌堆/弃牌堆）。选 3 张卡，每张随机触发一种效果：
/// ①立刻宽恕 ②回你手 ③自动打出一次。
/// 升级：改为选 4 张。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class SinMarket() : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("SelectCount", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var rng = owner.RunState.Rng.CombatCardGeneration;

        var pool = PileType.Hand.GetPile(owner).Cards
            .Concat(PileType.Draw.GetPile(owner).Cards)
            .Concat(PileType.Discard.GetPile(owner).Cards)
            .Where(c => c.HasComponent<Originalsin>())
            .Distinct()
            .ToList();
        if (pool.Count == 0) return;

        var count = Math.Min(DynamicVars["SelectCount"].IntValue, pool.Count);
        var chosen = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            pool,
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, count, count))).ToList();

        foreach (var card in chosen)
        {
            var component = (card as IComponentsCardModel)?.GetComponent<Originalsin>();
            var roll = rng.NextInt(3);
            switch (roll)
            {
                // ①立刻宽恕
                case 0:
                    if (component is not null)
                        await component.Forgive(choiceContext);
                    break;
                // ②回你手（已在手牌则无事发生）
                case 1:
                    if (card.Pile?.Type != PileType.Hand)
                        await CardPileCmd.Add(card, PileType.Hand);
                    break;
                // ③自动打出一次
                case 2:
                    await CardCmd.AutoPlay(choiceContext, card, null);
                    break;
            }
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["SelectCount"].UpgradeValueBy(1);
    }
}