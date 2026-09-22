using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 第十三格旁听（独立能力）：每次火色升温成功后，对「升温前那个颜色」触发一次该颜色的被消耗奖励。
/// 触发由升温入口（升温卡 / ApplyHeat）在升温成功后调用 <see cref="HandleHeatPromote"/>。
/// </summary>
[RegisterPower]
public sealed class ThirteenthListenerPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 升温成功后调用：对升温前颜色触发一次「被消耗奖励」。
    /// </summary>
    public async Task HandleHeatPromote(
        PlayerChoiceContext choiceContext,
        YalisalinFireColor promoteColor,
        CardModel? source = null)
    {
        if (Owner is null || Owner.Player is not { } player)
            return;

        if (!YalisalinFireColorSystem.TryGetHairpin(player, out var hairpin))
            return;

        await hairpin.ResolveExtraFireColorReward(choiceContext, promoteColor, source);
    }
}