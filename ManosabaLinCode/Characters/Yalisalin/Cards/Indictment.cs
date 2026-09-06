using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 点名罪证（1 费技能・罕见）：
/// 从 3 张随机原罪诅咒中选 1，加入抽牌堆并添加原罪组件。
/// 升级：额外获得 10 点格挡。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Indictment() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Block", 0)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var rng = owner.RunState.Rng.CombatCardGeneration;

        var options = AncientSinCardCatalog.CreateRandomDistinct(CombatState, owner, rng, 3);
        var chosen = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            options,
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).FirstOrDefault();

        if (chosen != null)
        {
            chosen.TryAddComponent(new Originalsin());
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Draw, owner, CardPilePosition.Random);
        }

        var block = DynamicVars["Block"].BaseValue;
        if (block > 0m)
            await CreatureCmd.GainBlock(owner.Creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["Block"].UpgradeValueBy(10);
    }
}