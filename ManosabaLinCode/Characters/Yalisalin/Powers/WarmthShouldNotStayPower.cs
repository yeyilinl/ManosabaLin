using ManosabaLin.Characters.Yalisalin.Components;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 不该留下的温柔（独立能力）：每次余火选择完成时，随机让 1 张没有余火的牌获得余火。
/// 存在即生效（层数无意义，仅作为开关）。
/// </summary>
[RegisterPower]
public sealed class WarmthShouldNotStayPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 效果说明已作为 enhancement.warmthShouldNotStay 追加到余火组件悬浮提示，不在状态栏显示。
    protected override bool IsVisibleInternal => false;

    /// <summary>
    /// 由遗物的余火「解析完成后」钩子转发调用。
    /// </summary>
    public void ResolveAfterComponent(Player owner)
    {
        YalisalinFireComponentRules.TryAddFireComponent(
            YalisalinFireComponentRules.RandomCardWithoutFireComponent(owner));
    }
}