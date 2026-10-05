using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common.Components.Abstracts;
using ManosabaLin.Characters.Common.LinRelics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Emalin.Components;

/// <summary>
///     【附魔具现】组件：由艾玛联动遗物「审判具现」（<c>HextechTrialEmbodiment</c>）把卡牌上的
///     附魔<b>全部</b>封存进卡里。
///     <para>
///         形态照「三周目的决定」（<c>EmaBadEndingRewardComponent</c>）与 <c>GenerateComponent</c>：
///         <b>一张卡里面装着另一张卡</b> —— 内嵌一份 <see cref="SerializableCard" />，附魔挂在它身上；
///         这张内嵌卡<b>不执行任何卡面效果</b>，只是附魔的载体（所以它「没有效果」）。
///     </para>
///     <para>
///         <b>2026-10-02 用户裁决改为「全部转换」</b>（此前是「第 1 个留在卡上作代表附魔」）：
///         现在卡牌的附魔槽（<c>card.Enchantment</c> 的<b>真槽位</b>）<b>恒为空</b> ——
///         这正是「<b>不占用附魔槽 / 不使用引擎的附魔显示框</b>」的前提。
///     </para>
///     <para>
///         ⭐ 但空槽不能让全游戏都看不见附魔：读取桥
///         （<c>Patches/HextechTrialEmbodimentBridge.cs</c> 打在 <c>CardModel.get_Enchantment</c> 上）
///         会把卡内<b>第 1 条</b>具现附魔如实交出去（见 <see cref="GetBridgeEnchantment" />），
///         于是引擎与任何角色的代码都不用改；第 2 条起由遗物自己的补丁补算。
///         卡面则改为由本组件贡献<b>附魔名字</b>（<c>CardModel.GetDescriptionForPile</c> 补丁）+
///         <b>附魔效果悬浮框</b>（<see cref="HoverTips" />），引擎的附魔标签页被藏掉。
///     </para>
///     <para>
///         卡牌打出时逐个还原内嵌卡的附魔、原样跑一遍它的 <c>OnPlay</c>（同一份代码 ⇒ 效果不走样），
///         但<b>不占卡牌的附魔槽</b> —— 所以卡上可以同时挂任意多个不同名的附魔。
///     </para>
///     <para>
///         ⚠️ <b>同名附魔只能存在一个、不能堆叠</b>：<see cref="TryMergeWith" /> 里与已有条目
///         <c>Id.Entry</c> 相同的直接丢弃（不新增、不累加层数），只有不同名的附魔才各自保留一条。
///     </para>
/// </summary>
public sealed partial class EnchantmentEmbodimentComponent : KeywordLikeComponent
{
    private static readonly IReadOnlyList<EnchantmentModel> EmptyEmbodied = [];

    private List<IHoverTip>? _hoverTips;

    /// <summary>
    ///     <see cref="GetEmbodiedEnchantments" /> 正在构建时的重入标记。
    ///     <para>
    ///         ⚠️ <b>必须有</b>：构建末尾会跑 <see cref="ApplyCardBodyChanges" /> 改卡本体（加关键字 / 改费用），
    ///         这些改动会经 <c>KeywordsChanged</c> / 卡面刷新**再读回 <c>card.Enchantment</c>** ⇒ 又进本方法。
    ///         不挡就是无限递归（<c>HashSet.Add</c> 即便已存在也会无条件触发 <c>KeywordsChanged</c>）。
    ///     </para>
    /// </summary>
    private bool _building;

    /// <summary>
    ///     还原后的附魔实例缓存 —— <b>每一条都已持久绑定本卡</b>（<c>ApplyInternal</c>，之后不再解绑）。
    ///     <para>
    ///         ⚠️ 必须有缓存：读取器会被挂在**每次伤害/格挡计算**的热点路径上
    ///         （<c>Hook.ModifyDamage</c> 等），每次都 <c>FromSerializable</c> 重建模型会明显掉帧。
    ///         只在 <see cref="SavedCards" /> 变化时失效重建。
    ///     </para>
    ///     <para>
    ///         ⚠️⭐ <b>为什么是「持久绑定」而不是「用时临时绑定」</b>：引擎里附魔的钩子能力来自
    ///         <c>ShouldReceiveCombatHooks =&gt; Card?.ShouldReceiveCombatHooks</c>，而原版相当一部分附魔的
    ///         效果<b>是靠战斗钩子实现的</b>（<c>Goopy</c>/<c>Glam</c>/<c>Vigorous</c> 的 <c>AfterCardPlayed</c>、
    ///         <c>Slither</c> 的 <c>AfterCardDrawn</c>、<c>SlumberingEssence</c> 的 <c>BeforeFlush</c>、
    ///         <c>Imbued</c> 的 <c>AfterAutoPrePlayPhaseEntered</c> …）。
    ///         绑上卡之后由 <see cref="EmbodiedEnchantmentHooks" /> 把这批实例登记成
    ///         <c>CombatState</c> 的钩子监听者 ⇒ 它们收到的东西与「还挂在卡上」完全一致。
    ///     </para>
    /// </summary>
    private List<EnchantmentModel>? _embodied;

    /// <summary>
    ///     <see cref="_embodied" /> 对应的 <see cref="SavedCards" /> <b>内容指纹</b>。
    ///     <para>
    ///         ⚠️⭐ <b>必须有</b>：MinionLib 的 <c>[ComponentState]</c> 会在状态同步 / 存档恢复 / 组件挂载时
    ///         <b>反复调用</b> <see cref="SavedCards" /> 的 setter —— 若每次都作废实例缓存就会反复重建；
    ///         而重建要跑 <c>EnchantmentModel.ApplyInternal</c>（内部有一句 <c>card.AssertMutable()</c>），
    ///         一旦撞上卡的<b>不可变窗口</b>（战斗结算 / 渲染预览 / 打出中）就会整批静默失败
    ///         ⇒ 读取桥交不出附魔 ⇒「<b>数值加成不生效、悬浮提示为空</b>」，
    ///         并且 <c>card.Enchantment</c> 每次返回不同引用 ⇒ 卡面 <c>NCard</c> 的
    ///         「退订比对引用」失配、池化复用时抛 <c>InvalidOperationException</c>。
    ///         按<b>内容</b>比对后，只有真的变了才重建。
    ///     </para>
    /// </summary>
    private string? _embodiedKey;

    /// <summary>
    ///     载荷改动计数 —— <b>所有</b>改动点（<see cref="SavedCards" /> 的 setter、
    ///     <see cref="Embodify" /> / <see cref="TryMergeWith" /> / <see cref="ClearAll" />）都会递增它。
    ///     <para>
    ///         ⚠️⭐ 存在的意义是给读取路径一条<b>零分配</b>的最快路径：<see cref="GetEmbodiedEnchantments" />
    ///         挂在 <c>Hook.ModifyDamage/ModifyBlock</c>（每次伤害计算）与 6 个
    ///         <c>DynamicVar.UpdateCardPreview</c>（每帧 × 每张手牌）上 ——
    ///         若每次调用都去算一遍内容指纹字符串，就是每条伤害 / 每一帧都在做无谓分配。
    ///     </para>
    /// </summary>
    private int _revision;

    /// <summary><see cref="_revision" /> 在**上一次成功构建**时的取值。</summary>
    private int _builtRevision = -1;

    /// <summary>
    ///     上一轮构建里有没有「没能绑上卡」的实例（撞上卡的不可变窗口）。
    ///     <para>
    ///         降级结果<b>依然可用</b>：<c>FromSerializable</c> 已经恢复了 <c>Amount</c> ⇒ 数值加成与
    ///         悬浮提示照常；只有「需要 <c>Card</c> 非空」的那部分（战斗钩子、<c>OnPlay</c>、改卡本体）暂时不行。
    ///         标记为降级 ⇒ 下次读取时若卡已恢复可变会自动重建补齐。
    ///     </para>
    /// </summary>
    private bool _degraded;

    /// <summary>一次会话里只记一次的诊断（避免热路径刷屏）。</summary>
    private static readonly HashSet<string> LoggedOnce = [];

    /// <summary>
    ///     <see cref="_embodied" /> 中<b>第 1 条</b>对应的 <see cref="SavedCards" /> 下标（-1 = 无）。
    ///     <para>读取桥交出去的就是这一条 ⇒ <c>Amount</c> 写回时要落回同一条。</para>
    /// </summary>
    private int _primarySavedIndex = -1;

    /// <summary>
    ///     卡里内嵌的「另一张卡」——每张只带一条附魔、不执行任何卡面效果（同名仅一条，不堆叠）。
    /// </summary>
    [ComponentState]
    private List<SerializableCard> SavedCards
    {
        get;
        set
        {
            field = value;
            InvalidateCaches();
        }
    }

    public EnchantmentEmbodimentComponent()
    {
        SavedCards = [];
    }

    /// <param name="amount">
    ///     ⚠️⭐ 引擎 <c>CardCmd.Enchant(enchantment, card, amount)</c> 里的**层数**。
    ///     必须原样存进载体卡 —— 传进来的 <paramref name="enchantment" /> 是原型克隆（<c>Amount</c> 恒为 0），
    ///     漏掉它就会让每条具现附魔都变成「0 层」：
    ///       · <c>Sharp</c>（锋利）的伤害加成 = <c>Amount</c> ⇒ 附 +2 变成 +0；
    ///       · <c>Doubt</c>/<c>Agreement</c>/<c>Rebuttal</c> 的「已打出 N 张」悬浮数字全为 0；
    ///       · <c>Adroit</c> 的格挡值同步失效。
    /// </param>
    public EnchantmentEmbodimentComponent(EnchantmentModel enchantment, CardModel carrier, decimal amount)
    {
        SavedCards = [MakeCarrier(enchantment, carrier, amount)];
    }

    /// <summary>把一条附魔包成「卡里的另一张卡」：卡面用载体自己的 Id，附魔挂在它的附魔字段上。</summary>
    private static SerializableCard MakeCarrier(EnchantmentModel enchantment, CardModel carrier, decimal amount)
    {
        var serialized = enchantment.ToSerializable();

        // ⚠️ 引擎语义：`ApplyInternal(card, amount)` 里 `Amount = (int)amount`。
        //    这里等价地把层数写进序列化载荷（构建实例时会用 `serialized.Amount` 调 ApplyInternal）。
        serialized.Amount = (int)amount;

        return new SerializableCard
        {
            Id = carrier.Id,
            Enchantment = serialized
        };
    }

    /// <summary>这条附魔是否已经被具现过（按附魔 Id 判定，用于「同名只能一个」）。</summary>
    public bool HasEnchantment(EnchantmentModel enchantment)
    {
        var entry = enchantment.Id?.Entry;
        return SavedCards.Any(c => c.Enchantment?.Id?.Entry == entry);
    }

    /// <summary>
    ///     已具现的附魔条目（**已绑定本卡**、并已登记为战斗钩子监听者；结果带缓存）。
    /// </summary>
    public IReadOnlyList<EnchantmentModel> GetEmbodiedEnchantments()
    {
        // ⭐ 最快路径：载荷**从未改动过**（所有改动点都会递增 _revision）⇒ 直接复用，连指纹都不算。
        //    这里是 `Hook.ModifyDamage` / 卡面刷新的热路径 ⇒ 刻意保持**零分配**。
        if (_embodied is not null && !_degraded && _builtRevision == _revision) return _embodied;

        var key = ComputeKey(SavedCards);

        // 次快路径：revision 变了但**内容指纹相同**（例如 [ComponentState] 把同一份载荷整体恢复了一次），
        // 或者处于降级态而卡此刻仍不可变（继续用降级结果：数值 / 悬浮已可用，只是钩子 / OnPlay 待补）。
        if (_embodied is not null && _embodiedKey == key)
        {
            if (!_degraded || Card is null || !CanMutate(Card))
            {
                _builtRevision = _revision;
                return _embodied;
            }
        }

        // ⚠️ 重入保护（见 _building 的说明）：构建期重入一律拿现有结果，别递归。
        if (_building) return _embodied is { } reentrant ? reentrant : EmptyEmbodied;

        _building = true;
        try
        {
            // 重建前先把旧实例从钩子登记处摘掉（内容已变 / 强制重建）。
            EmbodiedEnchantmentHooks.Untrack(_embodied);

            var result = new List<EnchantmentModel>();
            var primaryIndex = -1;
            var degraded = false;

            var card = Card;

            if (card is not null)
            {
                for (var i = 0; i < SavedCards.Count; i++)
                {
                    if (SavedCards[i].Enchantment is not { } serialized) continue;

                    EnchantmentModel enchantment;
                    try
                    {
                        // ⚠️⭐ `FromSerializable` 已经把载荷里的 Amount 恢复了
                        //    （`save.Props?.Fill(...)` 之后就是 `enchantmentModel.Amount = save.Amount`）
                        //    ⇒ 即使下面绑卡失败，**数值加成（如 Sharp 的 +X）与悬浮提示仍然是正确的**。
                        enchantment = EnchantmentModel.FromSerializable(serialized);
                    }
                    catch (System.Exception ex)
                    {
                        // 该条附魔的模型缺失（例如对方模组被卸载）⇒ 只能跳过这一条。
                        LogOnce($"skip {serialized.Id?.Entry}: {ex.GetType().Name} {ex.Message}");
                        continue;
                    }

                    try
                    {
                        // ⚠️ 先绑卡、再登记：ApplyInternal 自己会写一次 Amount，
                        //    此时还没 MarkEmbodied ⇒ 不会触发 WriteBackAmount（避免构建期重入）。
                        // ⚠️⭐ 它内部有一句 `card.AssertMutable()`：撞上卡的不可变窗口会抛 ——
                        //    这里**降级保留实例**（不丢整条），否则读取桥会交出 null、附魔效果全部静默失效。
                        enchantment.ApplyInternal(card, serialized.Amount);
                    }
                    catch (System.Exception ex)
                    {
                        degraded = true;
                        LogOnce($"degrade {serialized.Id?.Entry}: {ex.GetType().Name} {ex.Message}");
                    }

                    EffectiveEnchantments.MarkEmbodied(enchantment);
                    if (primaryIndex < 0) primaryIndex = i;
                    result.Add(enchantment);
                }
            }

            // ⚠️ 顺序要紧：**先**把缓存与钩子登记就位，**再**改卡本体 ——
            //    改卡本体会同步触发卡面刷新，刷新里又会读 card.Enchantment / 本组件，
            //    若此时 _embodied 仍为 null 就会再构建一次（递归）。
            // ⚠️⭐ 空表**不缓存**：撞上不可变窗口时整批失败，缓存空表就等于**永久失效**
            //    （卡面没名字、读取桥交出 null ⇒ 附魔效果全不触发）。空表仍走重建，下次自动重试。
            _primarySavedIndex = primaryIndex;
            _embodiedKey = key;
            _builtRevision = _revision;
            _degraded = degraded;
            _embodied = result.Count > 0 ? result : null;

            EmbodiedEnchantmentHooks.Track(_embodied);

            // ⚠️ 降级态**不**改卡本体：ModifyCard() 在未绑卡的实例上会抛，
            //    而且此刻卡的不可变窗口本来也不允许改 —— 等下一次成功绑定后再补。
            if (card is not null && _embodied is { } built && !degraded) ApplyCardBodyChanges(card, built);

            LogOnce($"build {key} -> {result.Count} 条{(degraded ? "（降级：未绑卡）" : string.Empty)}");

            return _embodied is { } cached ? cached : EmptyEmbodied;
        }
        finally
        {
            _building = false;
        }
    }

    /// <summary>
    ///     <see cref="SavedCards" /> 的<b>内容指纹</b> —— <b>只含每条附魔的 Id</b>，刻意不含 <c>Amount</c>。
    ///     <para>
    ///         ⚠️⭐ <b>为什么不含 Amount</b>：层数变化（<c>WriteBackAmount</c> / <c>SyncTrialAmounts</c>）
    ///         只改序列化载荷，<b>不</b>代表要换实例 —— 若把它算进指纹，每次「引擎写一次 <c>Amount</c>」
    ///         都会导致下一次读取重建实例，于是 <c>card.Enchantment</c> 频繁变成新引用，
    ///         卡面 <c>NCard</c>「退订靠引用相等」的判定失配 ⇒ 池化复用时抛
    ///         <c>InvalidOperationException</c>（实测日志里刷屏的那条）。
    ///         层数的同步由 <c>WriteBackAmount</c> / <c>SyncTrialAmounts</c> 直接写回实例负责，二者本就一致。
    ///     </para>
    /// </summary>
    private static string ComputeKey(List<SerializableCard> cards)
    {
        if (cards.Count == 0) return string.Empty;

        var parts = new List<string>(cards.Count);
        foreach (var card in cards)
            parts.Add(card.Enchantment?.Id?.Entry ?? "-");

        return string.Join(";", parts);
    }

    /// <summary>这张卡此刻是否可改（<c>EnchantmentModel.ApplyInternal</c> 会要求）。</summary>
    private static bool CanMutate(CardModel card)
    {
        try
        {
            card.AssertMutable();
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    /// <summary>诊断：同一句只记一次（构建可能发生在每次伤害计算的热路径上）。</summary>
    private static void LogOnce(string message)
    {
        lock (LoggedOnce)
        {
            if (LoggedOnce.Count > 40) return;
            if (!LoggedOnce.Add(message)) return;
        }

        try
        {
            MainFile.Logger.Info($"[Embodiment] {message}");
        }
        catch (System.Exception)
        {
            // 日志失败无所谓。
        }
    }

    /// <summary>
    ///     把每条具现附魔的「<b>永久改卡本体</b>」操作补到卡上 —— 对应 <c>EnchantmentModel.ModifyCard()</c>
    ///     （<c>OnEnchant()</c> + <c>RecalculateValues()</c> + 卡自身 DynamicVars 重算）。
    ///     <para>
    ///         引擎在「<b>刚附魔</b>」（<c>CardCmd.cs:546</c>）与「<b>反序列化</b>」（<c>CardModel.FromSerializable</c>）
    ///         后各调一次 —— 但附魔具现进卡里之后，引擎**永远不会**再对它们调 <c>ModifyCard()</c>
    ///         （它们不在真槽位上、<c>card.Enchantment</c> 的真槽位恒空）⇒ 原版那些靠 <c>OnEnchant()</c>
    ///         改卡本体的附魔会静默失效。实测原版至少：
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <c>Goopy</c> / <c>Steady</c> / <c>RoyallyApproved</c> / <c>TezcatarasEmber</c> / <c>SoulsPower</c>
    ///             —— <c>AddKeyword / RemoveKeyword</c>（Exhaust / Retain / Innate / Eternal）。
    ///         </item>
    ///         <item>
    ///             <c>TezcatarasEmber</c> / <c>MockFreeEnchantment</c> ——
    ///             <c>EnergyCost.UpgradeBy(-当前费用)</c>（把费用降到 0）。
    ///         </item>
    ///         <item>
    ///             <c>Adroit</c> —— <c>RecalculateValues()</c> 把 <c>DynamicVars.Block.BaseValue</c>
    ///             同步成 <c>Amount</c>（它的 <c>OnPlay</c> 要用）。
    ///         </item>
    ///     </list>
    ///     <para>
    ///         ⚠️ <b>幂等</b>：本方法在每次重建实例时都会跑，但原版这些 <c>OnEnchant</c> 全是幂等的 ——
    ///         <c>AddKeyword</c> 是集合操作、降费那句是 <c>UpgradeBy(-当前费用)</c>（已为 0 则减 0）
    ///         ⇒ 重复执行不会叠加。<b>这正是引擎的语义</b>：引擎在每次反序列化后也会重跑一遍。
    ///     </para>
    ///     <para>
    ///         ⚠️ 与遗物既有结算**互不干扰**：数值加成走 <c>EnchantmentReadPatches</c> 的 Hook 注入、
    ///         打出走组件 <see cref="OnPlayPostfix" />、打出次数走 <see cref="ModifyCardPlayCount" />，
    ///         本方法只管「卡本体」（关键字 / 费用 / 卡自身的 DynamicVars）。
    ///     </para>
    /// </summary>
    private static void ApplyCardBodyChanges(CardModel card, List<EnchantmentModel> embodied)
    {
        var changed = false;

        foreach (var enchantment in embodied)
        {
            try
            {
                enchantment.ModifyCard();
                changed = true;
            }
            catch (System.Exception)
            {
                // 单条失败（模型缺失 / 卡此刻不可变）⇒ 跳过，别牵连其它条。
            }
        }

        if (!changed) return;

        // 引擎在 ModifyCard() 之后也会接这一句（`CardCmd.cs:556`）—— 收尾「刚升级」的显示状态。
        try
        {
            card.FinalizeUpgradeInternal();
        }
        catch (System.Exception)
        {
            // 纯表现收尾，失败无碍。
        }
    }

    /// <summary>
    ///     卡被降级：<c>CardModel.DowngradeInternal</c> 会把关键字与费用重置回原型
    ///     （<c>_keywords = 原型</c>、<c>EnergyCost.ResetForDowngrade()</c>）⇒
    ///     由附魔 <c>OnEnchant()</c> 加上的关键字（Exhaust / Retain / Innate / Eternal）与降费会**一起被抹掉**。
    ///     <para>
    ///         引擎自己会在这之后补一句 <c>Enchantment?.ModifyCard()</c>，但附魔已具现、真槽位为空
    ///         （读取桥在这一步被刻意关掉）⇒ 必须由本组件把缓存作废，让下一次读取重跑
    ///         <see cref="ApplyCardBodyChanges" /> 补回来。
    ///     </para>
    /// </summary>
    public override void AfterDowngraded(ComponentContext componentContext)
    {
        _hoverTips = null;

        // ⚠️ 这里必须**强制**重建：降级会把关键字 / 费用重置回原型，而这些要靠
        //    ApplyCardBodyChanges（补跑附魔的 OnEnchant）写回来 —— 载荷内容其实没变，
        //    不把这两个"已构建"标记清掉就不会重建，也就补不回来
        //   （⚠️ `_builtRevision` 必须一起清：最快路径只比 revision，光清 key 会被它提前 return 掉）。
        _embodiedKey = null;
        _builtRevision = -1;
        _degraded = false;
    }

    /// <summary>
    ///     卡内附魔一旦有变，缓存作废（悬浮框）。
    ///     <para>
    ///         ⚠️⭐ <b>刻意不在这里清 <see cref="_embodied" /> / <see cref="_primarySavedIndex" /></b>：
    ///         MinionLib 的 <c>[ComponentState]</c> 恢复会<b>反复</b>调用 <see cref="SavedCards" /> 的 setter，
    ///         而内容往往<b>没变</b> —— 清掉就是白白重建，重建还可能撞上卡的不可变窗口而静默失败
    ///         （见 <see cref="_embodiedKey" /> 的说明）。实例缓存的失效统一交给
    ///         <see cref="GetEmbodiedEnchantments" /> 按<b>内容指纹</b>判定（重建时自身会 Untrack 旧实例）。
    ///     </para>
    /// </summary>
    private void InvalidateCaches()
    {
        _revision++;
        _hoverTips = null;
    }

    /// <summary>
    ///     把「对桥接实例 <c>Amount</c> 的写入」落回 <see cref="SavedCards" />。
    ///     <para>
    ///         ⚠️ <b>必须有</b>：读取桥把具现附魔交给 <c>card.Enchantment</c>，
    ///         而引擎 / 卡牌会直接写它的 <c>Amount</c>（例如 <c>EmaTrialBadge.SyncCountersToEnchantments</c>
    ///         的「赞同 / 反驳 / 疑问」计数回写、原版 <c>Goopy</c> 的 <c>Amount++</c>）——
    ///         不写回的话这些修改会**读完就丢**。
    ///     </para>
    ///     <para>
    ///         ⚠️ 这里**刻意不 <see cref="InvalidateCaches" />**：重建实例会丢掉实例上的运行时状态
    ///         （<c>Glam</c> 的 <c>_usedThisCombat</c>、<c>Vigorous</c> 的 <c>Status = Disabled</c> …），
    ///         那等于「一改层数就把附魔重置一次」。
    ///     </para>
    /// </summary>
    internal void WriteBackAmount(EnchantmentModel enchantment, int amount)
    {
        if (_embodied is not { Count: > 0 } embodied) return;
        if (!ReferenceEquals(embodied[0], enchantment)) return;
        if (_primarySavedIndex < 0 || _primarySavedIndex >= SavedCards.Count) return;
        if (SavedCards[_primarySavedIndex].Enchantment is not { } serialized) return;
        if (serialized.Amount == amount) return;

        serialized.Amount = amount;
    }

    /// <summary>
    ///     取「引擎应当看到的那一条」——<see cref="SavedCards" /> 里<b>第 1 条</b>具现附魔（已绑回本卡）。
    ///     <para>
    ///         由读取桥（<c>Patches/HextechTrialEmbodimentBridge.cs</c>）在
    ///         <c>CardModel.get_Enchantment</c> 上调用 —— 这样全游戏任何读 <c>card.Enchantment</c>
    ///         的地方（引擎自己 / 任何角色 / 任何第三方模组）都会把具现附魔当成还挂在卡上，
    ///         <b>不需要逐个去改读取方</b>。
    ///     </para>
    ///     <para>
    ///         只给第 1 条：属性是<b>单值</b>的，引擎的数值 / 打出 / 打出次数 / 悬浮都只读一次；
    ///         第 2 条起由遗物自己的补丁补算（<c>EnchantmentReadPatches</c> +
    ///         本组件的 <see cref="ModifyCardPlayCount" /> / <see cref="OnPlayPostfix" />）。
    ///     </para>
    /// </summary>
    internal EnchantmentModel? GetBridgeEnchantment()
    {
        var embodied = GetEmbodiedEnchantments();
        return embodied.Count > 0 ? embodied[0] : null;
    }

    /// <summary>把一条附魔封进卡里；同名已存在则丢弃（不新增、不堆叠）。返回是否真的写入。</summary>
    /// <param name="amount">引擎 <c>CardCmd.Enchant</c> 传进来的层数（见构造函数的说明）。</param>
    public bool Embodify(EnchantmentModel enchantment, CardModel carrier, decimal amount)
    {
        if (HasEnchantment(enchantment)) return false;

        SavedCards.Add(MakeCarrier(enchantment, carrier, amount));
        InvalidateCaches();

        // ⚠️ 立刻构建一次：把**新附魔的卡本体改动**（<c>OnEnchant</c>）当场落到卡上 ——
        //    时机与引擎「附完魔立刻 ModifyCard()」一致（见 ApplyCardBodyChanges）。
        //    不这么做的话，改动要等到下一次有人读 card.Enchantment / 刷卡面时才生效。
        GetEmbodiedEnchantments();
        return true;
    }

    /// <summary>
    ///     清空卡内所有具现附魔 —— 对应 <c>CardCmd.ClearEnchantment(card)</c> 对「卡上那条」的作用。
    ///     （附魔具现后，光清卡上的槽是清不掉任何东西的。）
    /// </summary>
    public void ClearAll()
    {
        if (SavedCards.Count == 0) return;

        SavedCards.Clear();
        InvalidateCaches();
    }

    /// <summary>
    ///     把卡内「赞同 / 反驳 / 疑问」三类具现附魔的 <c>Amount</c> 同步为给定计数。
    ///     <para>
    ///         对应 <c>EmaTrialBadge.SyncCountersToEnchantments</c> 的旧行为（原来直接改
    ///         <c>card.Enchantment.Amount</c>）。附魔具现后那份真身在 <see cref="SavedCards" /> 里
    ///         ⇒ 必须改**序列化条目**本身，光改 <see cref="GetEmbodiedEnchantments" /> 返回的实例是写不回去的。
    ///     </para>
    /// </summary>
    public void SyncTrialAmounts(int agreement, int rebuttal, int doubt)
    {
        var targets = new Dictionary<string, int>
        {
            [ModelDb.Enchantment<Agreement>().Id.Entry] = agreement,
            [ModelDb.Enchantment<Rebuttal>().Id.Entry] = rebuttal,
            [ModelDb.Enchantment<Doubt>().Id.Entry] = doubt
        };

        var changed = false;
        foreach (var saved in SavedCards)
        {
            if (saved.Enchantment is not { } serialized) continue;
            if (serialized.Id?.Entry is not { } entry) continue;
            if (!targets.TryGetValue(entry, out var amount)) continue;
            if (serialized.Amount == amount) continue;

            serialized.Amount = amount;
            changed = true;

            // ⚠️ 同步改已构建实例，而不是让它 InvalidateCaches 重建
            //   —— 重建会顺手丢掉附魔的运行时状态（见 WriteBackAmount 的说明）。
            if (_embodied?.FirstOrDefault(e => e.Id?.Entry == entry) is { } instance)
                instance.Amount = amount;
        }

        if (changed) _hoverTips = null;
    }

    public override bool TryMergeWith(ICardComponent incoming, ApplyComponentOptions options,
        out ICardComponent? merged)
    {
        if (incoming is not EnchantmentEmbodimentComponent other)
        {
            merged = null;
            return false;
        }

        // 同名附魔只能存在一个、不能堆叠：同名直接丢弃，不同名才追加。
        foreach (var card in other.SavedCards)
        {
            var entry = card.Enchantment?.Id?.Entry;
            if (SavedCards.Any(c => c.Enchantment?.Id?.Entry == entry))
                continue;
            SavedCards.Add(card);
        }

        InvalidateCaches();
        merged = this;
        return true;
    }

    /// <summary>
    ///     卡牌悬浮时补上卡内具现附魔的<b>效果悬浮框</b>（<b>全部</b>具现附魔，按具现顺序）。
    ///     <para>
    ///         ⚠️ 取的是附魔的 <c>HoverTips</c>（= 本体 <c>HoverTip</c> + <c>ExtraHoverTips</c>）而不是
    ///         只有 <c>HoverTip</c>：原版相当一部分附魔把关键信息放在 <c>ExtraHoverTips</c> 里
    ///         （<c>Goopy</c> 的 Exhaust、<c>Glam</c>/<c>Spiral</c> 的 Replay 次数、<c>Inky</c> 的 Weak、
    ///         <c>Steady</c> 的 Retain、<c>TezcatarasEmber</c> 的 Eternal …）⇒ 只取 <c>HoverTip</c> 会漏掉。
    ///     </para>
    ///     <para>
    ///         ⚠️⭐ <b>为什么列「全部」而不是「第 2 条起」</b>（2026-10-02 第三次修正）：
    ///         之前依赖「第 1 条由引擎 <c>CardModel.HoverTips</c> 里的 <c>Enchantment.HoverTips</c> 给出」，
    ///         而那条路径要经过<b>读取桥</b>（<c>card.Enchantment</c>）——桥一旦交不出附魔
    ///         （组件构建撞上卡的不可变窗口 / 桥的抑制深度失衡），<b>第 1 条的悬浮就整个消失</b>，
    ///         而卡面名字却照常显示（卡面名字补丁是<b>直读组件</b>的）⇒ 出现「看得到附魔名、
    ///         悬浮没有效果说明」的怪象（用户实测反馈）。
    ///         <b>本组件在这里直读组件、不经过桥</b>，所以只要卡里还有附魔，悬浮就一定给得出。
    ///     </para>
    ///     <para>
    ///         ⚠️ <b>不会与引擎那条重复</b>：<c>HoverTip.Id</c> 由「本地化表 + 键」拼成（全库唯一且稳定），
    ///         悬浮面板渲染前会跑 <c>IHoverTip.RemoveDupes</c>（<c>NHoverTipSet.cs:258</c>）按 <c>Id</c> 去重 ⇒
    ///         引擎那条（第 1 条）与本组件这条 <c>Id</c> 相同，会被自动合并成一条。
    ///     </para>
    ///     <para>
    ///         也**没有**「【附魔具现】」这类标题性说明（2026-10-02 用户裁决）：
    ///         用户要看到的就是「转换为组件的附魔效果悬浮提示」，所以这里只列附魔自己的效果。
    ///     </para>
    /// </summary>
    public override IEnumerable<IHoverTip> HoverTips
    {
        get
        {
            // ⚠️ 空表**不缓存**：构建可能恰好撞上卡的不可变窗口 ⇒ 拿到空结果；
            //    缓存它就等于「悬浮提示永久消失」（与 _embodied 空表不缓存同一个道理）。
            if (_hoverTips is { Count: > 0 } cached) return cached;

            var tips = BuildHoverTips();
            if (tips.Count > 0) _hoverTips = tips;
            return tips;
        }
    }

    private List<IHoverTip> BuildHoverTips()
    {
        var tips = new List<IHoverTip>();

        // ⚠️ 直读组件（不经过读取桥）：列**全部**具现附魔的效果，见上方说明。
        foreach (var enchantment in GetEmbodiedEnchantments())
        {
            try
            {
                tips.AddRange(enchantment.HoverTips);
            }
            catch (System.Exception)
            {
                // 附魔模型缺失（例如对方模组被卸载）时静默跳过，不要让悬浮提示崩掉。
            }
        }

        return tips;
    }

    /// <summary>
    ///     复刻附魔的 <c>EnchantPlayCount</c>（例如【反驳】的「每第 5 次反驳多打一次」）。
    ///     <para>
    ///         ⚠️ 附魔被具现进卡里后，<c>EnchantmentModel.EnchantPlayCount</c> 这条钩子随附魔槽一起
    ///         消失（它不是引擎的 <c>AbstractModel</c> 钩子，没人会自己来问），必须在组件侧补回，
    ///         否则「同等效果」缺一条。实例已持久绑定本卡，直接读 <c>Card.Owner</c> 即可。
    ///     </para>
    ///     <para>
    ///         ⚠️⭐ <b>补「全部」具现附魔</b>（2026-10-02 第三次修正）：引擎的
    ///         <c>GetEnchantedReplayCount</c> 只看得见单值的 <c>card.Enchantment</c>（= 读取桥的第 1 条），
    ///         桥一旦交不出附魔，第 1 条的 <c>EnchantPlayCount</c> 也会丢 ⇒ 改为由本组件统一补全部，
    ///         并由 <c>EmbodiedEnchantmentReplayPatch</c> 把引擎那半边（经桥的第 1 条）掐掉，避免重复。
    ///     </para>
    /// </summary>
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (Card != card) return playCount;

        var result = playCount;

        foreach (var enchantment in GetEmbodiedEnchantments())
            result = enchantment.EnchantPlayCount(result);

        return result;
    }

    /// <summary>
    ///     打出时，把卡里每张「另一张卡」的附魔<b>各跑一遍它的 <c>OnPlay</c></b> ——
    ///     「同等效果」由同一份代码保证。
    ///     <para>
    ///         ⚠️ 实例已持久绑定本卡（<see cref="GetEmbodiedEnchantments" />），这里**不再**临时
    ///         <c>ApplyInternal</c>/<c>ClearInternal</c> —— 否则会把钩子登记用的绑定拆掉。
    ///     </para>
    ///     <para>
    ///         ⚠️⭐ <b>为什么是「全部」而不是「第 2 条起」</b>（2026-10-02 第三次修正）：
    ///         原来依赖「第 1 条由引擎 <c>CardModel</c> 那句 <c>Enchantment.OnPlay</c> 结算」，
    ///         而那句要经过<b>读取桥</b>。桥交不出附魔时（组件构建撞上卡的不可变窗口 / 桥的抑制深度失衡），
    ///         <c>card.Enchantment</c> 为 null ⇒ 引擎整段跳过 ⇒ <b>第 1 条的附魔效果永远不触发</b>
    ///         （用户实测：锋利这类走数值接管路径的附魔正常，而「赞同 / 反驳 / 疑问」这类
    ///         <b>靠 OnPlay 实现</b>的附魔完全没用 —— 正是这个原因）。
    ///         ⇒ 改为这里直读组件、统一跑全部；引擎那半边由
    ///         <c>Patches/EmbodiedEnchantmentOnPlayPatch.cs</c> 对具现实例跳过（恰好一次）。
    ///     </para>
    /// </summary>
    public override async Task OnPlayPostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Card is null) return;

        foreach (var enchantment in GetEmbodiedEnchantments())
        {
            try
            {
                await enchantment.OnPlay(choiceContext, cardPlay);
            }
            catch (System.Exception)
            {
                // 单条失败（模型缺失 / 卡此刻不可变）⇒ 跳过，别牵连其它条，更别让整张牌打不出来。
            }
        }
    }
}
