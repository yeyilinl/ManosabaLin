using System.Threading.Tasks;
using ManosabaLin.Characters.Common.AncientCurses.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 橘雪莉的空洞：你造成的低于 13 的伤害减半（向下取整），按每次伤害实例判定；
/// 若打出的卡产生了被减半的伤害，则获得 1 层临时力量。手牌中才生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class SherryVoid : LinAncientCurseCard
{
    protected override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player,
        ComponentContext componentContext)
    {
        if (player != Owner) return;
        if (Owner.Creature.GetPower<SherryVoidPower>() is null)
            await PowerCmd.Apply<SherryVoidPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Pile?.Type != PileType.Hand) return;
        if (Owner.Creature.GetPower<SherryVoidPower>() is not { } power) return;
        if (!power.ConsumeHalvedPlay()) return;

        await PowerCmd.Apply<TemporaryStrengthPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
