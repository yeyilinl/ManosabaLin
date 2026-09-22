using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class YalisalinWitchPrisoner() : ManosabaCardTemplate(3, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<TempStrength>("TempStrength", 6m),
        new PowerVar<TempDexterity>("TempDexterity", 6m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<TempStrength>();
            yield return HoverTipFactory.FromPower<TempDexterity>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<TempStrength>(
            choiceContext, Owner.Creature,
            DynamicVars["TempStrength"].BaseValue,
            Owner.Creature,
            this,
            false
        );

        await PowerCmd.Apply<TempDexterity>(
            choiceContext, Owner.Creature,
            DynamicVars["TempDexterity"].BaseValue,
            Owner.Creature,
            this,
            false
        );

        // 予燎6点附带升温的火色：分6次，每次随机给予任意敌人1点浅橙火色，给予后满足升温条件则触发升温
        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        var enemies = Owner.Creature.CombatState.Enemies
            .Where(static e => e.IsAlive)
            .ToArray();
        if (enemies.Length == 0) return;

        var rng = Owner.RunState.Rng.CombatTargets;
        for (var i = 0; i < 6; i++)
        {
            var target = rng.NextItem(enemies);
            var wasFull = hairpin.IsFireColorFull(target);
            if (!hairpin.TryAddFireColor(target, 1, this))
                continue;

            // 若给予后满足升温条件（火色被补满），触发升温效果
            if (!wasFull && hairpin.IsFireColorFull(target))
                await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this, strong: false);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
