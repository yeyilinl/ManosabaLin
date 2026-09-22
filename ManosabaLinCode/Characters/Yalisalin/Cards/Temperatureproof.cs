using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Temperatureproof()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new EnergyVar(1),
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        await YalisalinFireColorCardHelpers.Attack(choiceContext, cardPlay, this, target, DynamicVars.Damage.BaseValue);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        // 选择1格火色进行封存
        YalisalinFireColor? sealedColor = null;
        if (await YalisalinFireColorSegmentPicker.Pick(Owner, target, SelectionScreenPrompt) is { } sealedSegment
            && hairpin.TrySealFireColorSegment(target, sealedSegment))
        {
            sealedColor = sealedSegment.Color;
            await YalisalinSealedFirePower.Sync(choiceContext, Owner, this);
        }

        var consumed = await hairpin.ConsumeFireColorDetailed(choiceContext, target, 1, this);
        if (sealedColor != null
            && consumed.Consumed.Count > 0
            && sealedColor.Value != consumed.Consumed.Last().Color)
        {
            hairpin.TryAddFireColor(target, 1, this);
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        if (IsUpgraded)
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }
}
