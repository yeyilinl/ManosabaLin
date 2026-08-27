using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 远野汉娜的狂想：每当你打出 1 张牌（含自动打出）时，有 10% 概率
/// 获得 20 金币或失去 10 金币（各 50%）；金币不足 10 时失去 10 会变为 0。不在手牌时生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class Hannadelusion : LinAncientCurseCard
{
    protected override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Pile is { Type: PileType.Hand }) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        if (rng.NextFloat() >= 0.1f) return;

        if (rng.NextFloat() < 0.5f)
            await PlayerCmd.GainGold(20, Owner);
        else
            await PlayerCmd.LoseGold(System.Math.Min(10, Owner.Gold), Owner);
    }
}
