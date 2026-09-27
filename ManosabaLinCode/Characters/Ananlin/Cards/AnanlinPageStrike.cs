using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Ananlin.Cards;

/// <summary>
///     书页打击 - 1 费攻击，多人专属。
///     <para>造成 13 点伤害，并给予所有人（含自己）一张「借来的留白书页」。</para>
///     <para>队友打出的那份书页用的是<b>本卡使用者（夏目安安）当时记录的卡池</b>，而不是持有者自己的素描本。</para>
/// </summary>
[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinPageStrike : ManosabaCardTemplate
{
    private const string PagesKey = "Pages";

    public AnanlinPageStrike() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13m, ValueProp.Move),
        new IntVar(PagesKey, 1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 发出书页时把「安安当前记录的卡池」快照进书页里
        var poolEntries = this.Sketchbook()?.RecordedPoolEntries ?? [];
        var pages = DynamicVars[PagesKey].IntValue;

        foreach (var player in CombatState.Players
                     .Where(p => p.Creature.Side == Owner.Creature.Side && p.Creature.IsAlive)
                     .ToList())
        {
            for (var i = 0; i < pages; i++)
            {
                var page = CombatState.CreateCard<BorrowedMarginPage>(player);
                page.SetRecordedPools(poolEntries);
                await CardPileCmd.AddGeneratedCardToCombat(page, PileType.Hand, player);
            }
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars[PagesKey].UpgradeValueBy(1m);
    }
}
