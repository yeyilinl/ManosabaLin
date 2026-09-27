using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Emalin;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>推理成立 - 1费攻击, 4伤害; 本回合每打过1种【审判】附魔牌额外造成1段2伤害(升级: 7伤害/每段3点)</summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class ReasoningEstablished : ManosabaCardTemplate
{
    /// <summary>升级后每段额外伤害。</summary>
    private const decimal UpgradedSegmentDamage = 3m;

    /// <summary>未升级时每段额外伤害。</summary>
    private const decimal SegmentDamage = 2m;

    public ReasoningEstablished() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 第 1 段：基础伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 后续段：本回合每打过 1 种【审判】附魔牌（赞同/反驳/疑问），额外造成 1 段伤害。
        // 数「种」而不是「张」——同一种附魔打出多张也只算 1 段（与「失控的恨意」一致）。
        var enchantmentTypeCount = EmalinCombatHelper.GetDistinctEnchantmentTypesThisTurn(
            Owner.Creature, CombatState);
        if (enchantmentTypeCount <= 0 || !target.IsAlive) return;

        await DamageCmd.Attack(IsUpgraded ? UpgradedSegmentDamage : SegmentDamage)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitCount(enchantmentTypeCount)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
