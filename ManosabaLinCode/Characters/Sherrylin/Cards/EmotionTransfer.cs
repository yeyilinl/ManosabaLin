using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.Components;
using ManosabaLin.Characters.Sherrylin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Sherrylin.Cards;

/// <summary>
///     传递的情绪 - 1 费技能，稀有，多人专属。
///     <para>获得 1 层【复杂的情绪】，并选择 1 名队友：本回合内其每打出一张牌，雪莉获得 1 层【情绪】。</para>
///     <para>升级后改为选择 2 名队友（先通过卡牌目标选 1 名，再弹一次玩家选择屏选第 2 名）。</para>
/// </summary>
[RegisterCard(typeof(SherrylinCardPool))]
public sealed class EmotionTransfer() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    private const string TargetsKey = "Targets";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(TargetsKey, 1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 获得 1 层【复杂的情绪】
        await PowerCmd.Apply<EmotionFusionPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);

        var wanted = DynamicVars[TargetsKey].IntValue;
        var chosen = new List<Player>();

        // 第 1 名来自卡牌目标
        if (cardPlay.Target?.Player is { } first && first != Owner && first.Creature.IsAlive)
            chosen.Add(first);

        // 升级后需要第 2 名：再弹一次玩家选择屏
        while (chosen.Count < wanted)
        {
            var candidates = CombatState.Players
                .Where(p => p != Owner
                            && p.Creature.Side == Owner.Creature.Side
                            && p.Creature.IsAlive
                            && !chosen.Contains(p))
                .ToList();
            if (candidates.Count == 0) break;

            var picked = await PlayerPickScreen.Choose(
                choiceContext,
                Owner,
                new LocString("cards", $"{Id.Entry}.pickPrompt"),
                candidates);
            if (picked is null) break;

            chosen.Add(picked);
        }

        if (chosen.Count == 0) return;

        var power = await PowerCmd.Apply<EmotionTransferPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        power?.Watch(chosen.Select(static p => p.Creature));
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars[TargetsKey].UpgradeValueBy(1m);
    }
}
