using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using ManosabaLin.Characters.Emalin;

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>共犯的沉默 - 2费技能, 1能量, 打过赞同和反驳则全体2能量且友方各1层汉娜的魔法, 升级1费</summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class AccompliceSilence : ManosabaCardTemplate
{
    public AccompliceSilence() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<HnmPower>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await PlayerCmd.GainEnergy(1m, Owner);

        if (!EmalinCombatHelper.HasPlayedBothAgreementAndRebuttal(Owner.Creature, CombatState)) return;

        foreach (var player in CombatState.Players)
            await PlayerCmd.GainEnergy(2m, player);

        // 友方（不含自己）各获得1层【远野汉娜的魔法】
        foreach (var ally in CombatState.Allies.Where(a => a is { IsAlive: true } && a != Owner.Creature))
            await PowerCmd.Apply<HnmPower>(choiceContext, ally, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
