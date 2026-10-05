using System;
using HarmonyLib;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Patches;

// 审判具现（HextechTrialEmbodiment）「补算第 2 条起」的**非数值**部分。
//
// 数值/打出/次数/悬浮已在 EnchantmentReadPatches + 组件里补齐（见那些文件的说明）。
// 本文件只补两类「引擎只读单值 card.Enchantment 所以看不到第 2 条起」的成员：
//
//   · 纯表现：CardModel.ShouldGlowGold / ShouldGlowRed
//     （CardModel.cs:831/843 —— `ShouldGlowGoldInternal || Enchantment?.ShouldGlowGold`）
//   · 起手沉底：EnchantmentModel.ShouldStartAtBottomOfDrawPile
//     （CombatManager.cs:908 —— `pile.Cards.Where(c => c.Enchantment?.ShouldStartAtBottomOfDrawPile ?? false)`，
//      只在第 1 回合抽牌前跑一次）
//
// ⭐ 第 1 条具现附魔**不需要**本文件：读取桥（HextechTrialEmbodimentBridge 打在
//   `CardModel.get_Enchantment` 上）已把它交给引擎，引擎自己就会读到它的取值。
//   本文件只负责 `EffectiveEnchantments.BeyondPrimary`。
//
// ⚠️ 没装遗物时 BeyondPrimary 恒为空 ⇒ 两个补丁都是**零行为变化**的直通。

internal static class EmbodiedEnchantmentBodyMath
{
    /// <summary>卡里「引擎看不到的那几条」（第 2 条起）是否有人要求金 / 红发光。</summary>
    internal static bool AnyGlow(CardModel? card, bool gold)
    {
        foreach (var enchantment in EffectiveEnchantments.BeyondPrimary(card))
        {
            try
            {
                if (gold ? enchantment.ShouldGlowGold : enchantment.ShouldGlowRed) return true;
            }
            catch (Exception)
            {
                // 单条取值失败（模型缺失 / 实例被冻结）⇒ 跳过，别牵连其它条。
            }
        }

        return false;
    }

    /// <summary>卡里「引擎看不到的那几条」是否有人要求起手沉底。</summary>
    internal static bool AnyStartAtBottom(CardModel? card)
    {
        foreach (var enchantment in EffectiveEnchantments.BeyondPrimary(card))
        {
            try
            {
                if (enchantment.ShouldStartAtBottomOfDrawPile) return true;
            }
            catch (Exception)
            {
                // 同上。
            }
        }

        return false;
    }
}

// ── 纯表现：卡面发光（金 = 有收益可拿，红 = 打了要出事）──────────────────────────────
//
// 打的是 CardModel 上的取值器（**不是**附魔自己的属性）—— 这样无论附魔那边怎么覆写都不会漏，
// 判定统一由遗物自己算：`卡自身条件 || 任意具现附魔要求发光`。纯表现，改不坏任何结算。

[HarmonyPatch(typeof(CardModel), "get_ShouldGlowGold")]
internal static class EmbodiedEnchantmentGlowGoldPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__result) return;

        try
        {
            if (EmbodiedEnchantmentBodyMath.AnyGlow(__instance, gold: true)) __result = true;
        }
        catch (Exception)
        {
            // 发光失败不值得让渲染崩掉。
        }
    }
}

[HarmonyPatch(typeof(CardModel), "get_ShouldGlowRed")]
internal static class EmbodiedEnchantmentGlowRedPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__result) return;

        try
        {
            if (EmbodiedEnchantmentBodyMath.AnyGlow(__instance, gold: false)) __result = true;
        }
        catch (Exception)
        {
            // 同上。
        }
    }
}

// ── 起手沉底（原版 `Imbued` 的 ShouldStartAtBottomOfDrawPile）─────────────────────────
//
// 引擎那句读的是 `c.Enchantment?.ShouldStartAtBottomOfDrawPile`：
//   · 第 1 条具现附魔 ⇒ 读取桥已经交给它，**不用管**；
//   · 第 2 条起 ⇒ 这里补。
//
// 打的是附魔自己那个虚属性（基类实现）。⚠️ 只对「引擎会读到的那一条」补算 ——
// 用 `Of(card)[0]` 做同一性判断：
//   · `__instance` 就是第 1 条 ⇒ 查卡里其余条；
//   · `__instance` 是第 2 条起（我们自己在遍历时读的）⇒ 原样返回，**不会再递归**（否则会无限套娃）。
// 唯一的已知缺口：某个**第三方**附魔既覆写了这个属性、又恰好是第 1 条 —— 那时本补丁不会被调用。
// 原版只有 `Imbued` 覆写（且恒为 true），故实际无影响。

[HarmonyPatch(typeof(EnchantmentModel), "get_ShouldStartAtBottomOfDrawPile")]
internal static class EmbodiedEnchantmentStartAtBottomPatch
{
    [HarmonyPostfix]
    private static void Postfix(EnchantmentModel __instance, ref bool __result)
    {
        if (__result) return;

        try
        {
            if (__instance.Card is not { } card) return;

            var all = EffectiveEnchantments.Of(card);
            if (all.Count == 0 || !ReferenceEquals(all[0], __instance)) return;

            if (EmbodiedEnchantmentBodyMath.AnyStartAtBottom(card)) __result = true;
        }
        catch (Exception)
        {
            // `Card` 的取值会 AssertMutable（原型实例会抛）⇒ 静默。
        }
    }
}
