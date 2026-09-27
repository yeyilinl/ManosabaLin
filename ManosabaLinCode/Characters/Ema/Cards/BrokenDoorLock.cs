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
using System.Threading.Tasks;
using STS2RitsuLib.Keywords;

namespace ManosabaLin.Characters.Ema.Cards;


/// <summary>坏掉的门锁 - 1费攻击, 8伤害; 若这一击有伤害落在目标的格挡上, 给予目标1层易伤(升级+4伤)</summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class BrokenDoorLock : ManosabaCardTemplate
{
    /// <summary>被打到格挡上时施加的易伤层数。</summary>
    private const decimal VulnerableAmount = 1m;

    public BrokenDoorLock() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { EmalinKeywordRules.RebuttalCardKeyword };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 不再无视格挡：正常结算，靠 DamageResult.BlockedDamage 判断「有没有打到格挡上」
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash");
        await attack.Execute(choiceContext);

        // BlockedDamage > 0 = 这一击至少有一部分被格挡吃掉（目标原本有格挡且真的吸收了伤害）
        var hitBlock = attack.Results
            .SelectMany(results => results)
            .Any(result => result.Receiver == target && result.BlockedDamage > 0);

        if (hitBlock && target.IsAlive)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext, target, VulnerableAmount, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
