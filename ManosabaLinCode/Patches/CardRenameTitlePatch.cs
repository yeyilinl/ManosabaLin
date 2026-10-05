using System;
using HarmonyLib;
using ManosabaLin.Characters.Common.HiroKeywords;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Patches;

// 「卡名覆盖」（CardRename）的读取侧：引擎的 CardModel.Title 是从本地化键现算的，没有原生改名 API，
// 唯一的读取出口就是这个属性 —— 在这里兜一层即可，改动面只有 1 个 Postfix。
//
// ⚠️ 必须自己拼升级后缀：原实现是 `名` / `名+` / `名+N`（MaxUpgradeLevel > 1 时带数字），
//    覆盖后若不补，升级卡会丢掉「+」，玩家就看不出升级了。
[HarmonyPatch(typeof(CardModel), "get_Title")]
internal static class CardRenameTitlePatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref string __result)
    {
        if (!CardRename.TryGet(__instance, out var name)) return;

        try
        {
            if (!__instance.IsUpgraded)
            {
                __result = name;
                return;
            }

            __result = __instance.MaxUpgradeLevel > 1
                ? $"{name}+{__instance.CurrentUpgradeLevel}"
                : name + "+";
        }
        catch (Exception)
        {
            // 改名只是显示层，取不到升级状态时退回原名，绝不让它把游戏搞崩。
        }
    }
}
