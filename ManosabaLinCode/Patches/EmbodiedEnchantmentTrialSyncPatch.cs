using System;
using HarmonyLib;
using ManosabaLin.Characters.Ema.Relics;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ManosabaLin.Patches;

// 审判具现（HextechTrialEmbodiment）：艾玛审判徽章的「赞同 / 反驳 / 疑问」**计数**回写补全。
//
// ── 问题 ──────────────────────────────────────────────────────────────────────────────
// `EmaTrialBadge.SyncCountersToEnchantments()`（EmaTrialBadge.cs:119）长这样：
//
//     switch (card.Enchantment)                          // ← card.Enchantment 是**单值**属性
//     {
//         case Agreement: card.Enchantment.Amount = _agreeCount;    break;
//         case Doubt:     card.Enchantment.Amount = _doubtCount;    break;
//         case Rebuttal:  card.Enchantment.Amount = _rebuttalCount; break;
//     }
//
// 读取桥让 `card.Enchantment` 交出卡内**第 1 条**具现附魔 ⇒ 一张牌同时挂着【赞同】+【反驳】时，
// 这个 switch 只会命中其中一条，**另一条的 Amount 永远是 0** ——
// 而三类审判附魔都是 `ShowAmount => true`，于是卡面 / 悬浮里那个「已打出 N 张」的计数显示不出来
//（其余按 `count % N` 触发的效果走 `badge.AgreeCount` 等属性，不受影响）。
//
// ── 修法 ──────────────────────────────────────────────────────────────────────────────
// ⚠️ 用户 2026-10-02 裁决「只改遗物自己的代码，艾玛源码一个字都不碰」
//   ⇒ 在遗物侧打 **Postfix**：原方法照跑（保住第 1 条的既有行为），
//     之后再把每张卡里**所有具现的**审判附魔统一同步成徽章当前计数
//     （组件侧 `SyncTrialAmounts` 会同时写回序列化载荷与已建实例，避免实例重建）。
//
// ⚠️ 幂等：同步的是「当前计数值」而不是增量 ⇒ 重复调用无副作用。
// ⚠️ 没装遗物 / 卡内没有具现附魔时 `SyncTrialAmounts` 找不到条目 ⇒ 零行为变化。
// ⚠️ `SyncCountersToEnchantments` 是**私有**方法 ⇒ 只能按名字定位（Harmony 支持）。
[HarmonyPatch(typeof(EmaTrialBadge), "SyncCountersToEnchantments")]
internal static class EmbodiedEnchantmentTrialSyncPatch
{
    private static readonly PileType[] TrialPiles =
        [PileType.Hand, PileType.Draw, PileType.Discard, PileType.Deck];

    [HarmonyPostfix]
    private static void Postfix(EmaTrialBadge __instance)
    {
        try
        {
            var owner = __instance.Owner;
            if (owner is null) return;

            foreach (var pileType in TrialPiles)
            {
                foreach (var card in pileType.GetPile(owner).Cards)
                {
                    EffectiveEnchantments.FindComponent(card)?.SyncTrialAmounts(
                        __instance.AgreeCount,
                        __instance.RebuttalCount,
                        __instance.DoubtCount);
                }
            }
        }
        catch (Exception)
        {
            // 计数只是显示，同步失败不值得把游戏搞崩。
        }
    }
}
