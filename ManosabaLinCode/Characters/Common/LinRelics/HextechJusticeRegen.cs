using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物 6：回合结束时，若当前【正义】层数 &gt;【再生】层数，则把【再生】补到与【正义】相同层数；
///     并且持有者身上的【正义】**不再回血**。
///     <para>
///         「不再回血」由 <c>HextechJusticeNoHealPatch</c> 拦掉 <c>JusticePower.AfterSideTurnEnd</c> 的
///         <c>CreatureCmd.Heal</c> 实现（保留原有的层数递减），因为同一个能力类型无法两实例共存。
///     </para>
///     <para>
///         ⚠️ <b>为什么挂在 <see cref="BeforeSideTurnEndVeryEarly" /> 而不是 <c>AfterSideTurnEnd</c></b>：
///         <c>RegenPower</c> 在自己的 <c>BeforeSideTurnEndEarly</c> 结算回血并递减，
///         而 <c>VeryEarly</c> 是 <c>BeforeSideTurnEnd</c> 内部的<b>第一个</b>子阶段（`Hook.cs:1240-1268`：
///         `VeryEarly` → `Early` → `BeforeSideTurnEnd`，各自遍历一遍监听器）。
///         ⇒ 在这里补层，新加的【再生】会**本回合就参与结算回血**；
///         挂在 <c>AfterSideTurnEnd</c> 则要等到**下回合结束**才回血（旧实现的 bug）。
///     </para>
///     <para>
///         另外此刻读到的 <c>JusticePower.Amount</c> 还没被它自己的 <c>AfterSideTurnEnd</c> 递减
///         ⇒ 拿到的就是「回合结束时的当前【正义】层数」，语义正确且与监听器顺序无关。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechJusticeRegen : ManosabaRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>该生物的主人是否持有本遗物（供 patch 使用）。</summary>
    internal static bool IsActiveFor(Creature? creature)
        => creature?.Player?.Relics.OfType<HextechJusticeRegen>().Any() == true;

    public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner.Creature)) return;

        var justice = Owner.Creature.GetPower<JusticePower>();
        if (justice is null || justice.Amount <= 0) return;

        var currentRegen = Owner.Creature.GetPower<RegenPower>()?.Amount ?? 0m;
        var delta = justice.Amount - currentRegen;
        if (delta <= 0) return;

        Flash();
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, delta, Owner.Creature, null, false);
    }
}
