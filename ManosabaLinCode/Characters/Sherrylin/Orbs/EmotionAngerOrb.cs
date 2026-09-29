using System.Linq;
using System.Threading.Tasks;
using Godot;
using ManosabaLin.Characters.Sherrylin.Cards.Emotions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Sherrylin.Orbs;

/// <summary>
/// 愤怒球体：本回合造成双倍伤害；每造成 20 伤害，对随机友方造成 1 点伤害。
/// <para>
///     后半段（2026-09-29 补齐）对着本地化文案
///     （<c>MANOSABA_LIN_CARD_EMOTION_ANGER.description</c> /
///     <c>MANOSABA_LIN_ORB_EMOTION_ANGER_ORB.description</c>）
///     「造成双倍伤害，每造成20伤害对随机友方造成1点伤害」。
/// </para>
/// </summary>
[RegisterOrb]
public sealed class EmotionAngerOrb : EmotionOrb<EmotionAnger>
{
    /// <summary>累计多少点伤害就反噬 1 点。</summary>
    private const int DamagePerBacklash = 20;

    /// <summary>尚未凑满 <see cref="DamagePerBacklash" /> 的零头。</summary>
    private decimal _damageDealt;

    /// <summary>结算反噬时的重入保护 —— 反噬自己造成的伤害绝不能再被统计进去。</summary>
    private bool _resolvingBacklash;

    protected override Color OrbColor => new(1f, 0.2f, 0.2f);

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner.Creature) return 1m;
        if (target == null || target.Side == Owner.Creature.Side) return 1m;
        if (amount <= 0) return 1m;
        return 2m;
    }

    /// <summary>
    ///     统计「你打出去的伤害」。<b>只算打敌人的</b>（打自己/队友不算），
    ///     数值取 <see cref="DamageResult.TotalDamage" />（= 格挡掉的 + 实打实的），
    ///     即玩家看到的、被球翻倍后的那一串数字。
    ///     每凑满 20 点 ⇒ 对<b>随机友方</b>（含自己）造成 1 点伤害。
    ///     <para>
    ///         ⚠️ 随机目标走 <c>RunState.Rng.CombatTargets</c>（联机 desync 铁律）。
    ///     </para>
    /// </summary>
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (_resolvingBacklash) return;
        if (dealer != Owner.Creature) return;
        if (target.Side == Owner.Creature.Side) return;

        _damageDealt += result.TotalDamage;
        if (_damageDealt < DamagePerBacklash) return;

        var ticks = (int)(_damageDealt / DamagePerBacklash);
        _damageDealt -= ticks * DamagePerBacklash;

        var allies = Owner.Creature.CombatState?
            .GetTeammatesOf(Owner.Creature)
            .Where(static c => c.IsAlive)
            .ToList();
        if (allies is not { Count: > 0 }) return;

        var rng = Owner.RunState.Rng.CombatTargets;

        _resolvingBacklash = true;
        try
        {
            for (var i = 0; i < ticks; i++)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    allies[rng.NextInt(allies.Count)],
                    1m,
                    ValueProp.Unpowered,
                    null,
                    null);
            }
        }
        finally
        {
            _resolvingBacklash = false;
        }
    }
}
