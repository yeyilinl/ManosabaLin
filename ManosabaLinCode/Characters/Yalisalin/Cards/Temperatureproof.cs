using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     温差证明：造成伤害（攻击本身会引爆 1 格火色），再给予 1 格火色，获得格挡；
///     这次伤害引爆的是浅橙时格挡翻倍。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Temperatureproof()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new BlockVar(5, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        var hasHairpin = YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin);
        var consumedBefore = hasHairpin ? hairpin.ConsumptionLog.Count : 0;

        await YalisalinFireColorCardHelpers.Attack(choiceContext, cardPlay, this, target, DynamicVars.Damage.BaseValue);

        // 攻击命中会自己从最新一格引爆 1 格火色；这次伤害引爆的是浅橙时，格挡翻倍。
        var consumedOrange = hasHairpin
                             && hairpin.ConsumptionLog
                                 .Skip(consumedBefore)
                                 .Any(static color => color == YalisalinFireColor.LightOrange);

        if (hasHairpin)
            await hairpin.GiveFireColor(choiceContext, target, 1, this);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (consumedOrange)
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
