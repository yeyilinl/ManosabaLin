using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 城崎诺亚的裹挟：抽到此卡时，加入一张"自控的颜料"（无法打出）诅咒卡进入抽牌堆或弃牌堆（随机），
/// 然后此卡进入弃牌堆或消耗堆（随机）。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class NoahEnsnare : LinAncientCurseCard
{
    protected override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, this)) return;
        if (CombatState is not { } combatState) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;

        var paint = combatState.CreateCard<SelfControlledPaint>(Owner);
        var paintPile = rng.NextFloat() < 0.5f ? PileType.Draw : PileType.Discard;
        await CardPileCmd.AddGeneratedCardToCombat(paint, paintPile, Owner, CardPilePosition.Random);

        var selfPile = rng.NextFloat() < 0.5f ? PileType.Discard : PileType.Exhaust;
        await CardPileCmd.Add(this, selfPile, CardPilePosition.Random);
    }
}
