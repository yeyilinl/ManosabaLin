using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 把道歉烧成灰（独立能力）：余火烧掉牌时，若该牌可以打出，先自动打出1次，然后再烧掉。
/// 存在即生效（层数无意义，仅作为开关）。
/// </summary>
[RegisterPower]
public sealed class BurnedApologyPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 效果说明已作为 enhancement.burnedApology 追加到余火组件悬浮提示，不在状态栏显示。
    protected override bool IsVisibleInternal => false;
}