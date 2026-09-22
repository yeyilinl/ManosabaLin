using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.Components;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 魔女审判（1 费技能・基础）：
/// 选择是否添加 1 张带[原罪]的原罪诅咒至手牌，然后抽 {Cards} 张牌。
/// 升级：费用 -1。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
[RegisterCharacterStarterCard(typeof(Yalisalin))]
public sealed class YalisalinWitchTrial() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 选择是否添加 1 张带[原罪]的原罪诅咒至手牌（"是/否"选项卡 UI）
        var rng = owner.RunState.Rng.CombatCardGeneration;
        var sin = AncientSinCardCatalog.CreateRandom(CombatState, owner, rng);
        sin.TryAddComponent(new Originalsin());

        var want = await YesNoChoiceScreen.Pick(
            choiceContext,
            owner,
            new LocString("cards", $"{Id.Entry}.yesNoPrompt"),
            YesNoChoiceScreen.Yes,
            YesNoChoiceScreen.No);

        if (want)
            await CardPileCmd.AddGeneratedCardToCombat(sin, PileType.Hand, owner);
        // 选"否"：sin 从未入堆（临时卡），无需任何清理

        // 抽 Cards 张牌
        await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].BaseValue, owner);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
