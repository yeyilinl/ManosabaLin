using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Emalin.Components;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物 3：魔女化核心。
///     <para>
///         效果：按照【魔女化】组件（<see cref="Witchification" />）自身的节奏 ——
///         带组件的卡在<b>回合结束被保留、并在下个回合开始 +1 计数</b>时，获得【魔女化】层数。
///         即【魔女化计数】每 +1 ⇒ 立刻获得 <b>10 层【魔女化】</b>
///         （由 <see cref="Witchification.AddWitchificationCountAsync" /> 回调本遗物的
///         <see cref="OnWitchificationCountGained" />）。
///     </para>
///     <para>
///         ⚠️ 用户 2026-09-28 裁定：「10 倍」= <b>计数每 +1 就 +10 层【魔女化】</b>，
///         <b>不是</b>「挂上组件那一刻按当前【魔女化】层数 × 10 放大一次」（旧实现已删）。
///     </para>
///     <para>
///         ⚠️ 用户（海克斯联动第 2 条）裁定：本遗物<b>只有这一件事</b> ——
///         旧版「回合开始把 &gt;300 层削减到 100 并 25:1 转计数」的另一套逻辑已删除。
///         「获得魔女化」只发生在组件「保留 → +1 计数」这条链上，不再有削层折返。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechWitchificationCore : ManosabaRelicTemplate
{
    /// <summary>计数每 +1 兑换的【魔女化】层数。</summary>
    internal const int WithPowerPerCount = 20;

    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>
    ///     由 <c>Witchification.AddWitchificationCountAsync</c> 回调：计数每 +1 ⇒ +10 层【魔女化】。
    ///     遗物不在场 / 计数没有真正增加时直接返回。
    ///     <para>
    ///         ⚠️ 由调用方 <c>await</c>（顺序发动）。<paramref name="choiceContext" /> 为 <c>null</c> 时
    ///         才退化成 <c>ThrowingPlayerChoiceContext</c> —— 那个上下文一旦下游真的发起玩家选择就会抛，
    ///         而异常在过去会被 fire-and-forget 吞掉（表现为「偶尔不加层」）。
    ///     </para>
    /// </summary>
    internal static async Task OnWitchificationCountGained(
        Player? owner, int amount, PlayerChoiceContext? choiceContext = null)
    {
        if (owner is null || amount <= 0) return;

        var relic = owner.Relics.OfType<HextechWitchificationCore>().FirstOrDefault();
        if (relic is null)
        {
            MainFile.Logger.Warn(
                $"[HextechWitchificationCore][诊断] 计数 +{amount} 但玩家 {owner.Character?.GetType().Name} 身上找不到本遗物（是否用 RelicCmd.Obtain<HextechWitchificationCore>(player) 获取？）");
            return;
        }

        MainFile.Logger.Info(
            $"[HextechWitchificationCore][诊断] 计数 +{amount} → 发放 {amount * WithPowerPerCount} 层【魔女化】给 {owner.Character?.GetType().Name}");
        relic.Flash();
        await PowerCmd.Apply<WithPower>(
            choiceContext ?? new ThrowingPlayerChoiceContext(),
            owner.Creature,
            amount * WithPowerPerCount,
            owner.Creature,
            null,
            false);
    }
}
