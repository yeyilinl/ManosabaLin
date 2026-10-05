using System.Collections.Generic;
using HarmonyLib;
using ManosabaLin.Characters.Common.LinRelics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ManosabaLin.Patches;

// 艾玛联动遗物 3（羁绊漂移）之一：观测「亲近」的变化。
//
// ⚠️ 为什么不借 Yalisabond.ApplyBondDeltaAsync：BondPower 的两个 setter 只在 **delta > 0** 时才调它
// （减少的那一半根本不通知）⇒ 收不到「亲近减少」。所以只能在 setter 这一层观测，
// 由遗物自己拿「上次值 vs 新值」算 delta（正负都在）。
[HarmonyPatch(typeof(BondPower), "set_Affinity")]
internal static class HextechBondDriftAffinityPatch
{
    [HarmonyPostfix]
    private static void Postfix(BondPower __instance)
        => HextechBondDrift.OnBondValueChanged(__instance, isAffinity: true);
}

// 艾玛联动遗物 3（羁绊漂移）之二：观测「疏远」的变化。
[HarmonyPatch(typeof(BondPower), "set_Estrangement")]
internal static class HextechBondDriftEstrangementPatch
{
    [HarmonyPostfix]
    private static void Postfix(BondPower __instance)
        => HextechBondDrift.OnBondValueChanged(__instance, isAffinity: false);
}

// 艾玛联动遗物 3（羁绊漂移）之三：站位重排之后把累计偏移叠回去。
//
// 位移落在 NCreature.Position（父级逻辑位置）上 —— 这是唯一一个「引擎自己不会改、又能长期停留」的槽位
// （Visuals.Position 会被受击抖动 AnimShake 重置为 0）。
// 代价是 NCombatRoom.PositionPlayersAndPets 每次重排都会把 Position 覆盖成基础站位，
// 所以在它跑完之后，对每个持有遗物的玩家把「疏远 − 亲近」的偏移重新加一次（绝对值，幂等）。
[HarmonyPatch(typeof(NCombatRoom), "PositionPlayersAndPets",
    [typeof(List<NCreature>), typeof(float), typeof(bool)])]
internal static class HextechBondDriftRepositionPatch
{
    [HarmonyPostfix]
    private static void Postfix(List<NCreature> creatureNodes)
    {
        if (creatureNodes is null) return;

        foreach (var node in creatureNodes)
        {
            if (node?.Entity.Player is not { } player) continue;
            HextechBondDrift.ReapplyOffset(player);
        }
    }
}
