namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

/// <summary>
/// 本场战斗的原罪「宽恕」+「自惩」总次数计数器（隐藏）。每次任意原罪触发宽恕或自惩 +1。
/// 能力是战斗内模型，随战斗结束自动清除，保证「本场」= 当前战斗节点（罪业圣盾依赖）。
/// </summary>
[RegisterPower]
public sealed class OriginalsinResolveCounterPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 隐藏计数，不在角色状态栏显示层数。
    protected override bool IsVisibleInternal => false;
}
