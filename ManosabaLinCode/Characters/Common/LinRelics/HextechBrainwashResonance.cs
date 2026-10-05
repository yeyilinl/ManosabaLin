using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物（安安 3/3）：**洗脑与缄默共用【缄默】的可成长替换池**，
///     且**洗脑每场战斗的第一次不获得【洗脑反噬】**。
///     <list type="number">
///         <item>
///             <b>共用可成长替换池</b>：默认情况下【洗脑】改写意图时用的是**初始值**
///             （<c>AnanlinSilenceIntentManager.ForceBrainwashAndGetTargets</c> 里写死的 <c>baseBonus: 0</c>），
///             不随【缄默】成长；持有本遗物后改为**读取【缄默】当前的可成长池数值**
///             （<c>GetSilenceGrowth</c>）⇒ 洗脑也吃到缄默攒下来的加成。
///             ⚠️ 用户（海克斯联动第 5 条）裁定：洗脑不仅读取、还**推进**同一个通用意图池 ——
///             与缄默一样，每次成功改写后让成长 +1（<c>ForceBrainwashAndGetTargets</c> 末尾）。
///         </item>
///         <item>
///             <b>首战免反噬</b>：每场战斗**第一次**强制洗脑成功时，跳过
///             <c>AnanlinBrainwashBacklashPower</c> 的获得（仍照常获得 25 层【魔女化】）。
///             标记用 <see cref="BrainwashBacklashWaivedThisCombat" />（<c>[SavedProperty]</c> ⇒ 联机同步），
///             在 <see cref="BeforeCombatStart" /> 重置。
///         </item>
///     </list>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechBrainwashResonance : ManosabaRelicTemplate
{
    /// <summary>本场战斗是否已经用掉「洗脑首战免反噬」。</summary>
    [SavedProperty] public bool BrainwashBacklashWaivedThisCombat { get; set; }

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override Task BeforeCombatStart()
    {
        BrainwashBacklashWaivedThisCombat = false;
        return Task.CompletedTask;
    }

    /// <summary>该玩家是否持有本遗物（洗脑是否吃【缄默】的可成长替换池）。</summary>
    internal static bool IsActiveFor(Player? player)
        => player?.Relics.OfType<HextechBrainwashResonance>().Any() == true;

    /// <summary>
    ///     本场战斗第一次强制洗脑成功时返回 <c>true</c> 并打上标记（此后恒返回 <c>false</c>）。
    ///     调用方据此跳过【洗脑反噬】的获得。
    /// </summary>
    internal static bool TryWaiveBrainwashBacklash(Player? player)
    {
        var relic = player?.Relics.OfType<HextechBrainwashResonance>().FirstOrDefault();
        if (relic is null || relic.BrainwashBacklashWaivedThisCombat) return false;

        relic.BrainwashBacklashWaivedThisCombat = true;
        relic.Flash();
        return true;
    }
}
