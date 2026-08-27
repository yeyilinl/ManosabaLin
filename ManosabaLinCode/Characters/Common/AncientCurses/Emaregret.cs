using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 樱羽艾玛的悔恨：每当你打出 1 张卡（含自动打出）时失去 1 点生命；
/// 若你的格挡大于 13，则改为失去 1 点格挡。手牌中才生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class Emaregret : LinAncientCurseCard
{
    protected override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Pile?.Type != PileType.Hand) return;

        var creature = Owner.Creature;
        if (creature.IsDead) return;

        if (creature.Block > 13)
        {
            await CreatureCmd.LoseBlock(choiceContext, creature, 1m, creature);
        }
        else
        {
            await CreatureCmd.Damage(
                choiceContext,
                creature,
                1m,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this,
                null);
        }
    }
}
