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

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>监狱的法则 - 1费攻击, 6伤害, 本回合打过疑问附魔牌则先给2层易伤再伤害+9, 升级+3伤</summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class PrisonLaw : ManosabaCardTemplate
{
    /// <summary>满足条件时对目标施加的易伤层数。</summary>
    private const decimal VulnerableAmount = 2m;

    public PrisonLaw() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6m, ValueProp.Move), new IntVar("Bonus", 9)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        // 只认吃到了【疑问】附魔的牌；只有疑问关键字、没吃到附魔的牌不算疑问牌
        var doubtCount = EmalinCombatHelper.GetDoubtPlaysThisTurn(Owner.Creature, CombatState);
        var triggered = doubtCount > 0;

        // 易伤按卡面文案顺序先施加，所以本次伤害会吃到易伤的 +50%
        if (triggered)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext, cardPlay.Target!, VulnerableAmount, Owner.Creature, this, false);
        }

        var totalDamage = DynamicVars.Damage.BaseValue
            + (triggered ? DynamicVars["Bonus"].BaseValue : 0m);

        await DamageCmd.Attack(totalDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
