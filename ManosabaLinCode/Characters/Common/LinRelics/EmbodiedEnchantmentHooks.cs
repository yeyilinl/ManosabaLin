using System;
using System.Collections.Generic;
using ManosabaLin.Characters.Emalin.Components;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     已具现附魔的<b>战斗钩子登记处</b>（审判具现）。
///     <para>
///         引擎里 <see cref="EnchantmentModel" /> 的钩子能力写在
///         <c>ShouldReceiveCombatHooks => Card?.ShouldReceiveCombatHooks ?? false</c> ——
///         附魔之所以能收到 <c>AfterCardPlayed</c> / <c>AfterCardDrawn</c> / <c>BeforeFlush</c> 这类战斗钩子，
///         是因为它挂在卡上、被引擎当作钩子监听者枚举。
///     </para>
///     <para>
///         ⚠️ 附魔一旦被<b>具现</b>进卡里的组件（不占附魔槽），它就不在任何被枚举的集合里了 ——
///         于是原版那些**靠钩子实现**的附魔效果会静默失效。实测原版附魔里至少有：
///         <c>Goopy</c> / <c>Glam</c> / <c>Vigorous</c>（<c>AfterCardPlayed</c>）、
///         <c>Slither</c>（<c>AfterCardDrawn</c>）、<c>SlumberingEssence</c>（<c>BeforeFlush</c>）、
///         <c>Imbued</c>（<c>AfterAutoPrePlayPhaseEntered</c>）。
///     </para>
///     <para>
///         ⭐ 引擎给了模组一个正规出口：<see cref="ModHelper.SubscribeForCombatStateHooks" /> ——
///         提供一批模型，让它们**照样**被算作该 <see cref="CombatState" /> 的钩子监听者。
///         审判具现在自己的代码里把每个具现附魔登记进来，附魔的钩子行为就与「还挂在卡上」完全一致，
///         <b>既不需要改写附魔、也不需要改写任何读取方</b>。
///         （同一套做法见 <see cref="HangingEmotionOrbs" />：让离开球位的情绪球继续收钩子。）
///     </para>
///     <para>
///         ⚠️ 附魔必须**已经绑定卡**才会被登记 —— 它自己的钩子实现里到处用 <c>base.Card</c>
///         （例如 <c>Goopy.AfterCardPlayed</c> 判 <c>cardPlay.Card != base.Card</c>）。
///         绑定由 <see cref="EnchantmentEmbodimentComponent" /> 在构建实例时完成（持久绑定，不再解绑）。
///     </para>
/// </summary>
internal static class EmbodiedEnchantmentHooks
{
    private const string SubscriptionId = "ManosabaLin.EmbodiedEnchantments";

    private static readonly List<EnchantmentModel> Tracked = [];

    private static bool _registered;

    /// <summary>在 <c>MainFile.Initialize()</c> 里调用一次。</summary>
    public static void Register()
    {
        if (_registered) return;
        _registered = true;

        ModHelper.SubscribeForCombatStateHooks(SubscriptionId, Iterate);
    }

    /// <summary>
    ///     ⚠️ 必须**立刻物化**成快照：钩子派发过程中卡/组件可能被销毁（会改登记表），
    ///     惰性迭代会抛 <c>InvalidOperationException</c>（与 <see cref="HangingEmotionOrbs" /> 同一个坑）。
    /// </summary>
    private static IEnumerable<AbstractModel> Iterate(CombatState combatState)
    {
        var snapshot = new List<AbstractModel>();

        for (var i = Tracked.Count - 1; i >= 0; i--)
        {
            var enchantment = Tracked[i];

            CardModel? card;
            try
            {
                // ⚠️ Card 的取值会 AssertMutable ⇒ 实例被冻结时抛；取不到就当作已失效剔除。
                card = enchantment.Card;
            }
            catch (Exception)
            {
                Tracked.RemoveAt(i);
                continue;
            }

            if (card is null)
            {
                // 组件已销毁 / 附魔被解绑 ⇒ 顺手从登记表里清掉（避免 static 强引用泄漏）。
                Tracked.RemoveAt(i);
                continue;
            }

            // 只对该战斗状态里的卡生效 —— 多人下别的玩家的 CombatState 不该收到它。
            if (card.CombatState != combatState) continue;

            snapshot.Add(enchantment);
        }

        return snapshot;
    }

    /// <summary>登记一批具现附魔（幂等）。</summary>
    public static void Track(IReadOnlyList<EnchantmentModel>? enchantments)
    {
        if (enchantments is null) return;

        foreach (var enchantment in enchantments)
        {
            if (enchantment is null) continue;
            if (Tracked.Contains(enchantment)) continue;
            Tracked.Add(enchantment);
        }
    }

    /// <summary>注销一批具现附魔（卡内附魔被清空时调用）。</summary>
    public static void Untrack(IReadOnlyList<EnchantmentModel>? enchantments)
    {
        if (enchantments is null) return;

        foreach (var enchantment in enchantments)
        {
            if (enchantment is null) continue;
            Tracked.Remove(enchantment);
        }
    }
}
