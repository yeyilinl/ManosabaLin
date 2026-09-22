using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.AncientCurses;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Relics;

/// <summary>
/// 魔女因子（亚里沙第二个初始遗物）：
/// 局内当你的所有牌里没有【魔女化】诅咒卡时，将一张【魔女化】诅咒卡加入抽牌堆。
/// </summary>
[RegisterRelic(typeof(YalisalinRelicPool))]
[RegisterCharacterStarterRelic(typeof(Yalisalin), Order = 10)]
public sealed class YalisalinWitchFactor : ManosabaRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature?.CombatState is not { } combatState)
            return;

        // 收集玩家在当前战斗中的所有牌（牌组/手牌/抽牌/弃牌/消耗）
        var allCards = PileType.Deck.GetPile(Owner).Cards
            .Concat(PileType.Hand.GetPile(Owner).Cards)
            .Concat(PileType.Draw.GetPile(Owner).Cards)
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .Concat(PileType.Exhaust.GetPile(Owner).Cards)
            .ToList();

        if (allCards.Any(c => c is WitchificationCurse))
            return;

        var curse = combatState.CreateCard<WitchificationCurse>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            curse,
            PileType.Draw,
            Owner,
            CardPilePosition.Random);
        Flash();
    }
}
