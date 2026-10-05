using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Ema.Relics;
using ManosabaLin.Characters.Emalin.Components;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     海克斯联动遗物 2：审判具现。
///     <para>
///         <b>效果</b>：你持有本遗物期间，你的牌<b>获得附魔</b>时，附魔会<b>具现</b>进牌里 ——
///         不占用附魔槽（<c>card.Enchantment</c> 的真槽位恒为空 ⇒ 同一张牌可以再被附魔）；
///         牌面显示附魔名字、悬浮可查看效果、打出时一并触发。
///     </para>
///     <para>
///         ⭐ <b>本遗物的行为只在它自己的代码里实现</b>（2026-10-02 用户裁决）：
///         拦截 + 转换 + <c>card.Enchantment</c> 的全部适配都由遗物自己完成，<b>一处都不去改</b>
///         其它角色 / 卡牌 / 能力的代码。这套实现由四块组成：
///     </para>
///     <list type="number">
///         <item>
///             <b>拦截</b>：<c>Patches/HextechTrialEmbodimentPatch.cs</c> 打在 <c>CardCmd.Enchant</c>
///             前置，把附魔封进卡里的 <see cref="EnchantmentEmbodimentComponent" />（同名丢弃）。
///         </item>
///         <item>
///             <b>读取桥</b>：<c>Patches/HextechTrialEmbodimentBridge.cs</c> 打在
///             <c>CardModel.get_Enchantment</c> 上 —— 真槽位为空但卡里有具现附魔时交出第 1 条
///             ⇒ 引擎与**任何**角色 / 第三方模组的代码都无需改动，照样看得见这条附魔。
///         </item>
///         <item>
///             <b>补算第 2 条起</b>：<c>Patches/EnchantmentReadPatches.cs</c>（数值与卡面文字）
///             + 组件的 <c>OnPlayPostfix</c>（打出）/ <c>ModifyCardPlayCount</c>（打出次数）/
///             <c>HoverTips</c>（悬浮）。引擎只会结算单值的第 1 条，其余由遗物自己补。
///         </item>
///         <item>
///             <b>卡面</b>：藏掉引擎的附魔标签页、掐掉它的附魔额外文字，改为在卡面列出
///             <b>附魔名字</b>（<c>CardModel.GetDescriptionForPile</c> 补丁）。
///         </item>
///     </list>
///     <list type="bullet">
///         <item>收益：一张卡可以同时挂<b>任意多个不同名</b>的附魔 —— 绕开引擎「一卡一附魔」的单值槽限制。</item>
///         <item>
///             ⚠️ <b>同名附魔只能存在一个、不能堆叠</b>：与已有任一条同名的一律直接丢弃
///             （不新增、不累加层数）。
///         </item>
///     </list>
///     <para>
///         ⚠️ 拦截点：补丁打在 <c>CardCmd.Enchant(EnchantmentModel, CardModel, decimal)</c> 的
///         <b>前置</b>（泛型重载 <c>Enchant&lt;T&gt;</c> 内部转调它 ⇒ 只打一处）——
///         必须抢在引擎真的把附魔挂上卡之前接管。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechTrialEmbodiment : ManosabaRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>该玩家是否持有本遗物。</summary>
    internal static bool IsActiveFor(Player? player)
        => player?.Relics.OfType<HextechTrialEmbodiment>().Any() == true;

    /// <summary>取该玩家身上的本遗物（没有则 null）。</summary>
    public static HextechTrialEmbodiment? Find(Player? player)
        => player?.Relics.OfType<HextechTrialEmbodiment>().FirstOrDefault();

    // ── 审判计数（赞同 / 反驳 / 疑问）────────────────────────────────────────────────────
    //
    // ⭐ 2026-10-02 用户裁决：把「赞同 / 反驳 / 疑问」的**计数**放到本遗物里，并允许悬浮本遗物
    //   直接看到当前计数 —— 这样**别的卡 / 能力也能从本遗物读**到这三个数（`AgreementCount` /
    //   `RebuttalCount` / `DoubtCount`，或静态入口 `HextechTrialEmbodiment.Find(player)`）。
    //
    // ⚠️ 数值来源与艾玛的「审判徽章」（`EmaTrialBadge`）**完全同源**：本遗物的计数由
    //    `Patches/EmbodiedEnchantmentTrialCountPatches.cs` 挂在 `EmaTrialBadge.IncrementCount`
    //    的**后置**上同步（徽章每 +1、本遗物也 +1）⇒ 两者永不脱节，也不会因为「谁先谁后」而错位。
    private int _trialAgreement;
    private int _trialRebuttal;
    private int _trialDoubt;
    private int _lastTrialRound;

    /// <summary>本回合已打出的【赞同】审判牌数（对外只读）。</summary>
    public int AgreementCount => _trialAgreement;

    /// <summary>本回合已打出的【反驳】审判牌数（对外只读）。</summary>
    public int RebuttalCount => _trialRebuttal;

    /// <summary>本回合已打出的【疑问】审判牌数（对外只读）。</summary>
    public int DoubtCount => _trialDoubt;

    [SavedProperty]
    public int SavedAgreementCount
    {
        get => _trialAgreement;
        set { AssertMutable(); _trialAgreement = value; }
    }

    [SavedProperty]
    public int SavedRebuttalCount
    {
        get => _trialRebuttal;
        set { AssertMutable(); _trialRebuttal = value; }
    }

    [SavedProperty]
    public int SavedDoubtCount
    {
        get => _trialDoubt;
        set { AssertMutable(); _trialDoubt = value; }
    }

    [SavedProperty]
    public int LastTrialRound
    {
        get => _lastTrialRound;
        set { AssertMutable(); _lastTrialRound = value; }
    }

    /// <summary>
    ///     悬浮本遗物时显示当前三项审判计数（各一条，标题就是附魔自己的名字）。
    ///     <para>
    ///         ⚠️ 与卡面 / 附魔悬浮用的是**同一个** <c>HoverTip</c> 机制；标题复用附魔自己的名字
    ///         （<c>enchantments</c> 表的 <c>*.title</c>），只有「已打出 N 张」这句是本遗物新增的键。
    ///     </para>
    /// </summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return BuildTrialCountTip("AGREEMENT", _trialAgreement);
            yield return BuildTrialCountTip("REBUTTAL", _trialRebuttal);
            yield return BuildTrialCountTip("DOUBT", _trialDoubt);
        }
    }

    private static HoverTip BuildTrialCountTip(string entry, int count)
    {
        var description = new LocString("relics",
            "MANOSABA_LIN_RELIC_HEXTECH_TRIAL_EMBODIMENT.trialCount.description");
        description.Add("Count", count);
        return new HoverTip(
            new LocString("enchantments", $"MANOSABA_LIN_ENCHANTMENT_{entry}.title"),
            description);
    }

    /// <summary>由 <c>EmaTrialBadge.IncrementCount</c> 的后置补丁调用（见上述说明）。</summary>
    internal void AddTrialCount(int kind)
    {
        switch (kind)
        {
            case 0: _trialAgreement++; break;
            case 1: _trialRebuttal++; break;
            case 2: _trialDoubt++; break;
            default: return;
        }
    }

    /// <summary>把三项计数清零。</summary>
    internal void ResetTrialCounts()
    {
        _trialAgreement = 0;
        _trialRebuttal = 0;
        _trialDoubt = 0;
    }

    /// <summary>
    ///     回合开始：与审判徽章同一套规则（「回合号变了就清零」）。
    ///     <para>⚠️ 只在真正换回合时清，避免同一回合内反复触发把计数抹掉。</para>
    /// </summary>
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        var currentRound = player.Creature?.CombatState?.RoundNumber ?? LastTrialRound;
        if (currentRound == LastTrialRound) return Task.CompletedTask;

        LastTrialRound = currentRound;
        ResetTrialCounts();
        return Task.CompletedTask;
    }

    /// <summary>战斗结束：计数清零（与审判徽章一致）。</summary>
    public override Task AfterCombatEnd(CombatRoom room)
    {
        ResetTrialCounts();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     由补丁在 <c>CardCmd.Enchant</c> 之前调用。
    ///     <para>
    ///         返回 <c>true</c> = 这个附魔已被处理完（具现进卡里 / 同名丢弃），<b>不要再走原逻辑</b>；
    ///         返回 <c>false</c> = 交给原逻辑（该玩家没有本遗物 ⇒ 照常挂在卡上）。
    ///     </para>
    ///     <para>
    ///         ⚠️⭐ <paramref name="amount" /> <b>必须原样存进具现载荷</b>：引擎原逻辑是
    ///         <c>EnchantInternal(enchantment, amount)</c> → <c>ApplyInternal</c> → <c>Amount = (int)amount</c>，
    ///         而传进来的 <paramref name="enchantment" /> 是**原型**的 mutable 克隆（自身 <c>Amount</c> 恒为 0）。
    ///         漏掉它 ⇒ 所有具现附魔的 <c>Amount</c> 都是 0（<c>Sharp</c> 的伤害加成、三类审判附魔的计数全归零）。
    ///     </para>
    /// </summary>
    internal static bool TryEmbodify(EnchantmentModel enchantment, CardModel card, decimal amount,
        ref EnchantmentModel? applied)
    {
        // 只对持有本遗物的玩家的卡生效；没遗物 ⇒ 交回原逻辑（照常挂在卡上）。
        if (!IsActiveFor(card.Owner)) return false;

        // 与真槽位里的附魔同名（旧存档 / 未转换来源）⇒ 同名只能一个、不堆叠 ⇒ 静默丢弃。
        // ⚠️ 用 Raw（真槽位）而不是 card.Enchantment —— 后者已经被读取桥补成「第 1 条具现附魔」，
        //    而卡内部那几条由 Embodify / HasEnchantment 自己判重，不需要在这里再看一遍。
        if (EffectiveEnchantments.Raw(card)?.Id?.Entry == enchantment.Id?.Entry)
        {
            applied = null;
            return true;
        }

        // 其余附魔（含「第一条」）⇒ 全部封进卡里那张「只带附魔的卡」（不占卡牌的附魔槽）。
        if (card is IComponentsCardModel components
            && components.GetComponent<EnchantmentEmbodimentComponent>() is { } existing)
        {
            applied = existing.Embodify(enchantment, card, amount) ? enchantment : null;
            return true;
        }

        var component = new EnchantmentEmbodimentComponent(enchantment, card, amount);
        var attached = card.TryAddComponent(component);
        if (attached is null)
        {
            applied = null;
            return true;
        }

        // ⚠️ 立刻让「卡本体改动」（附魔的 OnEnchant：加关键字 / 改费用）生效 ——
        //    与引擎「附完魔立刻 ModifyCard()」（CardCmd.cs:546）同一时机，
        //    否则要等到下一次有人读 card.Enchantment / 刷卡面时才补上。
        (attached as EnchantmentEmbodimentComponent)?.GetEmbodiedEnchantments();

        applied = enchantment;
        return true;
    }
}
