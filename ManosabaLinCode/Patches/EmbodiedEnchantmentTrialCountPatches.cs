using System;
using HarmonyLib;
using ManosabaLin.Characters.Common.LinRelics;
using ManosabaLin.Characters.Ema.Relics;
using ManosabaLin.Characters.Emalin.Enchantments;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Patches;

// ── 审判计数同步：审判徽章 +1 ⇒ 审判具现遗物也 +1 ──────────────────────────────────────────
//
// ⭐ 2026-10-02 用户裁决：把「赞同 / 反驳 / 疑问」的计数放进审判具现遗物
//    （`HextechTrialEmbodiment.AgreementCount / RebuttalCount / DoubtCount`），
//    并允许悬浮该遗物直接看到当前计数 —— 这样别的卡 / 能力也能从遗物读到这三个数。
//
// ⚠️ 一致性做法：**不去另起一套计数源**，而是挂在艾玛审判徽章 `EmaTrialBadge.IncrementCount`
//    的**后置**上 —— 徽章每 +1，本遗物就 +1。计数事件完全同源 ⇒ 永不脱节。
//    （徽章的计数由「审判附魔的 OnPlay」与「审判组件的 OnPlayPostfix」共同驱动，两条路径都会
//     经过 `IncrementCount`，所以这里挂在它上面就覆盖了全部来源。）
//
// ⚠️ 清零不在这里做：由 `HextechTrialEmbodiment.AfterPlayerTurnStart`（回合号变了就清）与
//    `AfterCombatEnd`（战斗结束清）负责，规则与徽章一致。
[HarmonyPatch(typeof(EmaTrialBadge), nameof(EmaTrialBadge.IncrementCount))]
internal static class EmbodiedEnchantmentTrialCountPatch
{
    /// <summary>赞同 / 反驳 / 疑问 的编号（与 <c>HextechTrialEmbodiment.AddTrialCount(int)</c> 对应）。</summary>
    private const int KindAgreement = 0;
    private const int KindRebuttal = 1;
    private const int KindDoubt = 2;

    [HarmonyPostfix]
    private static void Postfix(EmaTrialBadge __instance, EnchantmentModel enchantment)
    {
        try
        {
            var kind = KindOf(enchantment);
            if (kind < 0) return;

            HextechTrialEmbodiment.Find(__instance.Owner)?.AddTrialCount(kind);
        }
        catch (Exception)
        {
            // 计数只是显示与读取，同步失败不值得把游戏搞崩。
        }
    }

    private static int KindOf(EnchantmentModel? enchantment) => enchantment switch
    {
        Agreement => KindAgreement,
        Rebuttal => KindRebuttal,
        Doubt => KindDoubt,
        _ => -1
    };
}
