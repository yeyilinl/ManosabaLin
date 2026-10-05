using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 第十三格旁听：每当触发火色「连续」，先给予同一名敌人层数格火色，
/// 再消耗该敌人层数格火色并抽层数张牌（卡面「给予1格火色然后消耗1格火色并抽1张卡牌」）。
/// 额外消耗照常进入连续链：成对判定后链计数归零，这次额外消耗最多把计数顶回 1 ⇒ 顶多再推进一步即停。
/// </summary>
[RegisterPower]
public sealed class ThirteenthListenerPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnContinuousTriggered(
        PlayerChoiceContext choiceContext,
        Creature target,
        CardModel? source = null)
    {
        if (Owner.Player is not { } player
            || !YalisalinFireColorSystem.TryGetHairpin(player, out var hairpin))
            return;

        var amount = (int)Amount;
        Flash();
        await hairpin.GiveFireColor(choiceContext, target, amount, source);
        await hairpin.ConsumeFireColor(choiceContext, target, amount, source);
        await CardPileCmd.Draw(choiceContext, amount, player);
    }
}
