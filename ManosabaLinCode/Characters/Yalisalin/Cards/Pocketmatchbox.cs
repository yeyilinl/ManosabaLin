using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Pocketmatchbox()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new BlockVar(4, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        // 选择1格火色进行封存
        if (await YalisalinFireColorSegmentPicker.Pick(Owner, target, SelectionScreenPrompt) is not { } sealedSegment
            || !hairpin.TrySealFireColorSegment(target, sealedSegment))
            return;

        if (!hairpin.TryGetLastConsumedFireColorThisTurn(out var last) || last != sealedSegment.Color)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this, strong: true);
        }
        else
        {
            await YalisalinFireColorCardHelpers.ApplyHeat(choiceContext, Owner, target, this, strong: false);
        }

        if (IsUpgraded)
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }
}
