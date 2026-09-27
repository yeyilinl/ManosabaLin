using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Emalin;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>失控的恨意 - 2费攻击, 13伤害; 本回合每打过1种【审判】附魔牌 +13伤害 并随机给敌方全体1层易伤或1层虚弱(升级+4基础伤害)
/// （+13 与「随机 1 层易伤/虚弱」一一对应 ⇒ N 种附魔触发 N 次；伤害一段结算，触发在伤害之后）</summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class EmalinRestCeremony : ManosabaCardTemplate
{
    /// <summary>每种【审判】附魔牌提供的额外伤害。</summary>
    private const decimal DamagePerEnchantmentType = 13m;

    /// <summary>每次随机给予的层数（易伤或虚弱，二选一）。</summary>
    private const decimal DebuffAmount = 1m;

    public EmalinRestCeremony() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(13m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var distinctTypes = EmalinCombatHelper.GetDistinctEnchantmentTypesThisTurn(Owner.Creature, CombatState);
        var totalDamage = DynamicVars.Damage.BaseValue + distinctTypes * DamagePerEnchantmentType;

        await DamageCmd.Attack(totalDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (distinctTypes <= 0) return;

        var enemies = CombatState.Enemies.Where(enemy => enemy.IsAlive).ToList();
        if (enemies.Count == 0) return;

        // 每一种【审判】附魔牌就是一次「伤害 +13」，也就是一次触发 ⇒ 掷一次骰子。
        // 掷到哪种就给敌方全体上哪种（层数固定 1 层），所以 N 种附魔 = N 次触发 = 敌方全体共吃 N 层。
        // 用 Rng.Niche（引擎里泛指「其它随机」的同步流）—— 多人下必须走 RunState 的同步 RNG，
        // 且刻意不复用 CombatCardSelection，免得把同一回合其它选牌用的流位置带偏。
        // ⚠️ 触发在伤害**之后**：因此这 N 次易伤不会反过来放大本次伤害（伤害是确定的，不随 RNG 浮动）。
        var rng = Owner.RunState.Rng.Niche;
        for (var i = 0; i < distinctTypes; i++)
        {
            if (rng.NextInt(2) == 0)
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, DebuffAmount, Owner.Creature, this, false);
            else
                await PowerCmd.Apply<WeakPower>(choiceContext, enemies, DebuffAmount, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
