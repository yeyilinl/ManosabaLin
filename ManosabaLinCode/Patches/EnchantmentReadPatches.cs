using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Hooks;

namespace ManosabaLin.Patches;

// 审判具现（HextechTrialEmbodiment）把卡上的附魔**全部**搬进卡里的组件之后，
// 引擎**数值加成**所在的那些路径只会结算单值的 `card.Enchantment`：
//
//   · Hook.ModifyDamage  (Hook.cs:1503)  num += cardSource.Enchantment.EnchantDamageAdditive(...)
//   · Hook.ModifyBlock   (Hook.cs:1329)  num += cardSource.Enchantment.EnchantBlockAdditive(...)
//   · DynamicVars 的 UpdateCardPreview（卡面数字预览）：
//       BlockVar / CalculatedBlockVar / DamageVar / CalculatedDamageVar / ExtraDamageVar / OstyDamageVar
//     （BlockVar.cs:31、CalculatedBlockVar.cs:31、DamageVar.cs:31、CalculatedDamageVar.cs:44、
//      ExtraDamageVar.cs:35、OstyDamageVar.cs:35）
//
// ⭐ 第 1 条具现附魔**不需要**本文件操心：读取桥（HextechTrialEmbodimentBridge 打在
//   `CardModel.get_Enchantment` 上）已经把它交给引擎，引擎自己就会算进去。
//   本文件只负责**第 2 条起**（`EffectiveEnchantments.BeyondPrimary`）——
//   这正是「一张牌能同时挂多个附魔」这个功能真正落地的地方。
//
// 打法分两种：
//   · Hook.ModifyDamage / ModifyBlock 用 **Prefix** 把加成注入 `damage`/`block` 形参 —— 引擎自己的
//     `num = damage; num += Enchantment.EnchantXXXAdditive(num, props)` 就是在这个值上算的，
//     所以在它之前注入等价于「附魔还挂在卡上」，**实际战斗数值完全精确**。
//   · DynamicVar 的 `UpdateCardPreview` 返回 void、改的是属性，只能 **Postfix** 追加到
//     PreviewValue / EnchantedValue 上。加法/乘法口径与引擎一致（引擎也是
//     `v += additive(v); v *= multiplicative(v)`），只是顺序上排在了 hooks 之后 ⇒ 属显示值，可接受。
//
// ⚠️ 唯一的快路径守卫：卡内没有「第 2 条起的附魔」⇒ 直接返回（绝大多数卡走这条**零分配**快路径，
//   因为 Hook.ModifyDamage 是每次伤害计算都跑的最热路径）。
internal static class EmbodiedEnchantmentMath
{
    /// <summary>
    ///     取「引擎**不会**自己结算的那部分附魔」（第 2 条起），没有则 null。
    ///     <para>
    ///         引擎的数值路径只读单值的 <c>card.Enchantment</c>，而读取桥已把第 1 条具现附魔交给它
    ///         ⇒ 这里只补第 2 条起，否则第 1 条会被算两遍。
    ///     </para>
    /// </summary>
    private static IReadOnlyList<EnchantmentModel>? Embodied(CardModel? card)
    {
        if (card is null) return null;

        var list = EffectiveEnchantments.BeyondPrimary(card);
        return list.Count == 0 ? null : list;
    }

    /// <summary>
    ///     本遗物是否应当<b>全量接管</b>这张卡的附魔数值加成（= 完全不走引擎那句
    ///     <c>if (cardSource.Enchantment != null) num += cardSource.Enchantment.EnchantDamageAdditive(...)</c>）。
    ///     <para>
    ///         ⚠️⭐ 用户 2026-10-02 裁决：「像这种有具体数值加成的你直接让审判具现这个遗物接管」——
    ///         即数值不再依赖「读取桥恰好能交出附魔」这件事（桥要读组件、组件要能构建实例，
    ///         构建又受卡的不可变窗口影响）⇒ 由遗物直接读组件、自己算，链条最短。
    ///     </para>
    ///     <para>
    ///         条件：真槽位为空（附魔全部被具现进组件）<b>且</b>卡里至少有一条具现附魔。
    ///         真槽位里还留着附魔的老卡 ⇒ <b>不</b>接管：那条是引擎的正常附魔，让引擎自己算，
    ///         想同时看到两条时由 <see cref="BeyondPrimary" /> 在预览路径上补。
    ///     </para>
    /// </summary>
    internal static bool ShouldTakeOver(CardModel? card)
    {
        if (card is null) return false;
        if (EffectiveEnchantments.Raw(card) is not null) return false;

        return EffectiveEnchantments.FindComponent(card)?.GetEmbodiedEnchantments() is { Count: > 0 };
    }

    /// <summary>
    ///     接管：注入<b>全部</b>有效附魔的伤害加成。
    ///     <para>⚠️ 调用方必须<b>已经关桥</b>，否则引擎自己那段会把第 1 条再算一遍。</para>
    /// </summary>
    internal static void ApplyDamageAll(CardModel? card, ref decimal value, ValueProp props,
        bool multiplicativeOnly = false)
    {
        if (card is null) return;

        foreach (var e in EffectiveEnchantments.Of(card))
        {
            if (!multiplicativeOnly) value += e.EnchantDamageAdditive(value, props);
            value *= e.EnchantDamageMultiplicative(value, props);
        }
    }

    /// <summary>接管：注入<b>全部</b>有效附魔的格挡加成（调用方须已关桥）。</summary>
    internal static void ApplyBlockAll(CardModel? card, ref decimal value)
    {
        if (card is null) return;

        foreach (var e in EffectiveEnchantments.Of(card))
        {
            value += e.EnchantBlockAdditive(value);
            value *= e.EnchantBlockMultiplicative(value);
        }
    }

    /// <param name="multiplicativeOnly">
    ///     <see cref="ExtraDamageVar" /> 用：引擎对它**只**跑 `EnchantDamageMultiplicative`
    ///     （「额外伤害」不该再吃 Sharp 那类加算）⇒ 这里保持一致。
    /// </param>
    internal static void ApplyDamage(CardModel? card, ref decimal value, ValueProp props,
        bool multiplicativeOnly = false)
    {
        if (Embodied(card) is not { } embodied) return;

        foreach (var e in embodied)
        {
            if (!multiplicativeOnly) value += e.EnchantDamageAdditive(value, props);
            value *= e.EnchantDamageMultiplicative(value, props);
        }
    }

    internal static void ApplyBlock(CardModel? card, ref decimal value)
    {
        if (Embodied(card) is not { } embodied) return;

        foreach (var e in embodied)
        {
            value += e.EnchantBlockAdditive(value);
            value *= e.EnchantBlockMultiplicative(value);
        }
    }

    /// <summary>卡面格挡数字预览：把具现附魔的加成补到 PreviewValue / EnchantedValue 上。</summary>
    internal static void ApplyBlockPreview(CardModel card, DynamicVar var)
    {
        var before = var.PreviewValue;
        var value = before;
        ApplyBlock(card, ref value);
        if (value == before) return;

        var.PreviewValue = value;
        if (!card.IsEnchantmentPreview) var.EnchantedValue = value;
    }

    /// <summary>卡面伤害数字预览（同 <see cref="ApplyBlockPreview" />；<paramref name="props" /> 要传对，
    /// 因为像 Sharp 这样的附魔会判 `props.IsPoweredAttack()`）。</summary>
    internal static void ApplyDamagePreview(CardModel card, DynamicVar var, ValueProp props,
        bool multiplicativeOnly = false)
    {
        var before = var.PreviewValue;
        var value = before;
        ApplyDamage(card, ref value, props, multiplicativeOnly);
        if (value == before) return;

        var.PreviewValue = value;
        if (!card.IsEnchantmentPreview) var.EnchantedValue = value;
    }
}

// ── 实际战斗数值：由遗物**全量接管**（不再依赖读取桥恰好交得出附魔）───────────────────────────
//
// ⚠️⭐ 之前这里是「只补第 2 条起，第 1 条交给引擎经读取桥结算」。实测（2026-10-02）第 1 条**没生效**：
//   读取桥要读组件、组件要 `FromSerializable` + `ApplyInternal` 建实例，而 `ApplyInternal` 里有一句
//   `card.AssertMutable()` —— 只要这次读取撞上卡的**不可变窗口**（战斗结算 / 渲染 / 打出中），
//   整批构建就静默失败、桥交出 null，于是「引擎自己那条」和「我们补的那条」**一起消失**。
//
// ⇒ 现在改成：**真槽位为空 + 卡内有具现附魔**时，遗物临时**关掉读取桥**（引擎那段 `cardSource.Enchantment`
//   读到 null）并**自己注入全部**附魔的加成。好处：
//     · 数值不再依赖「桥这次有没有交出来」；
//     · 多条附魔的叠加口径由遗物一处说了算（不会与引擎各算一半）。
//   真槽位里还有附魔的老卡照旧不碰（那是引擎的正常附魔）。
//
// ⚠️ 关桥只在**这一段**内生效（Prefix 进 / Postfix 出，`__state` 存原深度，可嵌套），
//    函数其余部分与其它调用方看到的 `card.Enchantment` 都是正常的。

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
internal static class EmbodiedEnchantmentModifyDamagePatch
{
    // ⚠️ `__state` 用 -1 当「本次没接管」的哨兵（桥深度恒 ≥ 0）。
    [HarmonyPrefix]
    private static void Prefix(CardModel? cardSource, ValueProp props, ref decimal damage, out int __state)
    {
        __state = -1;
        if (!EmbodiedEnchantmentMath.ShouldTakeOver(cardSource)) return;

        EmbodiedEnchantmentBridge.Enter(out __state);
        EmbodiedEnchantmentMath.ApplyDamageAll(cardSource, ref damage, props);
    }

    [HarmonyPostfix]
    private static void Postfix(int __state)
    {
        if (__state >= 0) EmbodiedEnchantmentBridge.Exit(__state);
    }

    // ⚠️⭐ 目标方法抛异常时 Postfix 不会跑 ⇒ 只有 Finalizer 能兜住抑制深度
    //    （深度一旦失衡，读取桥就**永久**失效）。见 EmbodiedEnchantmentBridge.RestoreDepth。
    [HarmonyFinalizer]
    private static System.Exception? Finalizer(int __state, System.Exception? __exception)
        => EmbodiedEnchantmentBridge.RestoreDepth(__state, __exception);
}

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyBlock))]
internal static class EmbodiedEnchantmentModifyBlockPatch
{
    [HarmonyPrefix]
    private static void Prefix(CardModel? cardSource, ref decimal block, out int __state)
    {
        __state = -1;
        if (!EmbodiedEnchantmentMath.ShouldTakeOver(cardSource)) return;

        EmbodiedEnchantmentBridge.Enter(out __state);
        EmbodiedEnchantmentMath.ApplyBlockAll(cardSource, ref block);
    }

    [HarmonyPostfix]
    private static void Postfix(int __state)
    {
        if (__state >= 0) EmbodiedEnchantmentBridge.Exit(__state);
    }

    // ⚠️⭐ 同 ModifyDamage：用 Finalizer 兜住抑制深度，避免桥永久失效。
    [HarmonyFinalizer]
    private static System.Exception? Finalizer(int __state, System.Exception? __exception)
        => EmbodiedEnchantmentBridge.RestoreDepth(__state, __exception);
}

// ── 卡面数字预览（6 个 DynamicVar）──────────────────────────────────────────────────────

// ⚠️⭐ 这五个都带 `runGlobalHooks` 参数：为 true 时引擎会调 `Hook.ModifyDamage/ModifyBlock`（并**覆盖**
//   自己刚算的 num），而那两个入口已被本文件全量接管 ⇒ 此时不能再补一遍（否则重复）。
//   但「不该接管」（真槽位还有附魔的旧卡）时要照旧补第 2 条起 —— 所以判据是
//   `runGlobalHooks && ShouldTakeOver(card)`。
//   ⚠️ `ExtraDamageVar` 是个例外：它**根本**不调 Hook（只有 multiplicative），所以保持原样。

[HarmonyPatch(typeof(BlockVar), nameof(BlockVar.UpdateCardPreview))]
internal static class EmbodiedEnchantmentBlockVarPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel card, BlockVar __instance, bool runGlobalHooks)
    {
        if (runGlobalHooks && EmbodiedEnchantmentMath.ShouldTakeOver(card)) return;
        EmbodiedEnchantmentMath.ApplyBlockPreview(card, __instance);
    }
}

[HarmonyPatch(typeof(CalculatedBlockVar), nameof(CalculatedBlockVar.UpdateCardPreview))]
internal static class EmbodiedEnchantmentCalculatedBlockVarPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel card, CalculatedBlockVar __instance, bool runGlobalHooks)
    {
        if (runGlobalHooks && EmbodiedEnchantmentMath.ShouldTakeOver(card)) return;
        EmbodiedEnchantmentMath.ApplyBlockPreview(card, __instance);
    }
}

[HarmonyPatch(typeof(DamageVar), nameof(DamageVar.UpdateCardPreview))]
internal static class EmbodiedEnchantmentDamageVarPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel card, DamageVar __instance, bool runGlobalHooks)
    {
        if (runGlobalHooks && EmbodiedEnchantmentMath.ShouldTakeOver(card)) return;
        EmbodiedEnchantmentMath.ApplyDamagePreview(card, __instance, __instance.Props);
    }
}

[HarmonyPatch(typeof(CalculatedDamageVar), nameof(CalculatedDamageVar.UpdateCardPreview))]
internal static class EmbodiedEnchantmentCalculatedDamageVarPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel card, CalculatedDamageVar __instance, bool runGlobalHooks)
    {
        if (runGlobalHooks && EmbodiedEnchantmentMath.ShouldTakeOver(card)) return;
        EmbodiedEnchantmentMath.ApplyDamagePreview(card, __instance, __instance.Props);
    }
}

[HarmonyPatch(typeof(ExtraDamageVar), nameof(ExtraDamageVar.UpdateCardPreview))]
internal static class EmbodiedEnchantmentExtraDamageVarPatch
{
    // ⚠️ ExtraDamageVar 没有 Props 属性 —— 引擎对它硬编码 ValueProp.Move，且**只**跑 multiplicative；
    //    它自己也**不调** Hook（所以永远不会被本文件的接管覆盖）⇒ 保持无条件补算。
    [HarmonyPostfix]
    private static void Postfix(CardModel card, ExtraDamageVar __instance)
        => EmbodiedEnchantmentMath.ApplyDamagePreview(card, __instance, ValueProp.Move, multiplicativeOnly: true);
}

[HarmonyPatch(typeof(OstyDamageVar), nameof(OstyDamageVar.UpdateCardPreview))]
internal static class EmbodiedEnchantmentOstyDamageVarPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel card, OstyDamageVar __instance, bool runGlobalHooks)
    {
        if (runGlobalHooks && EmbodiedEnchantmentMath.ShouldTakeOver(card)) return;
        EmbodiedEnchantmentMath.ApplyDamagePreview(card, __instance, __instance.Props);
    }
}

// ── 卡面文字：列出具现附魔的「名字」──────────────────────────────────────────────────────
//
// 附魔具现进卡里之后，卡面上就完全看不出这张牌带附魔了 —— 引擎那句
//   `LocString locString = Enchantment?.DynamicExtraCardText; if (locString != null) list2.Add(...)`
// （CardModel.cs:1405）本来负责这件事，但它的内容（附魔的「额外卡面文字」）属于
// **引擎的正常附魔显示**，而用户 2026-10-02 裁决要的是「不使用正常附魔显示框、牌面显示附魔名字」
//   ⇒ 桥接补丁把具现实例的 `DynamicExtraCardText` 掐成 null（见 HextechTrialEmbodimentBridge），
//     这里再把它们的**名字**（`enchantments` 表的 `<Id>.title`）拼到卡面描述末尾。
//
// 为什么只显示名字、不显示效果：用户 2026-10-02 裁决「转换过的附魔在卡牌上面显示附魔名字就行了」
//   —— 效果交给悬浮框（组件的 HoverTips + 引擎自己那条）。
// 为什么追加到末尾而不是描述正下方：中间那段的行序由引擎拼（keywords 会 `Insert(0, ...)`、
//   replay 行会 Add），Postfix 只能改最终字符串，稳妥起见不再挪位置。
// ⚠️ 打的是**私有**实现重载 `(PileType, DescriptionPreviewType, Creature)`：卡面渲染走的是
//   `NCard.cs:888 → GetDescriptionForPile(pileType, target)`，而升级预览走另一个 public 重载，
//   两者最终都汇到这个私有实现 ⇒ 打这里一处即可全覆盖。
// ⚠️ 它第 2 个参数的类型 `DescriptionPreviewType` 是 `CardModel` 的**私有嵌套枚举**，没法在
//   `[HarmonyPatch]` 里写出来 ⇒ 只能用 `HarmonyTargetMethod` 反射按「名字 + 参数个数」定位。
[HarmonyPatch]
internal static class EmbodiedEnchantmentCardTextPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
        => AccessTools.GetDeclaredMethods(typeof(CardModel))
            .Single(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref string __result)
    {
        var embodied = EffectiveEnchantments.FindComponent(__instance)?.GetEmbodiedEnchantments();
        if (embodied is null or { Count: 0 }) return;

        var names = new StringBuilder();
        foreach (var enchantment in embodied)
        {
            string? name = null;
            try
            {
                // ⚠️ 附魔模型可能已不存在（对方模组被卸载）⇒ 逐个 try，别让卡面渲染崩掉。
                name = enchantment.Title.GetFormattedText();
            }
            catch (System.Exception)
            {
                // 忽略
            }

            if (string.IsNullOrWhiteSpace(name)) continue;
            names.Append('【').Append(name).Append('】');
        }

        if (names.Length == 0) return;

        // ⚠️ 颜色：用户 2026-10-02 裁决改为**粉红**（`#ff99cc`，与本模组其它粉红强调色一致）。
        __result += "\n[color=#ff99cc]" + names + "[/color]";
    }
}
