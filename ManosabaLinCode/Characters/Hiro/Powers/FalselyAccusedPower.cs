using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Hiro.Powers;

/// <summary>
///     被伪证 - 由「我才是正确」点选玩家后施加，回合开始时消失。
///     <para>
///         持有者被怪物攻击、实际掉血时，按掉血的数值消耗<b>施加者（希罗）</b>的伪证
///         （伪证不足时把正义按 1:5 兑换成伪证一起消耗，多余保留为伪证），
///         然后持有者回复等量的生命。
///     </para>
/// </summary>
[RegisterPower]
public sealed class FalselyAccusedPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    ///     「回合开始消失」：施加后要<b>撑过本回合剩下的时间 + 整个敌人回合</b>，
    ///     到持有者下一个回合开始时才移除。
    ///     <para>
    ///         本引擎里一回合内所有玩家的 <c>AfterPlayerTurnStart</c> 都在该回合开局阶段
    ///         依次触发完（见 <c>CombatManager</c> 的 playersStartingTurn 循环），
    ///         之后才轮到玩家出牌、再进敌人回合。所以「应用 → 敌人回合 → 玩家回合开始移除」
    ///         这个时序天然成立。
    ///     </para>
    ///     <para>
    ///         这里仍然只认持有者自己的回合开始（与 <c>EmaTrialBadge</c> 等原版 / 本模组
    ///         遗物的写法一致）：保证无论队友回合开始的事件顺序如何，
    ///         都不会在敌人回合之前被移除。
    ///     </para>
    /// </summary>
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner) return Task.CompletedTask;

        RemoveInternal();
        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner) return;
        if (result.UnblockedDamage <= 0) return;

        // 只结算被「怪物」打出的伤害
        if (dealer is null || dealer.Side == Owner.Side) return;

        var guard = Applier;
        if (guard is null || guard.IsDead) return;

        var perjury = guard.GetPower<PerjuryPower>();
        if (perjury is null) return;

        var consumed = await perjury.ConsumeForGuard((int)result.UnblockedDamage);
        if (consumed <= 0) return;

        Flash();
        await CreatureCmd.Heal(Owner, consumed);
    }
}
