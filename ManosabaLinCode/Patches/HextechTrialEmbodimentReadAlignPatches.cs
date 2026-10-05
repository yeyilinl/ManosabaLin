using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Emalin.Enchantments;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Patches;

// 审判具现（HextechTrialEmbodiment）：把「本回合打出的附魔牌」这类统计，
// 对齐到「卡内具现的**全部**附魔」。
//
// ── 为什么需要这个文件 ────────────────────────────────────────────────────────────────
// `EmalinCombatHelper` 的 6 个查询全部靠 `CardPlayStartedEntry.CardPlay.Card.Enchantment`
// 认附魔（`is Agreement / is Rebuttal / is Doubt`）。而 `CardModel.Enchantment` 是**单值属性**，
// 读取桥只能交出卡内的**第 1 条**具现附魔 ⇒ 一张牌同时挂着【赞同】+【反驳】+【疑问】时，
// 这三个查询只认得其中一条。
//
// ⚠️⭐ 用户 2026-10-02 裁决：**只改遗物自己的代码，艾玛源码一个字都不碰**
//   ⇒ 这里用 Harmony **后置补丁**把差值补上（`Characters/Ema/**` 下没有任何文件被修改）。
//   `EmalinCombatHelper` 的方法全部是**纯查询**（无副作用），所以「改返回值」是最安全的介入方式：
//   不新增/篡改战斗历史，也不改变任何状态。
//
// ── 口径 ──────────────────────────────────────────────────────────────────────────────
// 原方法数的是「打出的**卡**里、有附魔的那些」（每张卡计 1，看的是第 1 条）。
// 本文件补的是「同一张卡上**第 2 条起**的具现附魔」——
//   例：一张牌同时挂着【赞同】(第1条) +【反驳】(第2条)，打出一次：
//     · 总附魔牌数：原 1（卡有附魔）+ 补 1（第 2 条仍是审判附魔）= 2 ✅
//     · 赞同牌数  ：原 1（桥交出赞同）+ 补 0（第 2 条不是赞同）= 1 ✅
//     · 反驳牌数  ：原 0（桥交出的是赞同）+ 补 1（第 2 条是反驳）= 1 ✅
//   ⇒「同一张牌可以同时算作赞同牌与反驳牌」，正是「多附魔共存」应有的语义。
//
// ⚠️ 只走 `BeyondPrimary`（第 2 条起）：第 1 条已经被原方法经读取桥数过一遍，再补就重复了。
// ⚠️ 没装遗物 / 卡内没有具现附魔时 `BeyondPrimary` 返回空 ⇒ 补 0 ⇒ **零行为变化**。
internal static class EmbodiedEnchantmentPlayStats
{
    /// <summary>审判的三类附魔。</summary>
    private static bool IsTrial(EnchantmentModel? enchantment)
        => enchantment is Agreement or Rebuttal or Doubt;

    /// <summary>
    ///     本回合、由该持有者打出的每张卡上「第 2 条起」的具现附魔条数（按 <paramref name="match" /> 过滤）。
    /// </summary>
    internal static int ExtraCount(Creature? owner, ICombatState? combatState, Func<EnchantmentModel, bool> match)
    {
        if (owner is null || combatState is null) return 0;

        var entries = CombatManager.Instance?.History?.Entries;
        if (entries is null) return 0;

        var count = 0;
        foreach (var entry in entries.OfType<CardPlayStartedEntry>())
        {
            if (!entry.HappenedThisTurn(combatState)) continue;

            var card = entry.CardPlay?.Card;
            if (card is null || card.Owner?.Creature != owner) continue;

            foreach (var enchantment in EffectiveEnchantments.BeyondPrimary(card))
            {
                if (match(enchantment)) count++;
            }
        }

        return count;
    }

    /// <summary>
    ///     本回合、由该持有者打出的卡上实际出现的**审判附魔类型**集合（含第 1 条与第 2 条起）。
    ///     <para>
    ///         「打过几种附魔」「是否赞同+反驳都打过」这类判定无法靠加法补 —— 必须知道完整的类型集合，
    ///         所以这两处直接把结果**整体替换**掉（原方法的第 1 条口径由
    ///         <see cref="EffectiveEnchantments.Of" /> 保持一致 ⇒ 数值不会变）。
    ///     </para>
    /// </summary>
    internal static HashSet<Type> TrialTypes(Creature? owner, ICombatState? combatState)
    {
        var types = new HashSet<Type>();
        if (owner is null || combatState is null) return types;

        var entries = CombatManager.Instance?.History?.Entries;
        if (entries is null) return types;

        foreach (var entry in entries.OfType<CardPlayStartedEntry>())
        {
            if (!entry.HappenedThisTurn(combatState)) continue;

            var card = entry.CardPlay?.Card;
            if (card is null || card.Owner?.Creature != owner) continue;

            foreach (var enchantment in EffectiveEnchantments.Of(card))
            {
                if (IsTrial(enchantment)) types.Add(enchantment.GetType());
            }
        }

        return types;
    }
}

// ── 6 个查询的后置补丁 ────────────────────────────────────────────────────────────────
//
// ⚠️ 全部打 `Postfix`：原方法照常跑（保持引擎口径与第 1 条语义），只在结果上补差值 / 替换集合。

[HarmonyPatch(typeof(EmalinCombatHelper), nameof(EmalinCombatHelper.GetTotalEnchantmentPlaysThisTurn))]
internal static class EmbodiedEnchantmentTotalPlaysPatch
{
    [HarmonyPostfix]
    private static void Postfix(Creature owner, ICombatState combatState, ref int __result)
        => __result += EmbodiedEnchantmentPlayStats.ExtraCount(owner, combatState, e => e is Rebuttal or Agreement or Doubt);
}

[HarmonyPatch(typeof(EmalinCombatHelper), nameof(EmalinCombatHelper.GetAgreementPlaysThisTurn))]
internal static class EmbodiedEnchantmentAgreementPlaysPatch
{
    [HarmonyPostfix]
    private static void Postfix(Creature owner, ICombatState combatState, ref int __result)
        => __result += EmbodiedEnchantmentPlayStats.ExtraCount(owner, combatState, e => e is Agreement);
}

[HarmonyPatch(typeof(EmalinCombatHelper), nameof(EmalinCombatHelper.GetRebuttalPlaysThisTurn))]
internal static class EmbodiedEnchantmentRebuttalPlaysPatch
{
    [HarmonyPostfix]
    private static void Postfix(Creature owner, ICombatState combatState, ref int __result)
        => __result += EmbodiedEnchantmentPlayStats.ExtraCount(owner, combatState, e => e is Rebuttal);
}

[HarmonyPatch(typeof(EmalinCombatHelper), nameof(EmalinCombatHelper.GetDoubtPlaysThisTurn))]
internal static class EmbodiedEnchantmentDoubtPlaysPatch
{
    [HarmonyPostfix]
    private static void Postfix(Creature owner, ICombatState combatState, ref int __result)
        => __result += EmbodiedEnchantmentPlayStats.ExtraCount(owner, combatState, e => e is Doubt);
}

[HarmonyPatch(typeof(EmalinCombatHelper), nameof(EmalinCombatHelper.GetDistinctEnchantmentTypesThisTurn))]
internal static class EmbodiedEnchantmentDistinctTypesPatch
{
    // ⚠️ 集合类判定整体替换：原方法只看得见第 1 条的类型集合，加法补不出「新类型」。
    [HarmonyPostfix]
    private static void Postfix(Creature owner, ICombatState combatState, ref int __result)
        => __result = EmbodiedEnchantmentPlayStats.TrialTypes(owner, combatState).Count;
}

[HarmonyPatch(typeof(EmalinCombatHelper), nameof(EmalinCombatHelper.HasPlayedBothAgreementAndRebuttal))]
internal static class EmbodiedEnchantmentBothPlayedPatch
{
    // ⚠️ 同上：只关心「赞同 / 反驳」两类，其余类型不计入。
    [HarmonyPostfix]
    private static void Postfix(Creature owner, ICombatState combatState, ref bool __result)
    {
        var distinct = 0;
        foreach (var type in EmbodiedEnchantmentPlayStats.TrialTypes(owner, combatState))
        {
            if (type == typeof(Agreement) || type == typeof(Rebuttal)) distinct++;
        }

        __result = distinct >= 2;
    }
}
