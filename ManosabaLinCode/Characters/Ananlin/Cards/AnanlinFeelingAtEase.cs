using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ananlin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;

namespace ManosabaLin.Characters.Ananlin.Cards;

/// <summary>
///     感到安心 - 2 费技能（升级 1 费），多人专属。
///     <para>失去自己的全部【安心】，按失去的层数给每个友方（含自己）各随机获得「1 点能量 或 抽 1 张牌」，随后回满【安心】。</para>
/// </summary>
[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinFeelingAtEase : ManosabaCardTemplate
{
    public AnanlinFeelingAtEase() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 失去全部安心（LosePeaceOfMind 在失去 >= 2 层时会附带一次「选 1 张手牌获得重放 1」）
        var lost = await this.LosePeaceOfMind(choiceContext, int.MaxValue);

        if (lost > 0)
        {
            var rng = Owner.RunState.Rng.CombatCardSelection;

            foreach (var ally in CombatState.Players.Where(
                         p => p.Creature.Side == Owner.Creature.Side && p.Creature.IsAlive))
            {
                for (var i = 0; i < lost; i++)
                {
                    if (rng.NextBool())
                        await PlayerCmd.GainEnergy(1m, ally);
                    else
                        await CardPileCmd.Draw(choiceContext, 1m, ally);
                }
            }
        }

        // 回满安心
        await PowerCmd.Apply<AnanlinPeaceOfMindPower>(
            choiceContext, Owner.Creature, AnanlinPeaceOfMindPower.MaxStacks, Owner.Creature, this);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
