using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;

namespace ManosabaLin.Characters.Hiro.Cards;

/// <summary>
///     我才是正确 - 1 费能力（升级 0 费），稀有，多人专属。
///     <para>
///         每回合可以点击这个能力图标，选择任意玩家（含自己）给予「被伪证」（回合开始消失）。
///         带「被伪证」的玩家被怪物打出实际伤害时，按掉血数值消耗希罗的伪证（伪证不足时
///         把正义按 1:5 兑换成伪证一起消耗，多余保留为伪证），然后回复等量生命。
///     </para>
/// </summary>
[RegisterCard(typeof(HirolinCardPool))]
public sealed class IAmCorrect : ManosabaCardTemplate
{
    public IAmCorrect() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<IAmCorrectAction>(); }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<IAmCorrectAction>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
