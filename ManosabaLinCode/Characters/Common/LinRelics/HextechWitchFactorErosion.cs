using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     艾玛联动遗物 1：魔女因子侵蚀。
///     <para>
///         敌方阵营回合开始时，每个身上带【魔女因子】（<see cref="EmaWitchFactorPower" />）的敌人，
///         按<b>自身</b>的【魔女因子】层数受到<b>等量伤害</b>（每回合一次，无视格挡 —— 与猎人【毒】同形态的 DOT）。
///     </para>
///     <para>
///         ⚠️ 触发点选 <c>AfterSideTurnStart</c>（敌方阵营回合开始），只判 <c>side != Owner.Creature.Side</c>：
///         多人下队友回合与自己在同一个 <c>CombatSide</c>，因此队友回合不会误触发。
///     </para>
///     <para>
///         ⭐⭐ <b>本遗物造成的伤害是「无来源 DOT 伤害」，写法与猎人的【毒】完全一致（2026-10-02 用户裁定）</b>：
///         <c>CreatureCmd.Damage(choiceContext, enemy, stacks, ValueProp.Unblockable | ValueProp.Unpowered, null, null)</c>
///         —— 与 <c>PoisonPower.Trigger()</c> 同一形态：<b>无视格挡</b>（<see cref="ValueProp.Unblockable" />，
///         引擎注释原文 "HP loss like Poison"）+ <b>不吃力量加成</b>（<see cref="ValueProp.Unpowered" />，
///         原版遗物 / 药水 / 能力伤害）+ <b>没有攻击者</b>（6 参重载里 <c>dealer = cardSource?.Owner.Creature</c>，
///         cardSource 传 null ⇒ dealer 为 null）。
///     </para>
///     <para>
///         ⚠️⭐ <b>「无来源」本身就是这条规则的全部机制，不需要任何补丁</b>：
///         <see cref="EmaWitchKillerPower" /> 的 <c>AfterDamageGiven</c> 开头就是 <c>if (dealer == null) return;</c>
///         ⇒ dealer 为 null 时它<b>天然不会</b>给敌人补【魔女因子】（否则敌人每回合自己给自己涨因子，
///         侵蚀伤害会指数级滚起来）。同理 <see cref="ValueProp.Unpowered" /> 也不满足
///         <c>EmaWitchFactorPower</c> 的 <c>IsPoweredAttack()</c> 判据 ⇒ 不会误触【魔女因子】的即死。
///         记住：<b>绝不要给这次伤害传 dealer / cardSource</b>。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechWitchFactorErosion : ManosabaRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner?.Creature is not { } self) return;
        if (combatState is null) return;

        // 只在自己以外的阵营回合生效（= 敌人回合）。
        if (side == self.Side) return;

        var choiceContext = new ThrowingPlayerChoiceContext();
        var flashed = false;

        foreach (var enemy in combatState.Enemies.Where(static e => e.IsAlive).ToList())
        {
            var stacks = enemy.GetPower<EmaWitchFactorPower>()?.Amount ?? 0;
            if (stacks <= 0) continue;

            if (!flashed)
            {
                Flash();
                flashed = true;
            }

            // ⭐⭐ 无来源 DOT 伤害（与猎人【毒】同形态，见类注释）。
            //     无视格挡 + 不吃力量加成 + 没有攻击者：
            //     最后两个 null 是 cardSource / cardPlay，6 参重载内部
            //     `dealer = cardSource?.Owner.Creature` ⇒ dealer 为 null ⇒
            //     【魔女杀手】的 AfterDamageGiven（开头 `if (dealer == null) return;`）天然不触发。
            await CreatureCmd.Damage(choiceContext, enemy, stacks,
                ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
    }
}
