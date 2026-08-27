namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 莲见蕾雅的痴狂：你的攻击伤害或获得的格挡有 10% 概率落空（不造成伤害、不获得格挡），
/// 按每次伤害实例 / 每次获得格挡判定。落空时该段的抽牌/附带效果也不生效。不在手牌时生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class RaiyaMadness : LinAncientCurseCard
{
    protected override decimal ModifyDamageMultiplicativeC(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner?.Creature) return 1m;
        if (Pile is { Type: PileType.Hand }) return 1m;
        return ShouldWhiff() ? 0m : 1m;
    }

    protected override decimal ModifyBlockMultiplicativeC(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner?.Creature) return 1m;
        if (Pile is { Type: PileType.Hand }) return 1m;
        return ShouldWhiff() ? 0m : 1m;
    }

    private bool ShouldWhiff()
    {
        if (Owner?.RunState.Rng.CombatCardGeneration is not { } rng) return false;
        return rng.NextFloat() < 0.1f;
    }
}
