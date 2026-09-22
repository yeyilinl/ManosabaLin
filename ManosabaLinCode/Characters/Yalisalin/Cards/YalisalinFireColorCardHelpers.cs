using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

internal static class YalisalinFireColorCardHelpers
{
    public static async Task Attack(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        CardModel source,
        Creature target,
        decimal damage)
    {
        await DamageCmd.Attack(damage)
            .FromCard(source, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public static async Task ApplyHeat(PlayerChoiceContext choiceContext, Player owner, Creature target, CardModel source, bool strong = false)
    {
        YalisalinFireColor promoteColor;
        var promoted = strong
            ? YalisalinFireColorSystem.TryStrongConvertFireColor(owner, target, out promoteColor, source)
            : YalisalinFireColorSystem.TryConvertFireColor(owner, target, out promoteColor, source);

        if (!promoted)
            return;

        // 第十三格旁听（独立能力）：升温成功后，对升温前颜色触发一次被消耗奖励
        if (owner.Creature.GetPower<ThirteenthListenerPower>() is { } listener)
            await listener.HandleHeatPromote(choiceContext, promoteColor, source);
    }

    public static void SetHeatWord(this CardModel card, bool strong)
    {
        if (card.TryGetCapability<YalisalinHeatWordCapability>(out var heat))
            heat.SetStrongHeat(strong);
    }
}
