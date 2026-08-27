using System.Threading.Tasks;
using ManosabaLin.Characters.Common.AncientCurses.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 冰上梅露露的怯懦：每当你获得格挡时减少 2 点；若获得前你的格挡大于 15 则额外减少 1 点。
/// 药剂、遗物、能力给的格挡都算"获得格挡"。手牌中才生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class MeruruCowardice : LinAncientCurseCard
{
    protected override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player,
        ComponentContext componentContext)
    {
        if (player != Owner) return;
        if (Owner.Creature.GetPower<MeruruCowardicePower>() is null)
            await PowerCmd.Apply<MeruruCowardicePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
