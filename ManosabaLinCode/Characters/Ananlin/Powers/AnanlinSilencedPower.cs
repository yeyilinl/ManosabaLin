using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Ananlin.Powers;

/// <summary>
/// 被缄默：标记。敌人在其回合开始时自行移除。
/// 层数（Amount）描述缄默命中次数；MagicInfluenceRemaining 描述"已被魔法影响"次数，
/// 触发二次篡改归零（卡面显示"已被魔法影响0次"）。层数不归零，避免因0层自动移除。
/// </summary>
[RegisterPower]
public sealed class AnanlinSilencedPower : ManosabaPowerTemplate
{
    [SavedProperty] public int MagicInfluenceRemaining { get; set; } = 1;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add(new IntVar("MagicInfluence", MagicInfluenceRemaining));
            return description;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("MagicInfluence", 1)
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}