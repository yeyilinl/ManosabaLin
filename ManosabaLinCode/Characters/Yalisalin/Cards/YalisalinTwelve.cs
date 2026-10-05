using MinionLib.Component.Core;
﻿using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Yalisalin;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ManosabaLin.Characters.Yalisalin.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class YalisalinTwelve() : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Rare, TargetType.AnyPlayer)
{
    private const int RequiredSuspectAmount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ConsumeAmount", 2m),
        new PowerVar<SuspectPower>(3m),
        new EnergyVar(2),
        new PowerVar<YlsmPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<SuspectPower>();
            yield return HoverTipFactory.FromPower<IgnitePower>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;

        await CreatureCmd.TriggerAnim(source.Owner.Creature, "Cast", source.Owner.Character.CastAnimDelay);

        // 获得【嫌疑】
        await PowerCmd.Apply<SuspectPower>(
            choiceContext, source.Owner.Creature,
            source.DynamicVars["SuspectPower"].BaseValue,
            source.Owner.Creature,
            source,
            false
        );

        // 立刻触发 1 次【点火】的回合结束效果
        if (source.Owner.Creature.GetPower<IgnitePower>() is { } ignite)
            await ignite.TriggerIgnite(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["ConsumeAmount"].UpgradeValueBy(1m);
        DynamicVars["SuspectPower"].UpgradeValueBy(1m);
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
