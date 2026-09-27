using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>
///     审判开庭（3 费 能力・罕见，升级 2 费）：
///     打出时选择 1 名友方。此后每回合内，当你任一【审判】附魔计数达到 5 时进行锚定，
///     之后该类附魔计数的额外效果会同步给所选友方。
/// </summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class EnchantmentConvergence()
    : ManosabaCardTemplate(3, CardType.Power, CardRarity.Uncommon, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<EnchantmentConvergencePower>(); }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        // 「选择一个友方」：AnyAlly 已是「除自己以外的任意玩家」，这里再防御一次同侧校验。
        var target = cardPlay.Target;
        if (target is not { IsAlive: true } || target.Side != owner.Creature.Side)
            return;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<EnchantmentConvergencePower>(
            choiceContext, owner.Creature, 1m, owner.Creature, this, false);

        // 能力挂在自己身上（计数来源是 EmaTrialBadge），把所选友方记进能力里。
        if (power is not null)
            power.ChosenAlly = target;
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
