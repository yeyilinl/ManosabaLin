using System;
using System.Collections.Generic;
using HarmonyLib;
using ManosabaLin.Characters.Ema.Relics;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace ManosabaLin.Patches;

/// <summary>
///     审判具现（<c>HextechTrialEmbodiment</c>）的「读取桥」。
///     <para>
///         遗物把卡上的附魔<b>全部</b>搬进卡里的 <c>EnchantmentEmbodimentComponent</c> ⇒ 附魔槽变成空槽。
///         但引擎、以及**任何**角色 / 卡牌 / 能力 / 第三方模组的代码，判断「这张牌有没有附魔」靠的都是
///         读 <c>card.Enchantment</c> 这一个属性 —— 空槽会让它们全部失准。
///     </para>
///     <para>
///         ⭐ 本桥的做法：<b>不逐个去改读取方，而是直接把「属性本身」补成正确值</b> ——
///         给 <c>CardModel.get_Enchantment</c> 打后置补丁，卡上真槽位为空但卡里有具现附魔时，
///         返回**第 1 条**具现附魔。这样「附魔一旦具现，全游戏都当它还在卡上」，
///         而遗物自己的代码之外<b>一处都不用动</b>（这正是用户 2026-10-02 的裁决）。
///     </para>
///     <para>
///         为什么只返回第 1 条：属性是<b>单值</b>的，引擎的数值 / 打出 / 打出次数 / 悬浮路径也都只读它一次。
///         第 2 条起由遗物自己的补丁补算（<c>EnchantmentReadPatches</c> 走
///         <see cref="EffectiveEnchantments.BeyondPrimary" />），两边合起来正好等于「全部附魔都生效」。
///     </para>
///     <para>
///         ⚠️⭐ 有五处操作<b>绝对不能</b>看到这条桥接出来的附魔，必须读<b>真槽位</b>
///         （统一用 <see cref="SuppressScope" /> 临时关掉本桥）：
///     </para>
///     <list type="number">
///         <item><c>CardModel.ToSerializable</c> —— 否则会把具现附魔**写进存档的真槽位**（读档后重复）。</item>
///         <item><c>CardModel.DeepCloneFields</c> —— 否则克隆出来的卡会**同时**有真附魔和具现附魔。</item>
///         <item><c>CardModel.DowngradeInternal</c> —— 它调 <c>Enchantment?.ModifyCard()</c>，
///               而具现实例没有绑定卡牌，会抛 <c>InvalidOperationException</c>。</item>
///         <item><c>EnchantmentModel.CanEnchant</c> —— 这是引擎「一卡一附魔」的执行点
///               （见 <c>HextechTrialEmbodimentCanEnchantPatch</c>），必须看到「槽位为空」才肯放行第 2 条附魔。</item>
///         <item><c>EmaTrialBadge.EnchantAllTrialCards</c>（艾玛审判徽章的开战自动附魔）——
///               它靠「<c>card.Enchantment != null</c> 就跳过」判「这张牌已经有附魔了」。
///               审判具现把附魔**全部搬进组件**后，卡上真槽位是**空**的（附魔已经「不算附魔」了），
///               只是桥把第 1 条补给了 <c>card.Enchantment</c> 才让这句误判 ⇒ 执行期间必须关桥。</item>
///     </list>
/// </summary>
internal static class EmbodiedEnchantmentBridge
{
    [ThreadStatic] private static int _depth;

    internal static int Depth
    {
        get => _depth;
        set => _depth = value;
    }

    /// <summary>当前是否处于「只许看到真槽位」的窗口内。</summary>
    internal static bool Suppressed => _depth > 0;

    /// <summary>
    ///     在 <c>Prefix</c> 里 <c>Depth = __state + 1</c>、在 <c>Postfix</c> 里 <c>Depth = __state</c>
    ///     ⇒ 嵌套调用也能精确还原（比简单的 bool 开关安全）。
    /// </summary>
    internal static void Enter(out int state)
    {
        state = _depth;
        _depth = state + 1;
    }

    internal static void Exit(int state) => _depth = state;

    /// <summary>
    ///     <b>兜底还原</b>：给每个「关桥」补丁挂 <c>[HarmonyFinalizer]</c> 用。
    ///     <para>
    ///         ⚠️⭐ <b>为什么必须有</b>：<c>[ThreadStatic]</c> 的抑制深度只靠 Prefix/Postfix 成对还原，
    ///         而 <b>Postfix 在目标方法抛异常时不会执行</b>（Harmony 不把 postfix 包在 finally 里）
    ///         ⇒ 只要有一个被关桥的方法抛过一次，深度就<b>永久</b>留在 &gt; 0，读取桥从此彻底失效 ——
    ///         症状正是「卡面能看到附魔名（卡面补丁直读组件），但引擎与悬浮全都看不见附魔」
    ///         （2026-10-02 实测反馈的「能读 title 读不到 description」）。
    ///         Finalizer 一定在 <c>finally</c> 里跑，用它还原深度后桥再也不会卡死。
    ///     </para>
    ///     <para>
    ///         ⚠️ <paramref name="state" /> 为 <c>-1</c> 表示「本次调用压根没进抑制窗口」
    ///         （Hook.ModifyDamage/ModifyBlock 的 <c>__state</c> 哨兵值）⇒ 不动深度。
    ///     </para>
    /// </summary>
    internal static Exception? RestoreDepth(int state, Exception? exception)
    {
        if (state >= 0) _depth = state;
        return exception;
    }

    /// <summary>取「引擎应当看到的第 1 条有效附魔」（卡内具现的；没有则 null）。</summary>
    internal static EnchantmentModel? Primary(CardModel card)
    {
        try
        {
            return EffectiveEnchantments.FindComponent(card)?.GetBridgeEnchantment();
        }
        catch (Exception ex)
        {
            // ⚠️ 曾经这里是**完全静默**的 —— 桥失效时没有任何痕迹，只能靠猜。
            //    现在按「消息去重」记一条，既不影响热路径（每次伤害计算都会走这里），
            //    又能在日志里直接看到桥到底为什么交不出附魔。
            LogOnce($"{ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>桥接失败的诊断：同一句只记一次（本方法是每次伤害计算的必经之路）。</summary>
    private static void LogOnce(string message)
    {
        lock (LoggedOnce)
        {
            if (LoggedOnce.Count > 20) return;
            if (!LoggedOnce.Add(message)) return;
        }

        try
        {
            MainFile.Logger.Info($"[EmbodimentBridge] {message}");
        }
        catch (Exception)
        {
            // 日志失败无所谓。
        }
    }

    private static readonly HashSet<string> LoggedOnce = [];
}

/// <summary>卡牌附魔槽的取值器：真槽位为空而卡里有具现附魔时，如实给出第 1 条。</summary>
[HarmonyPatch(typeof(CardModel), "get_Enchantment")]
internal static class EmbodiedEnchantmentGetterPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref EnchantmentModel? __result)
    {
        if (__result is not null) return;                    // 真槽位有东西（或已被别的补丁设过）⇒ 原样
        if (EmbodiedEnchantmentBridge.Suppressed) return;     // 处于「只许看真槽位」的窗口 ⇒ 原样

        __result = EmbodiedEnchantmentBridge.Primary(__instance);
    }
}

// ── 四处「必须只看真槽位」的引擎操作 ────────────────────────────────────────────────────
//
// 统一写法：Prefix 关桥（把当前深度存进 __state 并把深度 +1），Postfix 还原 __state。

[HarmonyPatch(typeof(CardModel), nameof(CardModel.ToSerializable))]
internal static class EmbodiedEnchantmentSerializePatch
{
    [HarmonyPrefix]
    private static void Prefix(out int __state) => EmbodiedEnchantmentBridge.Enter(out __state);

    [HarmonyPostfix]
    private static void Postfix(int __state) => EmbodiedEnchantmentBridge.Exit(__state);

    // ⚠️⭐ 见 RestoreDepth 的说明：目标方法抛异常时 Postfix 不会跑，只有 Finalizer 能兜住抑制深度。
    [HarmonyFinalizer]
    private static Exception? Finalizer(int __state, Exception? __exception)
        => EmbodiedEnchantmentBridge.RestoreDepth(__state, __exception);
}

[HarmonyPatch(typeof(CardModel), "DeepCloneFields")]
internal static class EmbodiedEnchantmentDeepClonePatch
{
    [HarmonyPrefix]
    private static void Prefix(out int __state) => EmbodiedEnchantmentBridge.Enter(out __state);

    [HarmonyPostfix]
    private static void Postfix(int __state) => EmbodiedEnchantmentBridge.Exit(__state);

    // ⚠️⭐ 见 RestoreDepth 的说明：目标方法抛异常时 Postfix 不会跑，只有 Finalizer 能兜住抑制深度。
    [HarmonyFinalizer]
    private static Exception? Finalizer(int __state, Exception? __exception)
        => EmbodiedEnchantmentBridge.RestoreDepth(__state, __exception);
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.DowngradeInternal))]
internal static class EmbodiedEnchantmentDowngradePatch
{
    [HarmonyPrefix]
    private static void Prefix(out int __state) => EmbodiedEnchantmentBridge.Enter(out __state);

    [HarmonyPostfix]
    private static void Postfix(int __state) => EmbodiedEnchantmentBridge.Exit(__state);

    // ⚠️⭐ 见 RestoreDepth 的说明：目标方法抛异常时 Postfix 不会跑，只有 Finalizer 能兜住抑制深度。
    [HarmonyFinalizer]
    private static Exception? Finalizer(int __state, Exception? __exception)
        => EmbodiedEnchantmentBridge.RestoreDepth(__state, __exception);
}

// ── 第五处：艾玛审判徽章的「开战自动附魔」不该跳过已具现的卡 ──────────────────────────
//
// EmaTrialBadge.EnchantAllTrialCards() 的跳过判定是 `if (card.Enchantment != null) continue;`
// （以及内层 `CanReceiveInitialTrialComponent` 的 `card.Enchantment == null`）—— 原意是
// 「这张牌已经有附魔了，别再叠」。但审判具现把附魔**全部搬进组件**之后，卡上真槽位是**空**的
// （附魔已经「不算附魔」了），只是读取桥把第 1 条具现附魔补给了 `card.Enchantment`，
// 才让这两句误判成「已有附魔」而跳过 ⇒ 已具现过的卡永远吃不到徽章开战补的那条审判附魔。
//
// ⚠️ 用户 2026-10-02 裁决（原话）：「艾玛遗物是在有附魔情况下跳过，审判具现转换的不算附魔啊，
//    都是组件了本身就不应该跳过」⇒ 执行本方法期间**临时关桥**，让 `card.Enchantment`
//    如实反映**真槽位**：具现过的卡 = null ⇒ 不跳过；真槽位上还有附魔的老卡 ⇒ 照旧跳过。
// ⚠️ 只动读取桥开关，**不改艾玛源码**（与既有 ReadAlignPatches 同一手法）。
[HarmonyPatch(typeof(EmaTrialBadge), "EnchantAllTrialCards")]
internal static class EmbodiedEnchantmentTrialBadgePatch
{
    [HarmonyPrefix]
    private static void Prefix(out int __state) => EmbodiedEnchantmentBridge.Enter(out __state);

    [HarmonyPostfix]
    private static void Postfix(int __state) => EmbodiedEnchantmentBridge.Exit(__state);

    // ⚠️⭐ 见 RestoreDepth 的说明：目标方法抛异常时 Postfix 不会跑，只有 Finalizer 能兜住抑制深度。
    [HarmonyFinalizer]
    private static Exception? Finalizer(int __state, Exception? __exception)
        => EmbodiedEnchantmentBridge.RestoreDepth(__state, __exception);
}

// ── 卡面表现：不使用引擎的「附魔显示框」 ──────────────────────────────────────────────
//
// 桥接之后 card.Enchantment 非空 ⇒ 引擎会把附魔标签页（图标 + 数量）显示出来。
// 用户 2026-10-02 裁决「不使用正常附魔显示框」⇒ 这里对「具现出来的那一条」把它藏掉
//（真槽位上的附魔照常显示，不受影响）。

[HarmonyPatch(typeof(NCard), "UpdateEnchantmentVisuals")]
internal static class EmbodiedEnchantmentTabPatch
{
    [HarmonyPostfix]
    private static void Postfix(NCard __instance)
    {
        try
        {
            if (__instance.Model?.Enchantment is not { } enchantment) return;
            if (!EffectiveEnchantments.IsEmbodiedInstance(enchantment)) return;

            __instance.EnchantmentTab.Visible = false;
        }
        catch (Exception)
        {
            // 卡面表现失败不值得让渲染崩掉。
        }
    }
}

// 卡面上的引擎附魔额外文字（<c>enchantments.&lt;Id&gt;.extraCardText</c>）同样属于「正常附魔显示」，
// 用户要的是「牌面显示附魔名字」⇒ 对具现实例把它整条掐掉，名字由
// EnchantmentReadPatches 的 GetDescriptionForPile 补丁补上。
[HarmonyPatch(typeof(EnchantmentModel), "get_DynamicExtraCardText")]
internal static class EmbodiedEnchantmentExtraTextPatch
{
    [HarmonyPostfix]
    private static void Postfix(EnchantmentModel __instance, ref LocString? __result)
    {
        if (__result is null) return;
        if (EffectiveEnchantments.IsEmbodiedInstance(__instance)) __result = null;
    }
}

// ── 悬浮提示：曾经在这里把「具现实例的 HoverTips」整个清空 —— 2026-10-02 **已删除** ──────────
//
// 当时的想法是「引擎 `CardModel.HoverTips` 会 `AddRange(Enchantment.HoverTips)`，而组件自己也列了一遍
// ⇒ 清掉引擎那条防重复」。但 `EnchantmentModel.HoverTips`（= 本体 `HoverTip` + `ExtraHoverTips`）是
// **唯一**能拿到附魔效果悬浮的公共出口 —— 组件读的是同一个属性，于是这条补丁把**两边一起掐死**了：
//   · 引擎那条没了（预期内）；
//   · 组件 `BuildHoverTips` 也读到空 ⇒ **一条效果悬浮都看不到**（用户实测反馈）。
//
// ⭐ 正确分工（现在）：两边**各管一段**，谁都别去清对方的数据源 ——
//   · 第 1 条 → 引擎 `CardModel.HoverTips` 经读取桥给出（桥交出的就是第 1 条）；
//   · 第 2 条起 → 组件 `EnchantmentEmbodimentComponent.HoverTips`（走 `EffectiveEnchantments.BeyondPrimary`）。

// ── 「清除附魔」也要清掉卡内具现的那几条 ──────────────────────────────────────────────
// 引擎的 CardCmd.ClearEnchantment(card) 只会清**真槽位**（CardModel.ClearEnchantmentInternal
// 唯一调用方就是它，`CardCmd.cs:569-572`）⇒ 附魔具现之后这句命令会变成空操作。
// 这里在它之后把卡内的具现附魔一并清掉，并刷新一次卡面
//（否则「牌面显示附魔名字」会一直留在屏幕上，直到下一次自然刷新）。
[HarmonyPatch(typeof(CardModel), nameof(CardModel.ClearEnchantmentInternal))]
internal static class EmbodiedEnchantmentClearPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance)
    {
        try
        {
            var component = EffectiveEnchantments.FindComponent(__instance);
            if (component is null || component.GetEmbodiedEnchantments().Count == 0) return;

            component.ClearAll();

            if (NCard.FindOnTable(__instance) is { } node)
                node.UpdateVisuals(node.DisplayingPile, CardPreviewMode.None);
        }
        catch (Exception)
        {
            // 清理失败不值得让游戏崩掉；最坏情况只是那几条附魔还留着。
        }
    }
}

// ── 写回 Amount：桥接实例是**临时实例**，直接改它读完就丢 ────────────────────────────────
//
// 引擎与卡牌会直接写 `card.Enchantment.Amount`，例如：
//   · 艾玛审判徽章的计数回写（EmaTrialBadge.SyncCountersToEnchantments 那句
//     `card.Enchantment.Amount = _agreeCount`）；
//   · 原版 Goopy 的 `base.Card.DeckVersion.Enchantment.Amount++`。
// 桥接出来后 `card.Enchantment` 是临时实例 ⇒ 不写回的话这些修改会**静默丢失**，
// 于是「看起来和真挂在卡上一模一样」就成了假象。
[HarmonyPatch(typeof(EnchantmentModel), "set_Amount")]
internal static class EmbodiedEnchantmentAmountPatch
{
    [HarmonyPostfix]
    private static void Postfix(EnchantmentModel __instance)
    {
        if (!EffectiveEnchantments.IsEmbodiedInstance(__instance)) return;

        try
        {
            var card = __instance.Card;
            if (card is null) return;

            EffectiveEnchantments.FindComponent(card)?.WriteBackAmount(__instance, __instance.Amount);
        }
        catch (Exception)
        {
            // 写回失败只影响层数同步，不值得让游戏崩掉。
        }
    }
}
