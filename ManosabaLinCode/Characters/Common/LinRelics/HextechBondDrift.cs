using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     艾玛联动遗物 3：羁绊漂移。
///     <para>
///         <b>表现形式</b>：人物模型按「疏远 − 亲近」的累计偏移左右移动 —— <b>累计、长期停留</b>
///         （用户 2026-10-01 裁定：不自动回位）。
///     </para>
///     <list type="bullet">
///         <item>亲近<b>增加</b> ⇒ 向左移（偏移变小）；亲近<b>减少</b> ⇒ 向右移。</item>
///         <item>疏远<b>增加</b> ⇒ 向右移（偏移变大）；疏远<b>减少</b> ⇒ 向左移。</item>
///         <item>每次变化事件<b>只触发一次</b>效果（亲近 +2 也只算一次左移事件）。</item>
///     </list>
///     <para>
///         <b>左移时</b>：你的所有队友（不含自己）各自获得一张其角色卡池的【多人】卡。
///         <b>右移时</b>：从所有队友的手牌 / 抽牌堆 / 弃牌堆里随机移除一张【多人】卡；
///         若成功移除，其主人获得 1 层「下一张多人卡费用 −1」（可叠加，一旦打出多人卡就整层清空）。
///     </para>
///     <para>
///         ⚠️ <b>位移载体是 <c>NCreature.Position</c>（父级逻辑位置），不是 <c>Visuals.Position</c></b>：
///         后者会被引擎的受击抖动（<c>AnimShake</c> 把 <c>Visuals.Position</c> 重置为 0 再跑 tween）清掉，
///         做不到「长期停留」。代价是 <c>NCombatRoom.PositionPlayersAndPets</c> 每次重排都会把位置重置，
///         所以额外打了一个后置补丁，在重排<b>之后</b>把累计偏移叠回去（见 <c>HextechBondDriftPatches</c>）。
///     </para>
///     <para>
///         ⚠️ 变化来源是 <see cref="BondPower" /> 的两个 <c>[SavedProperty]</c> 上（它们的 setter
///         在<b>减少</b>时不会调用 <c>Yalisabond.ApplyBondDeltaAsync</c> ⇒ 靠那个钩子根本收不到「减少」），
///         所以用补丁观测 setter，自己按「上次值 vs 新值」算 delta（联机两端都会跑，结果一致）。
///         数值基线（<c>LastAffinity</c> / <c>LastEstrangement</c>）**跟着遗物一起存档** ⇒
///         读档恢复 <c>BondPower</c> 时算出的 delta 为 0，不会凭空触发一次移动效果。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechBondDrift : ManosabaRelicTemplate
{
    /// <summary>
    ///     每 1 点羁绊偏移对应的水平像素。
    ///     <para>
    ///         ⚠️ 2026-10-02 实测反馈「移动太不明显，只看得出动了一点」⇒ 由 12f 调到 40f。
    ///         （12f 时 1 点变化只有 12px，在 1920 宽的屏幕上几乎看不出来。）
    ///     </para>
    /// </summary>
    internal const float StepPerPoint = 40f;

    /// <summary>上次观察到的亲近值（随遗物存档 ⇒ 读档后基线正确）。</summary>
    [SavedProperty] public int LastAffinity { get; set; }

    /// <summary>上次观察到的疏远值。</summary>
    [SavedProperty] public int LastEstrangement { get; set; }

    /// <summary>亲近一侧的基线是否已建立（首次观察不触发效果，只对齐基线）。</summary>
    [SavedProperty] public bool AffinityBaselineSet { get; set; }

    /// <summary>疏远一侧的基线是否已建立。</summary>
    [SavedProperty] public bool EstrangementBaselineSet { get; set; }

    public override RelicRarity Rarity => RelicRarity.Starter;

    internal static HextechBondDrift? Find(Player? player)
        => player?.Relics.OfType<HextechBondDrift>().FirstOrDefault();

    internal static bool IsActiveFor(Player? player) => Find(player) is not null;

    /// <summary>
    ///     诊断日志（排查「位移不动」用）。稳定后可整段移除。
    /// </summary>
    internal static void Diag(string message)
    {
        try
        {
            MainFile.Logger.Info("[BondDrift] " + message);
        }
        catch (System.Exception)
        {
            // 日志失败不影响玩法。
        }
    }

    /// <summary>战斗开始时把两侧基线对齐到当前值（幂等；BondPower 尚未建立时留待首次观察）。</summary>
    public override Task BeforeCombatStart()
    {
        var bond = Owner?.Creature.GetPower<BondPower>();
        Diag($"BeforeCombatStart owner={(Owner is null ? "null" : "ok")} bond={(bond is null ? "null" : $"{bond.Affinity}/{bond.Estrangement}")}");

        if (bond is not null)
        {
            LastAffinity = bond.Affinity;
            LastEstrangement = bond.Estrangement;
            AffinityBaselineSet = true;
            EstrangementBaselineSet = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     亲近 / 疏远的值发生了一次变化（由补丁在 setter 之后调用）。
    ///     <paramref name="isAffinity" /> 为 <c>true</c> 表示这次变的是亲近，否则是疏远。
    /// </summary>
    internal static void OnBondValueChanged(BondPower bond, bool isAffinity)
    {
        if (bond.Owner?.Player is not { } player)
        {
            Diag($"OnBondValueChanged affinity={isAffinity}: bond.Owner/Player 为空 ⇒ 退出");
            return;
        }

        var relic = Find(player);
        if (relic is null)
        {
            Diag($"OnBondValueChanged affinity={isAffinity}: 该玩家没持有遗物 ⇒ 退出");
            return;
        }

        var delta = isAffinity
            ? bond.Affinity - relic.LastAffinity
            : bond.Estrangement - relic.LastEstrangement;

        var baselineSet = isAffinity ? relic.AffinityBaselineSet : relic.EstrangementBaselineSet;
        if (isAffinity)
        {
            relic.LastAffinity = bond.Affinity;
            relic.AffinityBaselineSet = true;
        }
        else
        {
            relic.LastEstrangement = bond.Estrangement;
            relic.EstrangementBaselineSet = true;
        }

        Diag($"OnBondValueChanged affinity={isAffinity} A={bond.Affinity} E={bond.Estrangement} delta={delta} baselineSet={baselineSet}");

        // 本场战斗第一次观察到这个数值 ⇒ 只对齐基线，不算作「一次变化」。
        if (!baselineSet) return;
        if (delta == 0) return;

        // 偏移 = 疏远 − 亲近：
        //   亲近 +delta ⇒ 偏移 −delta（向左）；疏远 +delta ⇒ 偏移 +delta（向右）。
        relic.HandleOffsetDelta(isAffinity ? -delta : delta);
    }

    /// <summary>
    ///     偏移变化了一次：先做模型位移，再按方向触发一次效果（左 +1 / 右 −1）。
    /// </summary>
    private void HandleOffsetDelta(int offsetDelta)
    {
        Diag($"HandleOffsetDelta offsetDelta={offsetDelta} ownerNull={Owner is null} creatureNull={Owner?.Creature is null} combatStateNull={Owner?.Creature?.CombatState is null}");
        if (offsetDelta == 0) return;
        if (Owner is not { } player) return;
        if (player.Creature is not { } self) return;
        if (self.CombatState is null) return;

        MoveBy(self, offsetDelta);

        // ⚠️ setter 是同步的，这里没法 await ⇒ 与 BondPower 自己的
        // `TaskHelper.RunSafely(yalisabond.ApplyBondDeltaAsync(...))` 同口径。
        TaskHelper.RunSafely(offsetDelta < 0
            ? HextechBondDriftEffects.OnShiftedLeft(player)
            : HextechBondDriftEffects.OnShiftedRight(player));
    }

    /// <summary>把模型按 <paramref name="offsetDelta" /> 点做一次增量位移（增量为 0 时不动）。</summary>
    internal static void MoveBy(Creature creature, int offsetDelta)
    {
        if (offsetDelta == 0) return;

        var room = NCombatRoom.Instance;
        var node = room?.GetCreatureNode(creature);
        Diag($"MoveBy delta={offsetDelta} roomNull={room is null} nodeNull={node is null}"
             + (node is null ? "" : $" X {node.Position.X:F0} -> {node.Position.X + offsetDelta * StepPerPoint:F0}"));
        if (node is null) return;

        node.Position = new Vector2(node.Position.X + offsetDelta * StepPerPoint, node.Position.Y);
    }

    /// <summary>
    ///     把「累计偏移」重新叠到当前站位上（绝对值，基于刚被重排好的基础位置）。
    ///     由 <c>NCombatRoom.PositionPlayersAndPets</c> 的后置补丁调用。
    /// </summary>
    internal static void ReapplyOffset(Player player)
    {
        if (!IsActiveFor(player)) return;

        var bond = player.Creature.GetPower<BondPower>();
        if (bond is null) return;

        var offset = bond.Estrangement - bond.Affinity;
        if (offset == 0) return;

        var node = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
        Diag($"ReapplyOffset offset={offset} nodeNull={node is null}"
             + (node is null ? "" : $" X {node.Position.X:F0} -> {node.Position.X + offset * StepPerPoint:F0}"));
        if (node is null) return;

        node.Position = new Vector2(node.Position.X + offset * StepPerPoint, node.Position.Y);
    }
}
