using ManosabaLin.Characters.Common.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 宝生玛格的惑情：每当你生命值变化时（含回血、含悔恨那种扣血），有 10% 概率
/// 随机给予一个目标（全部敌人与全部友方，含自己）1 层再生或覆甲。不在手牌时生效。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class MargeCharm : LinAncientCurseCard
{
    // 测试挂载：原罪组件正式由其他卡牌 AddComponent 添加，此处仅用于测试。
    protected override IEnumerable<ICardComponent> CanonicalComponents => [new Originalsin()];

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("Chance", 10m); }
    }

    protected override async Task AfterCurrentHpChanged(
        Creature creature,
        decimal delta,
        ComponentContext componentContext)
    {
        if (Pile is { Type: PileType.Hand }) return;
        if (creature != Owner.Creature) return;
        if (delta == 0m) return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        if (rng.NextFloat() >= (float)(DynamicVars["Chance"].BaseValue / 100m)) return;

        if (Owner.Creature.CombatState is not { } combatState) return;
        var targets = combatState.Creatures.Where(c => c.IsAlive).ToList();
        if (targets.Count == 0) return;

        var target = rng.NextItem(targets);
        if (target is null) return;

        var ctx = new ThrowingPlayerChoiceContext();
        if (rng.NextFloat() < 0.5f)
            await PowerCmd.Apply<RegenPower>(ctx, target, 1m, Owner.Creature, this);
        else
            await PowerCmd.Apply<PlatingPower>(ctx, target, 1m, Owner.Creature, this);
    }
}
