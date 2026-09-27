using Godot;
using HarmonyLib;
using ManosabaLin.Characters.Yalisalin.Components;
using MegaCrit.Sts2.Core.Nodes.Cards;
using System.Reflection;

namespace ManosabaLin.Patches;

/// <summary>
///     余火选卡界面里「本次打不出」的牌，在能量处显示 ×（临时无法打出），
///     与手牌里条件不满足的卡观感一致。
///     <para>
///         引擎只在 <c>pileType == PileType.Hand</c> 且 <c>!CanPlay</c> 时才点亮
///         <c>%UnplayableEnergyIcon</c>；而选卡界面用 <c>PileType.None</c> 展示卡牌，
///         所以这里自己补一次 —— 判据只看
///         <see cref="YalisalinFireComponentUnplayableRegistry" /> 的登记结果。
///     </para>
/// </summary>
[HarmonyPatch(typeof(NCard))]
internal static class YalisalinFireComponentUnplayablePatch
{
    private static readonly FieldInfo? EnergyIconField =
        AccessTools.Field(typeof(NCard), "_unplayableEnergyIcon");

    private static readonly FieldInfo? StarIconField =
        AccessTools.Field(typeof(NCard), "_unplayableStarIcon");

    [HarmonyPatch(nameof(NCard.UpdateEnergyCostVisuals))]
    [HarmonyPostfix]
    private static void EnergyCostPostfix(NCard __instance)
    {
        ForceUnplayableIcon(__instance, EnergyIconField);
    }

    [HarmonyPatch(nameof(NCard.UpdateStarCostVisuals))]
    [HarmonyPostfix]
    private static void StarCostPostfix(NCard __instance)
    {
        // 只有真的要花星费的卡才画星费的 ×，免得给纯能量费的卡添一个假星标。
        if (__instance.Model is { } model && model.GetStarCostWithModifiers() <= 0)
            return;

        ForceUnplayableIcon(__instance, StarIconField);
    }

    private static void ForceUnplayableIcon(NCard card, FieldInfo? field)
    {
        if (field is null) return;
        if (!GodotObject.IsInstanceValid(card)) return;
        if (card.Model is not { } model) return;
        if (!YalisalinFireComponentUnplayableRegistry.Contains(model)) return;
        if (field.GetValue(card) is not TextureRect icon) return;
        if (!GodotObject.IsInstanceValid(icon)) return;

        icon.Visible = true;
    }
}
