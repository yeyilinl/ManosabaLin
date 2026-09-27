using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     痛觉的重量（2 费 能力・稀有）：
///     每当你通过余火烧掉 2 张牌，本场获得 1 点力量；升级后改为每烧掉 1 张牌获得 1 点力量。
///     阈值写在能力上（普通 2 / 升级 1），重复打出时取更低的阈值。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class WeightOfPain()
    : ManosabaCardTemplate(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        await PowerCmd.Apply<WeightOfPainPower>(
            choiceContext, owner.Creature, 1m, owner.Creature, this, false);

        owner.Creature.GetPower<WeightOfPainPower>()?.ConfigureThreshold(IsUpgraded ? 1 : 2);
    }
}
