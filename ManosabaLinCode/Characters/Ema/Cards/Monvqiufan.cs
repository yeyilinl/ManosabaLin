using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Emalin;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ManosabaLin.Characters.Common.Powers;
using ManosabaLin.Characters.Ema.Powers;

namespace ManosabaLin.Characters.Ema.Cards;

[RegisterCard(typeof(EmalinCardPool))]
public sealed class Monvqiufan : ManosabaCardTemplate
{
    public Monvqiufan() : base(3, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new PowerVar<TempStrength>(6m);
            yield return new PowerVar<TempDexterity>(5m);
            yield return new BlockVar(9m, ValueProp.Move);
        }
    }

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
        var source = this;
        var creature = source.Owner.Creature;

        // 获得6层临时力量
        await PowerCmd.Apply<TempStrength>(
            choiceContext, creature,
            source.DynamicVars["TempStrength"].BaseValue,
            creature, source, false);

        // 获得5层临时迅捷
        await PowerCmd.Apply<TempDexterity>(
            choiceContext, creature,
            source.DynamicVars["TempDexterity"].BaseValue,
            creature, source, false);

        // 传播9点【魔女因子】：对全体存活敌人各施加9层魔女因子
        var factorAmount = source.DynamicVars["Block"].BaseValue;
        var enemies = creature.CombatState.Enemies
            .Where(static e => e.IsAlive)
            .ToArray();
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<EmaWitchFactorPower>(
                choiceContext, enemy, factorAmount, creature, source, false);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}