using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 没被采用的结论（独立能力）：一回合一次，当火色实际被消耗且与上一次消耗的颜色不同时，
/// 封存本次消耗的颜色并抽1张。存在即生效（层数保留为开关，逻辑与 per-turn 标记归本 Power）。
/// </summary>
[RegisterPower]
public sealed class MixedConclusionPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _usedThisTurn;

    public bool IsUsedThisTurn => _usedThisTurn;

    public void MarkUsed()
    {
        _usedThisTurn = true;
    }

    /// <summary>由遗物在回合开始时调用，重置 per-turn 标记。</summary>
    public void ResetUsedThisTurn()
    {
        _usedThisTurn = false;
    }
}