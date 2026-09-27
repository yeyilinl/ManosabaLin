using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Emalin.Enchantments;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using STS2RitsuLib.Keywords;

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>便签条 - 1费技能, 疑问关键字, 抽4张, 手牌里的疑问附魔牌可以免费打出一次</summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class Data : ManosabaCardTemplate
{
    public Data() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { EmalinKeywordRules.DoubtCardKeyword };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        // 卡面是「手牌里疑问附魔牌可以免费打出一次」，不附任何前提 ⇒ 不再用 CanPlay() 过滤
        // （留着那个判断会让玩家「当下付不起」的疑问牌拿不到免费，与文案矛盾）。
        // SetToFreeThisTurn() 就是引擎的「本回合或直到打出为止」免费原语。
        foreach (var card in PileType.Hand.GetPile(Owner).Cards)
        {
            if (card.Enchantment is Doubt)
                card.SetToFreeThisTurn();
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        
    }
}
