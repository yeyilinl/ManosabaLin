using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using ManosabaLin.Characters.Emalin.Components;
using ManosabaLin.Characters.Emalin.Enchantments;

namespace ManosabaLin.Extensions;

/// <summary>
///     「有效附魔」统一读取器 —— 审判具现体系的唯一读取入口。
///     <para>
///         审判具现（<c>HextechTrialEmbodiment</c>）把卡上的附魔<b>全部</b>搬进
///         <see cref="EnchantmentEmbodimentComponent" />：<c>card.Enchantment</c> 的<b>真槽位</b>恒为
///         <c>null</c>（这正是「不占用附魔槽 / 不使用引擎附魔显示框」的前提）。
///     </para>
///     <para>
///         ⚠️ <b>读到「真槽位」还是读到「有效附魔」是两件事</b>：
///     </para>
///     <list type="bullet">
///         <item>
///             <b>语义判断</b>（「这张牌到底有没有附魔 / 有没有某条附魔」）用 <see cref="Of" /> /
///             <see cref="Has" /> / <see cref="Has{T}" /> / <see cref="Count" /> / <see cref="OfTrial" />。
///             <c>card.Enchantment</c> 属性本身也已经由桥接补丁
///             （<c>Patches/HextechTrialEmbodimentBridge.cs</c>）呈现有效值 ⇒ 外部代码直接读它也正确。
///         </item>
///         <item>
///             <b>「引擎已经处理过哪一条」</b>用 <see cref="BeyondPrimary" /> —— 引擎只会结算第 1 条
///             （它只读单值的 <c>card.Enchantment</c>），所以遗物自己的补丁只补第 2 条起，避免重复计入。
///         </item>
///         <item>
///             <b>「卡上真槽位到底有没有东西」</b>用 <see cref="Raw" />（例如序列化、深拷贝、
///             引擎「一卡一附魔」判定 —— 这些地方必须看到真相）。
///         </item>
///     </list>
///     <para>
///         ⚠️ 没装遗物时本类全部退化为引擎原语义（<see cref="Raw" /> 与 <see cref="Of" /> 都等同于
///         <c>card.Enchantment</c>）⇒ 零行为变化。
///     </para>
/// </summary>
public static class EffectiveEnchantments
{
    private static readonly IReadOnlyList<EnchantmentModel> Empty = [];

    /// <summary>卡的附魔槽自动属性背后的真身字段（<c>&lt;Enchantment&gt;k__BackingField</c>）。</summary>
    private static readonly AccessTools.FieldRef<CardModel, EnchantmentModel>? RawField;

    /// <summary>「由组件具现出来的附魔实例」的弱引用登记表（用于识别 <c>card.Enchantment</c> 上的桥接值）。</summary>
    private static readonly ConditionalWeakTable<EnchantmentModel, object> EmbodiedInstances = new();

    private static readonly object Mark = new();

    static EffectiveEnchantments()
    {
        try
        {
            RawField = AccessTools.FieldRefAccess<CardModel, EnchantmentModel>("<Enchantment>k__BackingField");
        }
        catch (Exception)
        {
            RawField = null;
        }
    }

    /// <summary>
    ///     卡牌附魔槽的<b>原始</b>取值（绕过桥接补丁）。
    ///     <para>
    ///         ⚠️ 凡「卡上到底有没有真附魔」的语义都必须走这里 —— 直接读 <c>card.Enchantment</c>
    ///         会拿到桥接出来的具现附魔，从而重复计数 / 误判。
    ///     </para>
    /// </summary>
    public static EnchantmentModel? Raw(CardModel? card)
    {
        if (card is null) return null;
        return RawField is not null ? RawField(card) : card.Enchantment;
    }

    /// <summary>卡上真附魔 + 卡里具现的附魔（真附魔在前，其余按具现顺序）。</summary>
    public static IReadOnlyList<EnchantmentModel> Of(CardModel? card)
    {
        if (card is null) return Empty;

        var embodied = FindComponent(card)?.GetEmbodiedEnchantments();
        var raw = Raw(card);

        if (raw is null) return embodied ?? Empty;
        if (embodied is null or { Count: 0 }) return [raw];

        var list = new List<EnchantmentModel>(embodied.Count + 1) { raw };
        list.AddRange(embodied);
        return list;
    }

    /// <summary>
    ///     <b>引擎不会自己处理</b>的那部分有效附魔（第 2 条起）。
    ///     <para>
    ///         引擎的数值结算（<c>Hook.ModifyDamage/ModifyBlock</c>、各 <c>*Var.UpdateCardPreview</c>）、
    ///         打出结算（<c>CardModel</c> 里那句 <c>Enchantment.OnPlay</c>）、打出次数
    ///         （<c>GetEnchantedReplayCount</c>）、悬浮提示都只读**单个** <c>card.Enchantment</c> ——
    ///         该值是 <see cref="Of" /> 的第 1 条 ⇒ 遗物自己的补丁只需补第 2 条起。
    ///     </para>
    /// </summary>
    public static IReadOnlyList<EnchantmentModel> BeyondPrimary(CardModel? card)
    {
        var all = Of(card);
        if (all.Count <= 1) return Empty;

        var list = new List<EnchantmentModel>(all.Count - 1);
        for (var i = 1; i < all.Count; i++) list.Add(all[i]);
        return list;
    }

    /// <summary>这张卡「实际上」是否带有该附魔（同名即算，不区分是按哪条路径挂上的）。</summary>
    public static bool Has(CardModel? card, EnchantmentModel? enchantment)
    {
        var entry = enchantment?.Id?.Entry;
        if (entry is null) return false;

        foreach (var e in Of(card))
        {
            if (e.Id?.Entry == entry) return true;
        }

        return false;
    }

    /// <summary>这张卡「实际上」是否带有某类附魔（例如 <c>Has&lt;Doubt&gt;(card)</c>）。</summary>
    public static bool Has<T>(CardModel? card) where T : EnchantmentModel
    {
        foreach (var e in Of(card))
        {
            if (e is T) return true;
        }

        return false;
    }

    /// <summary>这张卡「实际上」有多少条附魔。</summary>
    public static int Count(CardModel? card) => Of(card).Count;

    /// <summary>这张卡实际带的「审判」附魔（赞同 / 反驳 / 疑问）。</summary>
    public static IEnumerable<EnchantmentModel> OfTrial(CardModel? card)
        => Of(card).Where(e => e is Agreement or Rebuttal or Doubt);

    /// <summary>取卡里的具现组件（没有则 null）。</summary>
    public static EnchantmentEmbodimentComponent? FindComponent(CardModel? card)
        => card is IComponentsCardModel components
            ? components.GetComponent<EnchantmentEmbodimentComponent>()
            : null;

    /// <summary>登记一个「由组件具现出来的附魔实例」（弱引用，不阻止回收）。</summary>
    internal static void MarkEmbodied(EnchantmentModel enchantment)
    {
        try
        {
            if (!EmbodiedInstances.TryGetValue(enchantment, out _))
                EmbodiedInstances.Add(enchantment, Mark);
        }
        catch (Exception)
        {
            // 登记失败只影响「是否认得这是具现实例」，不影响附魔本身。
        }
    }

    /// <summary>这个附魔实例是不是「组件具现出来、通过桥接补丁出现在 <c>card.Enchantment</c> 上」的那一个。</summary>
    internal static bool IsEmbodiedInstance(EnchantmentModel? enchantment)
        => enchantment is not null && EmbodiedInstances.TryGetValue(enchantment, out _);
}
