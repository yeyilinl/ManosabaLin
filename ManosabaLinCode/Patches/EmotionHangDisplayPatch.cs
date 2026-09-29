using System;
using HarmonyLib;
using ManosabaLin.Characters.Sherrylin.Orbs;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ManosabaLin.Patches;

/// <summary>
///     把「挂在血条下方」的持续型情绪显示区挂到每个<b>玩家生物</b>的 <c>NCreature</c> 上。
///     先例：<c>YalisalinFireColorCounterPatch</c>（同样是 <c>NCreature._Ready</c> postfix + <c>AddChild</c>）。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
internal static class EmotionHangDisplayPatch
{
    private const string NodeName = "EmotionHangDisplay";

    [HarmonyPostfix]
    public static void Postfix(NCreature __instance)
    {
        try
        {
            var creature = __instance.Entity;
            if (creature?.Player is not { } player) return;

            if (__instance.GetNodeOrNull<EmotionHangDisplay>(NodeName) != null) return;

            var display = new EmotionHangDisplay { Name = NodeName };
            __instance.AddChild(display);
            display.SetContext(player, creature);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[HextechEmotionOverflow] hang display setup failed: {ex.Message}");
        }
    }
}
