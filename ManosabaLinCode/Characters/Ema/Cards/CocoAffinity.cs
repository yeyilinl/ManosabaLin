using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Ema.Cards;

[RegisterCard(typeof(EmalinCardPool))]
public sealed class CocoAffinity : ManosabaCardTemplate
{
    public CocoAffinity() : base(4, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<BondPower>(); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var creature = owner.Creature;
        var target = cardPlay.Target!;

        var bond = creature.GetPower<BondPower>();
        if (bond != null) bond.Affinity++;

        if (target.Monster?.NextMove?.Intents == null) return;

        var allies = CombatState.Allies.Where(a => a is { IsAlive: true }).ToList();
        var intents = target.Monster.NextMove.Intents
            .Where(i => IsSupportedIntent(i.IntentType))
            .ToList();
        if (intents.Count == 0) return;

        foreach (var intent in intents)
            await ApplyIntentEffect(choiceContext, creature, allies, intent.IntentType);

        if (bond != null && bond.Affinity > bond.Estrangement)
        {
            var extra = intents[owner.RunState.Rng.CombatCardSelection.NextInt(intents.Count)];
            await ApplyIntentEffect(choiceContext, creature, allies, extra.IntentType);
        }
    }

    private static bool IsSupportedIntent(IntentType intentType) => intentType switch
    {
        IntentType.Attack or IntentType.DeathBlow
            or IntentType.Defend or IntentType.Buff
            or IntentType.Debuff or IntentType.DebuffStrong
            or IntentType.Summon or IntentType.StatusCard => true,
        _ => false
    };

    private async Task ApplyIntentEffect(
        PlayerChoiceContext choiceContext,
        Creature source,
        List<Creature> allies,
        IntentType intentType)
    {
        switch (intentType)
        {
            case IntentType.Attack:
            case IntentType.DeathBlow:
                foreach (var ally in allies)
                    await PowerCmd.Apply<HardenedShellPower>(choiceContext, ally, 20, source, this, false);
                break;

            case IntentType.Defend:
            case IntentType.Buff:
                foreach (var ally in allies)
                    await PowerCmd.Apply<RitualPower>(choiceContext, ally, 9, source, this, false);
                break;

            case IntentType.Debuff:
            case IntentType.DebuffStrong:
                foreach (var ally in allies)
                    await PowerCmd.Apply<ArtifactPower>(choiceContext, ally, 5, source, this, false);
                break;

            case IntentType.Summon:
            case IntentType.StatusCard:
                foreach (var ally in allies)
                    await PowerCmd.Apply<ThornsPower>(choiceContext, ally, 7, source, this, false);
                break;
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}