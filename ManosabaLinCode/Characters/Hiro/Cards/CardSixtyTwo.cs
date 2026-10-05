using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.HiroKeywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ManosabaLin.Characters.Hiro.Cards;

[RegisterCard(typeof(HirolinCardPool))]
public class CardSixtyTwo() : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

    public override bool GainsBlock => true;

    // 固定基础伤害，不再用 IsUpgraded 判断
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DamageVar(4m, ValueProp.Move)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [TransmigrationRules.TransmigrationCardKeyword];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 卡面：「造成 N 点伤害并获得等量格挡」。
        var dealt = attack.Results.SelectMany(static hit => hit).Sum(static r => r.TotalDamage);
        if (dealt > 0m)
            await CreatureCmd.GainBlock(Owner.Creature, dealt, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：伤害 +3
        DynamicVars.Damage.BaseValue += 3; // 4 → 7
    }
}
