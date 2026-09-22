using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common.AncientCurses.Powers;
using ManosabaLin.Characters.Common.Components;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

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
        new IntVar("PerForgive", 2),
        new DamageAfterVar()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var owner = Owner;
        var creature = owner.Creature;

        var forgiveCount = creature.GetPower<OriginalsinForgivenessCounterPower>()?.Amount ?? 0m;
        var damage = DynamicVars["DamageBase"].BaseValue + forgiveCount * DynamicVars["PerForgive"].BaseValue;

        // 卡面显示：本牌结算后可以打出的伤害（8 + 宽恕数×2）
        DynamicVars["DamageAfter"].BaseValue = damage;

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
        // 若手牌没有原罪：可选择先获得一张带[原罪]的原罪诅咒入手，再宽恕它（本牌伤害减半）
        else
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            var sin = AncientSinCardCatalog.CreateRandom(CombatState, owner, rng);
            sin.TryAddComponent(new Originalsin());

            // "是/否"选卡界面：选中"是"才获得并宽恕
            var want = await YesNoChoiceScreen.Pick(
                choiceContext,
                owner,
                new LocString("cards", $"{Id.Entry}.yesNoPrompt"),
                YesNoChoiceScreen.Yes,
                YesNoChoiceScreen.No);

            if (want)
            {
                await CardPileCmd.AddGeneratedCardToCombat(sin, PileType.Hand, owner);
                damage = Math.Floor(damage / 2m);
                if ((sin as IComponentsCardModel)?.GetComponent<Originalsin>() is { } generated)
                    await generated.Forgive(choiceContext);
            }
            // 选"否"：sin 从未入堆（临时卡），无需任何清理
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

    /// <summary>
    /// 卡面动态伤害数值（DamageAfter）：实时显示本牌结算后可打出的伤害（8 + 本场宽恕次数×2）。
    /// 注：实际攻击伤害在 OnPlay 里按是否减半另行计算，此值仅供卡面展示。
    /// </summary>
    private sealed class DamageAfterVar : DynamicVar
    {
        public DamageAfterVar() : base("DamageAfter", 8m) { }

        public override void UpdateCardPreview(
            CardModel card,
            CardPreviewMode previewMode,
            Creature? target,
            bool runGlobalHooks)
        {
            var forgiveCount = card.Owner?.Creature
                .GetPower<OriginalsinForgivenessCounterPower>()?.Amount ?? 0m;
            PreviewValue = 8m + forgiveCount * 2m;
            EnchantedValue = PreviewValue;
        }
    }
}