using System.Threading.Tasks;
using HarmonyLib;
using ManosabaLin.Characters.Emalin.Components;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Patches;

// ── 「打出时触发」与「打出次数」：改由**组件直读**统一负责，引擎那半边对具现实例跳过 ─────────────
//
// 背景（2026-10-02 实测根因）：
//   引擎结算一张牌时，附魔相关的两条路径都只看得见**单值**的 `card.Enchantment`：
//     · `CardModel.Play` 里那句 `if (Enchantment != null) await Enchantment.OnPlay(...)`
//       （CardModel.cs:1940）—— 附魔的「打出时效果」全靠它；
//     · `CardModel.GetEnchantedReplayCount()`（= `Enchantment?.EnchantPlayCount(BaseReplayCount)`）
//       —— 被 `CardModel.cs:2031` 用来算 `playCount`。
//   而审判具现把附魔搬进组件之后，`card.Enchantment` 的真槽位是**空**的，全靠「读取桥」补第 1 条；
//   桥一旦交不出附魔（组件构建撞上卡的不可变窗口 / 桥的抑制深度失衡），这两条**一起失效**：
//     · 附魔的 OnPlay 永远不跑 ⇒「赞同 / 反驳 / 疑问」这类**靠 OnPlay 实现**的附魔完全没用
//       （而 Sharp 那类走 `Hook.ModifyDamage` 数值接管的照常生效 —— 正是实测现象）；
//     · `GetEnchantedReplayCount` 退回 `BaseReplayCount`。
//
// ⭐ 所以现在改成「遗物全量接管」（与 `EnchantmentReadPatches` 对数值的做法完全一致）：
//   · 触发：`EnchantmentEmbodimentComponent.OnPlayPostfix` **直读组件**、对**全部**具现附魔跑一遍 `OnPlay`；
//   · 次数：`EnchantmentEmbodimentComponent.ModifyCardPlayCount` 同样补**全部**。
// ⇒ 本文件把引擎那两条路径对「具现实例 / 含具现附魔的卡」跳过，保证**恰好一次**、互不重复。

/// <summary>
///     具现附魔的 <c>OnPlay</c> 不再由引擎结算 —— 改由
///     <see cref="EnchantmentEmbodimentComponent.OnPlayPostfix" /> 直读组件统一跑（恰好一次）。
/// </summary>
[HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.OnPlay))]
internal static class EmbodiedEnchantmentOnPlaySkipPatch
{
    /// <summary>
    ///     ⚠️ 只拦「组件具现出来的实例」：真槽位上的普通附魔（及其它模组的附魔）照旧走引擎，
    ///     行为零变化。
    /// </summary>
    [HarmonyPrefix]
    private static bool Prefix(EnchantmentModel __instance, ref Task __result)
    {
        if (!EffectiveEnchantments.IsEmbodiedInstance(__instance)) return true;

        __result = Task.CompletedTask;
        return false;
    }
}

/// <summary>
///     含具现附魔的卡：把引擎算进来的那份「附魔重放次数」抹掉
///     （它只算得到读取桥交出的**第 1 条**），改由
///     <see cref="EnchantmentEmbodimentComponent.ModifyCardPlayCount" /> 补**全部**。
///     <para>
///         ⚠️ 不这么做的话，桥正常情况下第 1 条会被算**两遍**
///         （`CardModel.cs:2031` 的 `GetEnchantedReplayCount()` 一次，随后 `Hook.ModifyCardPlayCount` 又一次）。
///     </para>
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetEnchantedReplayCount))]
internal static class EmbodiedEnchantmentReplayStripPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref int __result)
    {
        var component = EffectiveEnchantments.FindComponent(__instance);
        if (component is null || component.GetEmbodiedEnchantments().Count == 0) return;

        __result = __instance.BaseReplayCount;
    }
}
