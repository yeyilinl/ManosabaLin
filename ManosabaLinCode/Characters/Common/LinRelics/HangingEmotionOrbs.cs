using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     「已脱离球位、挂在血条下方」的<b>持续型情绪球</b>登记处（联动遗物 2）。
///     <para>
///         核心机制：引擎 <c>CombatState.IterateHookListeners</c> 只会枚举
///         <c>OrbQueue.Orbs</c> 里的球 ⇒ 球一旦离开队列就收不到战斗钩子。
///         但引擎给了模组一个正规出口 ——
///         <see cref="ModHelper.SubscribeForCombatStateHooks" />：模组可以提供一批模型，
///         让它们**照样**被算作该 <c>CombatState</c> 的钩子监听者。
///     </para>
///     <para>
///         所以「脱离球位、换个地方挂着、效果继续跑」不需要任何能力（Power），也不需要重写效果逻辑 ——
///         把球<b>本身</b>（同一个 <see cref="OrbModel" /> 对象）从队列里摘下来，登记到这里，
///         它的被动/钩子照旧按原样继续工作；血条下方那幅画面只是它的<b>新显示位置</b>。
///         ⚠️ 这正是用户 2026-09-28/29 两次强调的口径：
///         「卡牌挂在血条下面不就等于充能球换个地方挂着」「依旧不要能力」。
///     </para>
///     <para>
///         键是 <see cref="Player" />：模型侧完全确定性（只依赖同步的球位状态），
///         因此联机两端登记出的集合一致，钩子触发也一致。
///     </para>
/// </summary>
internal static class HangingEmotionOrbs
{
    private const string SubscriptionId = "ManosabaLin.HangingEmotionOrbs";

    private static readonly Dictionary<Player, List<OrbModel>> Hanging = [];

    private static bool _registered;

    /// <summary>某个玩家的挂卡集合发生变化（UI 刷新用；纯本地显示，不参与模型逻辑）。</summary>
    public static event Action<Player>? Changed;

    /// <summary>在 <c>MainFile.Initialize()</c> 里调用一次：把「挂着的球」注册成战斗钩子监听者。</summary>
    public static void Register()
    {
        if (_registered) return;
        _registered = true;

        ModHelper.SubscribeForCombatStateHooks(SubscriptionId, Iterate);
    }

    /// <summary>
    ///     ⚠️ 必须**立刻物化**成列表：钩子派发过程中可能有人 <see cref="Release" />（会改字典），
    ///     惰性迭代字典会抛 <c>InvalidOperationException</c>。
    /// </summary>
    private static IEnumerable<AbstractModel> Iterate(CombatState combatState)
    {
        var snapshot = new List<AbstractModel>();

        foreach (var (player, orbs) in Hanging)
        {
            if (player.Creature.CombatState != combatState) continue;

            foreach (var orb in orbs)
            {
                if (orb.HasBeenRemovedFromState) continue;
                snapshot.Add(orb);
            }
        }

        return snapshot;
    }

    public static IReadOnlyList<OrbModel> For(Player player)
    {
        if (!Hanging.TryGetValue(player, out var list)) return [];
        PruneStale(player, list);
        return list;
    }

    /// <summary>挂着的球数量（UI 每帧判空用 —— 不必为了「没有」而每次分配一个空列表）。</summary>
    public static int Count(Player player)
    {
        if (!Hanging.TryGetValue(player, out var list)) return 0;
        PruneStale(player, list);
        return list.Count;
    }

    /// <summary>雀跃的循环接续用：该玩家有没有「挂着的」某类情绪球。</summary>
    public static bool HasHanging(Player? player, Func<OrbModel, bool> predicate)
    {
        if (player is null || !Hanging.TryGetValue(player, out var list)) return false;
        PruneStale(player, list);
        return list.Any(predicate);
    }

    /// <summary>
    ///     剔除「已经不在战斗状态里」的球（<c>HasBeenRemovedFromState</c>）—— 引擎若在别的路径上把它移除，
    ///     登记表里的残留条目会让显示层继续把它画出来。顺手清掉变空的条目，避免 <see cref="Player" /> 强引用泄漏。
    ///     <para>⚠️ 只在**读**路径上顺手做，不能反过来指望它替代战斗结束的 <see cref="ReleaseAll" />。</para>
    /// </summary>
    private static bool PruneStale(Player player, List<OrbModel> list)
    {
        var removed = list.RemoveAll(static orb => orb.HasBeenRemovedFromState);
        if (removed > 0 && list.Count == 0)
            Hanging.Remove(player);

        return removed > 0;
    }

    /// <summary>
    ///     把一个持续型情绪球从球位「摘下来」挂到血条下方。
    ///     ⚠️ 调用方负责已经把它从 <c>OrbQueue</c> 里移除（本方法只管登记与通知）。
    /// </summary>
    public static void Hang(Player player, OrbModel orb)
    {
        if (!Hanging.TryGetValue(player, out var list))
        {
            list = [];
            Hanging[player] = list;
        }

        if (list.Contains(orb)) return;

        list.Add(orb);
        Changed?.Invoke(player);
    }

    /// <summary>
    ///     释放某玩家全部挂着的球（**下个玩家回合开始**调用 —— 与球体自身
    ///     <c>AfterTurnStartOrbTrigger ⇒ EvokeNext</c>「到下回合开始消散」对齐）。
    /// </summary>
    public static void Release(Player player)
    {
        if (!Hanging.TryGetValue(player, out var list) || list.Count == 0) return;

        Hanging.Remove(player);

        foreach (var orb in list)
            orb.RemoveInternal();

        Changed?.Invoke(player);
    }

    /// <summary>
    ///     释放**所有**玩家挂着的球（**战斗结束**时调用）。
    ///     <para>
    ///         ⚠️ 之前只有「下个玩家回合开始」的 <see cref="Release(Player)" />；「球还挂着战斗就结束 /
    ///         持有者阵亡」这条路径没有任何收口 ⇒ <see cref="Hanging" />（<c>static</c> 且键是
    ///         <see cref="Player" /> 强引用）会一直留着：轻则 <c>EmotionHangDisplay</c> 画出**幻影挂位**，
    ///         重则跨 run 泄漏内存。战斗结束是唯一可靠的收口点。
    ///     </para>
    ///     <para>
    ///         幂等：多人下每端、每个持有遗物的玩家各调一次也无副作用
    ///         （已从状态移除的球不再重复 <c>RemoveInternal</c>，空表直接返回）。
    ///     </para>
    /// </summary>
    public static void ReleaseAll()
    {
        if (Hanging.Count == 0) return;

        var snapshot = Hanging.ToArray();
        Hanging.Clear();

        foreach (var (player, orbs) in snapshot)
        {
            foreach (var orb in orbs)
            {
                if (!orb.HasBeenRemovedFromState)
                    orb.RemoveInternal();
            }

            Changed?.Invoke(player);
        }
    }
}
