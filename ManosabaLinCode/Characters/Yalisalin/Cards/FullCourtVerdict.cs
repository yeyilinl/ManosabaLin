using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 全席宣判（2 费技能・稀有）：
/// 从所有牌（抽牌堆 / 手牌 / 弃牌堆）中选择任意张原罪进行自惩，其余原罪自动宽恕。
/// 宽恕次数 &gt; 自惩 → 抽差值张牌；自惩 ≥ 宽恕 → 获得差值能量，失去差值×4 生命。
/// 升级：失去生命改为 ×3。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class FullCourtVerdict() : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("HpMult", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var creature = owner.Creature;

        // 「所有牌」= 抽牌堆 + 手牌 + 弃牌堆（不含消耗堆）。
        var allSins = new List<CardModel>();
        foreach (var pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
        {
            foreach (var card in pileType.GetPile(owner).Cards)
            {
                if (card.HasComponent<Originalsin>())
                    allSins.Add(card);
            }
        }

        if (allSins.Count == 0) return;

        // 选择要自惩的原罪卡，未选择的自动宽恕。
        // 候选可能来自抽牌堆 / 弃牌堆，故用网格选择而非手牌选择。
        var punishCards = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            allSins,
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, allSins.Count))).ToHashSet();

        var forgiveCount = 0;
        var punishCount = 0;

        foreach (var sin in allSins)
        {
            if ((sin as IComponentsCardModel)?.GetComponent<Originalsin>() is not { } component) continue;

            if (punishCards.Contains(sin))
            {
                await component.Punish(choiceContext);
                punishCount++;
            }
            else
            {
                await component.Forgive(choiceContext);
                forgiveCount++;
            }
        }

        var diff = Math.Abs(forgiveCount - punishCount);
        if (forgiveCount > punishCount)
        {
            if (diff > 0)
                await CardPileCmd.Draw(choiceContext, diff, owner);
        }
        else if (punishCount >= forgiveCount)
        {
            if (diff > 0)
            {
                await PlayerCmd.GainEnergy(diff, owner);
                await CreatureCmd.Damage(
                    choiceContext,
                    creature,
                    diff * DynamicVars["HpMult"].BaseValue,
                    ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                    this,
                    null);
            }
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["HpMult"].UpgradeValueBy(-1);
    }
}
