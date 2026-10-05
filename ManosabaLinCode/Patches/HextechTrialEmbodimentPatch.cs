using HarmonyLib;
using ManosabaLin.Characters.Common.LinRelics;

namespace ManosabaLin.Patches;

// 海克斯联动遗物 2：审判具现 ⇒ 让卡牌可以同时持有多个不同名的附魔。
//
// 打的是非泛型的那个重载 CardCmd.Enchant(EnchantmentModel, CardModel, decimal)：
//   · 泛型重载 Enchant<T>(CardModel, decimal) 内部就是转调它 ⇒ 只打一处；
//   · 它也是「真正把附魔挂上卡」的唯一出口（CardModel.EnchantInternal 的调用者）。
//
// 前置（而不是后置）：本遗物下卡上**永远**不挂真附魔 —— 每一条（含第一条）都在这里被截下、
//   封进卡内的 EnchantmentEmbodimentComponent（真槽位保持为空 ⇒ 卡面不使用引擎的附魔显示框，
//   且同一张牌还能继续被附魔）。⚠️ 引擎原逻辑里 CanEnchant 判定失败是**抛
//   InvalidOperationException**（`CardCmd.cs:539-541`），不是静默失败 ⇒ 必须抢在它之前接管
//   （Prefix 返回 false 跳过原逻辑）。
//
// ⚠️⭐ 本遗物的行为**只在它自己的代码里实现**（2026-10-02 用户第二次裁决）：
//   「附魔拦截 + 转换附魔 + card.Enchantment 相关的全部适配」都由审判具现自己完成 ——
//   本文件 + HextechTrialEmbodimentBridge.cs + EnchantmentReadPatches.cs + 组件四块。
//   **绝不去改**其它角色 / 卡牌 / 能力里读 card.Enchantment 的判定：
//   读取侧已由 HextechTrialEmbodimentBridge 从**属性层面**统一解决（见那个文件的说明），
//   任何代码都不用动。
[HarmonyPatch(typeof(CardCmd), "Enchant", [typeof(EnchantmentModel), typeof(CardModel), typeof(decimal)])]
internal static class HextechTrialEmbodimentPatch
{
    // ⚠️⭐ `amount` 必须接住并传下去：引擎原逻辑是
    //     `card.EnchantInternal(enchantment, amount)` → `Enchantment.ApplyInternal(card, amount)`
    //     → `Amount = (int)amount`。而传进来的 `enchantment` 是**原型**的 mutable 克隆，
    //     它自己的 `Amount` 恒为 0 —— 漏掉这个参数就等于「所有具现附魔的 Amount 都是 0」：
    //       · `Sharp`（锋利）的伤害加成就是 `Amount` ⇒ 附了 +2 等于 +0，效果完全消失；
    //       · `Doubt/Agreement/Rebuttal` 的「已打出 N 张」计数、`Adroit` 的格挡值同理。
    [HarmonyPrefix]
    private static bool Prefix(EnchantmentModel enchantment, CardModel card, decimal amount,
        ref EnchantmentModel? __result)
    {
        if (enchantment is null || card is null) return true;

        // TryEmbodify 返回 true ⇒ 已处理完（具现进卡里 / 同名丢弃）⇒ 跳过原逻辑。
        return !HextechTrialEmbodiment.TryEmbodify(enchantment, card, amount, ref __result);
    }
}

// 配套补丁：让附魔选牌界面认出「卡内已具现的附魔」。
//
// 背景（2026-10-02 用户实测反馈）：事件「自助指南」（SelfHelpBook）的三个选项
// （读封底/读段落/读全书 = 给攻击/技能/能力牌附 Sharp/Nimble/Swift +2）选不到已附魔的卡，
// 但装了审判具现之后本应还能再附魔（附魔全部具现进卡里，不占附魔槽）。
//
// 根因：选牌是 CardSelectCmd.FromDeckForEnchantment 拿 `enchantment.CanEnchant(c)` 过滤的
//   （CardSelectCmd.cs:660），而 EnchantmentModel.CanEnchant 末段
//   「card.Enchantment != null && (!IsStackable || 类型不同) ⇒ false」（EnchantmentModel.cs:289）
//   是引擎「一卡一附魔」的判定点 —— 卡在过滤这一步就被筛掉，根本轮不到 CardCmd.Enchant。
//
// ⚠️ 读取桥（HextechTrialEmbodimentBridge）会把 card.Enchantment 补成「第 1 条具现附魔」，
//   于是这个判定点会**无条件拒绝**已经具现过附魔的卡 ⇒ 这里必须把桥**临时关掉**
//   （Prefix 进 / Postfix 出），让引擎看到「真槽位为空」这件事实 —— 它自己就会放行。
//   Postfix 里只剩一件事：拦住**同名**（卡内已具现过同名附魔的，仍然拒绝，不堆叠）。
//
// ⚠️ 子类 override 的 CanEnchant（如 Nimble = base.CanEnchant && card.GainsBlock）内部都调 base，
//   所以打在基类上是有效的，各附魔自己的附加条件也仍然生效。
[HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.CanEnchant))]
internal static class HextechTrialEmbodimentCanEnchantPatch
{
    [HarmonyPrefix]
    private static void Prefix(out int __state) => EmbodiedEnchantmentBridge.Enter(out __state);

    [HarmonyPostfix]
    private static void Postfix(EnchantmentModel __instance, CardModel card, ref bool __result, int __state)
    {
        EmbodiedEnchantmentBridge.Exit(__state);

        if (card is null) return;
        if (!HextechTrialEmbodiment.IsActiveFor(card.Owner)) return;   // 没遗物 ⇒ 原逻辑

        // 同名（卡内的某条具现附魔）⇒ 同名附魔只能一个、不能堆叠 ⇒ 拒绝。
        if (EffectiveEnchantments.Has(card, __instance))
        {
            __result = false;
            return;
        }

        // 真槽位里还留着附魔（获取本遗物**之前**就已经附过魔的牌 / 旧存档）⇒ 也放行：
        // 新附魔会被具现进卡里，不会去挤那一格。
        if (!__result && EffectiveEnchantments.Raw(card) is not null) __result = true;
    }
}
