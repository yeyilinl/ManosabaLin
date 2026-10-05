using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.HiroKeywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ManosabaLin.Characters.Hiro.Cards;

[RegisterCard(typeof(HirolinCardPool))]
public sealed class CardSeventyFive : ManosabaCardTemplate
{
    public CardSeventyFive() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [TransmigrationRules.TransmigrationCardKeyword];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new BlockVar(1m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;
        var target = cardPlay.Target;

        ArgumentNullException.ThrowIfNull(target);

        await CreatureCmd.TriggerAnim(source.Owner.Creature, "Cast", source.Owner.Character.CastAnimDelay);

        // 造成伤害
        await DamageCmd.Attack(source.DynamicVars.Damage.BaseValue)
            .FromCard(source, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 卡面：「当前【抽牌堆】每有一张【轮回】卡获得 N 点格挡」
        var rebirthInDraw = PileType.Draw.GetPile(source.Owner).Cards
            .Count(TransmigrationRules.HasTransmigration);

        if (rebirthInDraw > 0)
        {
            await CreatureCmd.GainBlock(
                source.Owner.Creature,
                source.DynamicVars.Block.BaseValue * rebirthInDraw,
                ValueProp.Move,
                cardPlay);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级保留伤害升级，同时每张轮回卡获得的格挡 1 → 2。
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(1m);
    }
}
