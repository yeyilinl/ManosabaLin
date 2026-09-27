namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
///     「爆裂魔法」一场只能打出一次的隐藏标记（战斗结束自动消失，不显示图标）。
/// </summary>
[RegisterPower]
public sealed class ExplosionMagicUsedPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;
}
