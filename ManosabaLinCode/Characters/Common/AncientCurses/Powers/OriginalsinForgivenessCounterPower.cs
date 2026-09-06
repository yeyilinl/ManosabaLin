namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

/// <summary>
/// 本场战斗的原罪宽恕次数计数器（隐藏）。每次任意原罪触发「宽恕」+1。
/// </summary>
[RegisterPower]
public sealed class OriginalsinForgivenessCounterPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 隐藏计数，不在角色状态栏显示层数。
    protected override bool IsVisibleInternal => false;
}