using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     临界擦边：给予目标 2 格火色并获得能量；未升级时若目标因此满格，消耗 1 格火色；
///     升级后改为「消耗 1 格火色然后再给予 1 格火色」。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Grazingcritical()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var target = cardPlay.Target;
        ArgumentNullException.ThrowIfNull(target);

        if (!YalisalinFireColorSystem.TryGetHairpin(Owner, out var hairpin))
            return;

        await hairpin.GiveFireColor(choiceContext, target, 2, this);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

        if (IsUpgraded)
        {
            await hairpin.ConsumeFireColor(choiceContext, target, 1, this);
            await hairpin.GiveFireColor(choiceContext, target, 1, this);
            return;
        }

        // 未升级：只有「给予后正好满格」才把多出来的那格烧掉。
        if (hairpin.IsFireColorFull(target))
            await hairpin.ConsumeFireColor(choiceContext, target, 1, this);
    }
}
