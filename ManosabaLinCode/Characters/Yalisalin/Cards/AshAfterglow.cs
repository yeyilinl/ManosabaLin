using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     灰烬的余温（1 费 技能・罕见）：
///     本回合每有 1 张牌被余火烧掉，获得 4 点格挡；升级后每张改为 5 点。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class AshAfterglow()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        var burns = YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin)
            ? hairpin.FireComponentBurnsThisTurn
            : 0;

        if (burns <= 0)
            return;

        await CreatureCmd.GainBlock(
            owner.Creature, DynamicVars.Block.BaseValue * burns, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(1);
    }
}
