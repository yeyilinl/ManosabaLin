using System.Collections.Generic;
using HarmonyLib;
using ManosabaLin.Characters.Common.AncientCurses;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace ManosabaLin.Patches;

/// <summary>
/// 图鉴归属补丁：让特殊先古诅咒卡（<see cref="IManosabaAncientCurseCard"/>）在诅咒图鉴
/// （杂项池 MiscPool）可见，同时从先古图鉴（AncientsPool）中隐藏。
/// 原版先古行谓词 = Rarity==Ancient（只看稀有度）；杂项行谓词 = 稀有度 6~10（不含 Ancient）。
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary._Ready))]
internal static class LinAncientCurseCardLibraryPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(
        NCardLibrary __instance,
        Dictionary<NCardPoolFilter, Func<CardModel, bool>> ____poolFilters)
    {
        var ancients = __instance.GetNodeOrNull<NCardPoolFilter>("%AncientsPool");
        var misc = __instance.GetNodeOrNull<NCardPoolFilter>("%MiscPool");

        if (ancients is not null && ____poolFilters.TryGetValue(ancients, out var oldAncient))
        {
            ____poolFilters[ancients] = c => oldAncient(c) && c is not IManosabaAncientCurseCard;
        }

        if (misc is not null && ____poolFilters.TryGetValue(misc, out var oldMisc))
        {
            ____poolFilters[misc] = c => oldMisc(c) || c is IManosabaAncientCurseCard;
        }
    }
}
