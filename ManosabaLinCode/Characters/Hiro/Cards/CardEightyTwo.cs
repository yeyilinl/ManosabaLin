using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Hiro.Cards;

[RegisterCard(typeof(HirolinCardPool))]
public sealed class CardEightyTwo() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new BlockVar(8m, ValueProp.Move),
        new DynamicVar("Cards", 1m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;
        var owner = source.Owner;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        // 卡面：获得 8 点格挡
        await CreatureCmd.GainBlock(owner.Creature, source.DynamicVars.Block, cardPlay);

        // 卡面：选择 N 张在【弃牌堆】的卡牌返回【抽牌堆】
        var returnCount = source.DynamicVars["Cards"].IntValue;
        var discardPile = PileType.Discard.GetPile(owner);
        if (discardPile.Cards.Count == 0) return;

        var prefs = new CardSelectorPrefs(source.SelectionScreenPrompt, returnCount, returnCount);
        var selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            discardPile.Cards,
            owner,
            prefs);

        foreach (var card in selectedCards)
            await CardPileCmd.Add(card, PileType.Draw);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：选择 1 张 → 2 张。
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
