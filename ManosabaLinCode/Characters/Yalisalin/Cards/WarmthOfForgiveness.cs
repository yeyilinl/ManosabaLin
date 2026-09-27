using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     被原谅的体温（2 费 技能・稀有）：
///     回复 4 点生命，本回合每有 1 次「宽恕」额外回复 1 点生命。
///     升级后费用降为 1 费。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class WarmthOfForgiveness()
    : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Heal", 4)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        var extra = YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin)
            ? hairpin.SinForgiveThisTurn
            : 0;

        await CreatureCmd.Heal(owner.Creature, DynamicVars["Heal"].BaseValue + extra);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
