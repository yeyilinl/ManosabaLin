using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 指名还押（1 费技能・普通）：
/// 将弃牌堆 1 张原罪诅咒加入抽牌堆底部，然后抽 1 张。
/// 若弃牌堆没有原罪诅咒，则可以选择加入 1 张带原罪的原罪诅咒。
/// 升级：额外获得 1 点临时能量。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class ZhiMingHuanYa() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;

        var discard = PileType.Discard.GetPile(owner);
        var discardSins = discard.Cards.Where(c => c.HasComponent<Originalsin>()).ToList();

        if (discardSins.Count > 0)
        {
            var selected = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                discard,
                owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
                c => c.HasComponent<Originalsin>())).FirstOrDefault();

            if (selected != null)
                await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Bottom, this);

            await CardPileCmd.Draw(choiceContext, 1, owner);
            return;
        }

        // 弃牌堆没有原罪诅咒：可选择是否加入 1 张带原罪的原罪诅咒（"是/否"选项卡 UI）
        var rng = owner.RunState.Rng.CombatCardGeneration;
        var sin = AncientSinCardCatalog.CreateRandom(CombatState, owner, rng);
        sin.TryAddComponent(new Originalsin());

        var take = await YesNoChoiceScreen.Pick(
            choiceContext,
            owner,
            new LocString("cards", $"{Id.Entry}.yesNoPrompt"),
            YesNoChoiceScreen.Yes,
            YesNoChoiceScreen.No);

        if (take)
            await CardPileCmd.Add(sin, PileType.Draw, CardPilePosition.Bottom, this);
        // 选"否"：sin 从未入堆（临时卡），无需任何清理

        await CardPileCmd.Draw(choiceContext, 1, owner);

        if (IsUpgraded)
            await PlayerCmd.GainEnergy(1m, owner);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
    }
}