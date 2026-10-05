using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Ananlin.Powers;

[RegisterPower]
public sealed class AnanlinSilentAmplificationPower : ManosabaPowerTemplate
{
    private const int DefaultRewritesPerAmplification = 3;

    [SavedProperty] public int PendingRewrites { get; set; }
    [SavedProperty] public int RewritesPerAmplification { get; set; } = DefaultRewritesPerAmplification;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    ///     描述随「改写阈值是否已被升级牌降到 2」切键（升级也是一种达成条件）。数值本身由
    ///     <c>{Rewrites}</c> 动态变量给出，这里只按状态切键。两侧通道都切，做法对齐「共犯」/
    ///     被缚的普罗米修斯。
    /// </summary>
    public override LocString Description
    {
        get
        {
            var description = new LocString("powers",
                RewritesPerAmplification < DefaultRewritesPerAmplification
                    ? $"{Id.Entry}.descriptionEnhanced"
                    : $"{Id.Entry}.description");
            description.Add(new IntVar("Rewrites", RewritesPerAmplification));
            return description;
        }
    }

    protected override string SmartDescriptionLocKey =>
        RewritesPerAmplification < DefaultRewritesPerAmplification
            ? $"{Id.Entry}.smartDescriptionEnhanced"
            : $"{Id.Entry}.smartDescription";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Rewrites", DefaultRewritesPerAmplification)
    ];

    internal int ReplacementValueMultiplier => Math.Max(1, Amount);

    internal void SetRewritesPerAmplification(int value)
    {
        RewritesPerAmplification = Math.Max(1, value);
    }

    internal void RecordRewrites(int count)
    {
        if (count <= 0) return;

        PendingRewrites += count;
        var gainedMultipliers = PendingRewrites / RewritesPerAmplification;
        if (gainedMultipliers <= 0) return;

        PendingRewrites %= RewritesPerAmplification;
        SetAmount(ReplacementValueMultiplier + gainedMultipliers);
        Flash();
    }
}
