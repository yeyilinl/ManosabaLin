using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Ema.Cards;

[RegisterCard(typeof(EmalinCardPool))]
public sealed class TheOnlyClue : ManosabaCardTemplate
{
    public TheOnlyClue() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<BondPower>();
            yield return HoverTipFactory.FromPower<NyxmPower>();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Cards", 2m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var creature = owner.Creature;

        var bond = creature.GetPower<BondPower>();
        if (bond != null) bond.Affinity++;

        await PowerCmd.Apply<NyxmPower>(
            choiceContext, creature, 2, creature, this, false);

        await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].BaseValue, owner);

        // 未升级：抽牌后弃 1 张。升级后不再弃牌（只抽 3 张）。
        if (!IsUpgraded)
        {
            var handCards = PileType.Hand.GetPile(owner).Cards;
            if (handCards.Count > 0)
            {
                var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1, 1);
                var selected = await CardSelectCmd.FromHand(
                    choiceContext, owner, prefs, null, this);
                var discarded = selected.FirstOrDefault();
                if (discarded != null)
                    await CardPileCmd.Add(discarded, PileType.Discard);
            }
        }

        // 卡面：「若【亲近】>【疏远】，获得等于【弃牌堆】卡数等量的格挡」
        // —— 未升级时在弃牌结算之后读取（含刚刚弃掉的那张）；升级不弃牌，读到的就是弃牌堆原样。
        if (bond != null && bond.Affinity > bond.Estrangement)
        {
            var discardCount = PileType.Discard.GetPile(owner).Cards.Count;
            if (discardCount > 0)
                await CreatureCmd.GainBlock(creature, discardCount, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：抽 2 张 → 抽 3 张，且不再弃 1 张（不再减费）。
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
