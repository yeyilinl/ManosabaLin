using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>
///     共同审判 - 3 费能力（升级 2 费），稀有，多人专属。
///     <para>
///         所有人每累计打出 5 张牌，就让每个玩家各自随机生效一次【审判】附魔的额外效果
///         （赞同 / 疑问 / 反驳 各 5 档，共 15 种中随机 1 种）。
///     </para>
/// </summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class JointJudgment : ManosabaCardTemplate
{
    public JointJudgment() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<JointJudgmentPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
