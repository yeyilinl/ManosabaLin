using ManosabaLin.Characters.Common.AncientCurses.Powers;
using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 对照伤（2 费攻击・罕见）：
/// 造成 8 + 本场宽恕次数×2 点伤害。若手牌有原罪，可再选 1 张立刻宽恕，但本牌伤害减半。
/// 升级：费用 -1。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class ContrastWound() : ManosabaCardTemplate(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("DamageBase", 8),
        new IntVar("PerForgive", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var owner = Owner;
        var creature = owner.Creature;

        var forgiveCount = creature.GetPower<OriginalsinForgivenessCounterPower>()?.Amount ?? 0m;
        var damage = DynamicVars["DamageBase"].BaseValue + forgiveCount * DynamicVars["PerForgive"].BaseValue;

        // 若手有原罪：可再选 1 张立刻宽恕，但本牌伤害减半
        var handSins = PileType.Hand.GetPile(owner).Cards
            .Where(c => c.HasComponent<Originalsin>())
            .ToList();
        if (handSins.Count > 0)
        {
            var selected = (await CardSelectCmd.FromHand(
                choiceContext,
                owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
                c => c.HasComponent<Originalsin>(),
                this)).FirstOrDefault();

            if (selected != null)
            {
                damage = Math.Floor(damage / 2m);
                if ((selected as IComponentsCardModel)?.GetComponent<Originalsin>() is { } sin)
                    await sin.Forgive(choiceContext);
            }
        }

        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}