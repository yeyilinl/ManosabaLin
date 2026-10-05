using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
///     被缚的普罗米修斯（本回合）：你的火色消耗全部视为同色，不触发连续；每次消耗对该敌人造成伤害。
///     升级版把颜色锁定为打出时消耗的那一格，之后每次消耗都按这个颜色结算消耗效果。
///     回合结束时移除。
/// </summary>
[RegisterPower]
public sealed class BoundPrometheusPower : ManosabaPowerTemplate
{
    public const int DamagePerConsume = 6;

    /// <summary>非空时，本回合所有消耗都按这个颜色结算。</summary>
    public YalisalinFireColor? LockedColor { get; set; }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    ///     描述随「是否已锁色」切换：升级牌打出并消耗到火色后才锁色。
    ///     power 的悬浮说明在引擎里走的是 <c>Description</c>（<c>GetDumbHoverTip</c> 只读它，
    ///     UI 上的 power tooltip 也用它），所以必须覆写 <c>Description</c> 才能让描述跟着状态变。
    ///     做法对齐「共犯」（MeruruAndEmaAccomplicePower）。
    /// </summary>
    public override LocString Description =>
        new LocString("powers", LockedColor is null ? $"{Id.Entry}.description" : $"{Id.Entry}.descriptionLocked");

    /// <summary>
    ///     smart 通道（HoverTips）也按同一状态切键。该键缺失时引擎会自动回退到 <c>Description</c>，
    ///     所以这里即使不被用到也不会显示错内容。
    /// </summary>
    protected override string SmartDescriptionLocKey =>
        LockedColor is null ? $"{Id.Entry}.smartDescription" : $"{Id.Entry}.smartDescriptionLocked";

    public async Task OnFireColorConsumed(PlayerChoiceContext choiceContext, Creature target)
    {
        if (!target.IsAlive)
            return;

        Flash();
        await CreatureCmd.Damage(choiceContext, target, DamagePerConsume, ValueProp.Unpowered, Owner, null, null);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        await PowerCmd.Remove(this);
    }
}
