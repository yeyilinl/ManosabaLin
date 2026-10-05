using MinionLib.Component.Core;
﻿using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.HiroKeywords;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ManosabaLin.Characters.Hiro.Cards;

[RegisterCard(typeof(HirolinCardPool))]
public sealed class CardSixtyThree() : ManosabaCardTemplate(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get { yield return CardKeyword.Exhaust; }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("Cards", 1m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;

        await CreatureCmd.TriggerAnim(source.Owner.Creature, "Cast", source.Owner.Character.CastAnimDelay);

        // 卡面：「从所有牌里面选择 N 张卡牌获得【轮回】」
        // —— 「所有牌」= 手牌 + 抽牌堆 + 弃牌堆（用户 2026-10-02 明确）。
        var candidates = new List<CardModel>();
        foreach (var pileType in AllPiles)
            candidates.AddRange(pileType.GetPile(source.Owner).Cards.Where(c => c != source));

        if (candidates.Count == 0) return;

        // 选择，数量使用动态变量
        var selectCount = source.DynamicVars["Cards"].IntValue;
        var prefs = new CardSelectorPrefs(source.SelectionScreenPrompt, selectCount, selectCount)
        {
            PretendCardsCanBePlayed = true
        };

        var selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            source.Owner,
            prefs
        );

        // 给选中的卡牌添加轮回关键词
        foreach (var card in selectedCards) card.AddModKeyword(TransmigrationRules.TransmigrationCardKeyword);
    }

    private static readonly PileType[] AllPiles = [PileType.Hand, PileType.Draw, PileType.Discard];

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["Cards"].UpgradeValueBy(1m); // 升级后可选 2 张
    }
}