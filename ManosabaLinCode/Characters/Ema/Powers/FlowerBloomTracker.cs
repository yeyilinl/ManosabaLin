using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;

namespace ManosabaLin.Characters.Ema.Powers;

/// <summary>
///     花朵绽放的全局登记表。
///     <para>
///         变形 / 生成拦截补丁（<c>CardCmd.Transform</c>、<c>CardPileCmd.AddGeneratedCardsToCombat</c>）
///         位于引擎热路径上，每次调用都去遍历所有玩家身上的能力代价太高，
///         因此这里用一个极廉价的 <see cref="Any" /> 开关作为前置短路，
///         只有本回合真的有花朵绽放生效时才继续做进一步判定。
///     </para>
/// </summary>
internal static class FlowerBloomTracker
{
    private static readonly List<FlowerBloomPower> Active = [];

    /// <summary>本回合是否有任何花朵绽放生效（热路径短路开关）。</summary>
    internal static bool Any => Active.Count > 0;

    /// <summary>
    ///     拦截补丁的递归保护：补丁内部会重新调用被拦截的原方法，
    ///     期间必须让前缀直接放行，否则会无限递归。
    /// </summary>
    internal static bool Intercepting { get; set; }

    internal static void Register(FlowerBloomPower power)
    {
        foreach (var existing in Active)
            if (ReferenceEquals(existing, power))
                return;

        Active.Add(power);
    }

    internal static void Unregister(FlowerBloomPower power)
    {
        for (var i = Active.Count - 1; i >= 0; i--)
            if (ReferenceEquals(Active[i], power))
                Active.RemoveAt(i);
    }

    /// <summary>该生物是否被任意一次花朵绽放监听。</summary>
    internal static bool IsWatched(Creature? creature)
    {
        if (creature is null || Active.Count == 0) return false;

        foreach (var power in Active)
            if (power.IsWatching(creature))
                return true;

        return false;
    }
}
