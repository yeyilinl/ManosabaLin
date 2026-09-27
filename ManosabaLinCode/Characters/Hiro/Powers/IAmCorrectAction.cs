using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Hiro.Powers;

/// <summary>
///     我才是正确 - 可点击的能力（Action），<b>不可叠加的 single 能力，不使用层数</b>。
///     <para>
///         每回合可以<b>点击任意次数</b>此能力图标，进入目标选择模式，把「被伪证」给予任意玩家（含自己）。
///     </para>
///     <para>
///         不限次数靠「不给未覆写 <see cref="DecrementAfterAct" /> / 不扣层数」实现：
///         本能力不可叠加，没有层数可扣；而且引擎在层数降到 0 时会把能力整个移除
///         （<c>PowerModel.ShouldRemoveDueToAmount()</c> 在 <c>!AllowNegative &amp;&amp; Amount &lt;= 0</c> 时返回 true），
///         用 <see cref="DecrementAfterAct" /> 会直接用一次就整张能力消失。
///         引擎侧 <c>ActionModel.CanAct</c> 只要求 <c>Amount &gt; 0</c>（施加时给 1 层即可），
///         <c>CreatureActionQueueThreshold.IsExhausted</c> 也只限制「同时排队数」（执行完即释放），
///         因此不自行加回合内限次就等于「每回合可点无数次」。
///     </para>
/// </summary>
[RegisterPower]
public sealed class IAmCorrectAction : ManosabaActionTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target is null || !target.IsAlive) return;

        await PowerCmd.Apply<FalselyAccusedPower>(choiceContext, target, 1m, Owner, null, false);
    }
}
