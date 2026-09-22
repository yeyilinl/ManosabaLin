using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 罪债抵押（2 费技能・罕见）：
/// 获得 8 点格挡。然后选择：①宽恕 1 张手牌原罪然后消耗；②自惩 1 张手牌原罪然后消耗。
/// 升级：格挡 8 → 12。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class ZuiZhaiDiYa() : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block, cardPlay);

        var handSins = PileType.Hand.GetPile(owner).Cards
            .Where(c => c.HasComponent<Originalsin>())
            .ToList();
        if (handSins.Count == 0) return;

        // ① 宽恕 1 张手牌原罪后消耗（若取消则进入②）
        var forgiveSel = (await CardSelectCmd.FromHand(
            choiceContext,
            owner,
            new CardSelectorPrefs(new LocString("cards", $"{Id.Entry}.forgivePrompt"), 0, 1),
            c => c.HasComponent<Originalsin>(),
            this)).FirstOrDefault();

        if (forgiveSel != null)
        {
            if ((forgiveSel as IComponentsCardModel)?.GetComponent<Originalsin>() is { } f)
                await f.Forgive(choiceContext);
            await CardCmd.Exhaust(choiceContext, forgiveSel);
            return;
        }

        // ② 自惩 1 张手牌原罪后消耗
        var punishSel = (await CardSelectCmd.FromHand(
            choiceContext,
            owner,
            new CardSelectorPrefs(new LocString("cards", $"{Id.Entry}.punishPrompt"), 0, 1),
            c => c.HasComponent<Originalsin>(),
            this)).FirstOrDefault();

        if (punishSel != null)
        {
            if ((punishSel as IComponentsCardModel)?.GetComponent<Originalsin>() is { } p)
                await p.Punish(choiceContext);
            await CardCmd.Exhaust(choiceContext, punishSel);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}