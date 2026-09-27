using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     魔女之力：需要 100 层【魔女化】才能打出，获得 1 层【魔女仪式】、2 层无实体与 1 层【劫难】
///     （<see cref="CalamityPower" />，原版 calamity 能力）。
/// </summary>
/// <remarks>
///     与希罗那张 <c>HiroWith</c>（同名「魔女之力」）是对应卡：规格、可打出判定都照它，
///     只把【乱战】(<c>MayhemPower</c>) 换成【劫难】(<see cref="CalamityPower" />)。
/// </remarks>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class YalisalinWitchForce : ManosabaCardTemplate
{
    private const int RequiredWithAmount = 100;

    public YalisalinWitchForce() : base(4, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<RitualCeremonyPower>(1m),
        new PowerVar<IntangiblePower>(2m),
        new PowerVar<CalamityPower>(1m),
        new PowerVar<WithPower>(100m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<WithPower>();
            yield return HoverTipFactory.FromPower<RitualCeremonyPower>();
            yield return HoverTipFactory.FromPower<IntangiblePower>();
            yield return HoverTipFactory.FromPower<CalamityPower>();
        }
    }

    /// <summary>场上【魔女化】不足 100 层时不可打出。</summary>
    protected override bool IsPlayableC
    {
        get
        {
            if (!base.IsPlayableC)
                return false;

            var withPower = Owner.Creature.GetPower<WithPower>();
            var withAmount = withPower?.Amount ?? 0;

            return withAmount >= RequiredWithAmount;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;

        await CreatureCmd.TriggerAnim(source.Owner.Creature, "Cast", source.Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<RitualCeremonyPower>(
            choiceContext, source.Owner.Creature,
            source.DynamicVars["RitualCeremonyPower"].BaseValue,
            source.Owner.Creature,
            source,
            false
        );

        await PowerCmd.Apply<IntangiblePower>(
            choiceContext, source.Owner.Creature,
            source.DynamicVars["IntangiblePower"].BaseValue,
            source.Owner.Creature,
            source,
            false
        );

        await PowerCmd.Apply<CalamityPower>(
            choiceContext, source.Owner.Creature,
            source.DynamicVars["CalamityPower"].BaseValue,
            source.Owner.Creature,
            source,
            false
        );
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
