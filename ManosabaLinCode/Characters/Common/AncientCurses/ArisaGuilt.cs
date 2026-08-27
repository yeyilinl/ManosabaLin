using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 紫藤亚里沙的负疚：每当你造成伤害时（每次伤害实例），先判断再获得：
/// 若你已有【紫藤亚里沙的魔法】，则给予伤害目标 1 层；否则自己获得 1 层。手牌中才生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class ArisaGuilt : LinAncientCurseCard
{
    protected override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource,
        ComponentContext componentContext)
    {
        if (Pile?.Type != PileType.Hand) return;
        if (dealer != Owner.Creature) return;
        if (result.TotalDamage <= 0) return;

        if (Owner.Creature.HasPower<YlsmPower>())
            await PowerCmd.Apply<YlsmPower>(choiceContext, target, 1m, Owner.Creature, this);
        else
            await PowerCmd.Apply<YlsmPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
