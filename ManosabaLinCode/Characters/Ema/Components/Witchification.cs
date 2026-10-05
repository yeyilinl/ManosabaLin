using ManosabaLin.Characters.Common.Components.Abstracts;
using ManosabaLin.Characters.Common.LinRelics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Component.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Emalin.Components;

public sealed partial class Witchification : KeywordLikeComponent
{
    /// <summary>
    ///     本卡累积的「魔女化计数」。
    ///     由「保留 → 下回合开始 +1」累积（见 <see cref="AfterPlayerTurnStartEarlyPostfix" />）。
    ///     <para>
    ///         ⚠️ 刻意**不加** <c>[ComponentState]</c>（与 <c>RetainCounterComponent._counter</c> 一致）：
    ///         加进去会改变组件的二进制序列化布局，破坏新旧版本之间的联机/存档兼容。
    ///         因此该计数**不跨存档持久化**，只在当前战斗内有效。
    ///     </para>
    ///     <para>
    ///         <b>玩家侧**主动**增加计数时</b>，每 +1 会触发一次联动遗物 3：<b>+10 层【魔女化】</b>
    ///         （遗物不在场时内部直接返回）。
    ///         ⚠️ 10-01 起遗物 3 只剩这一条「+1 计数 ⇒ +10 层」链，旧版「削超量层数 → 按 25:1 折算写入计数」
    ///         那条路径已删除（<c>grantWitchification: false</c> 保留仅为兼容，当前无调用方）。
    ///     </para>
    /// </summary>
    public int WitchificationCount { get; private set; }

    /// <summary>
    ///     卡面用悬浮提示（本地化键 <c>ManosabaLin.Witchification.hovertip.title / .description</c>）。
    ///     给「挂载该组件的卡」在 <c>AdditionalHoverTips</c> 里引用。
    /// </summary>
    public static IHoverTip[] Tip => GetHoverTip<Witchification>();

    /// <summary>
    ///     增加「魔女化计数」。
    ///     <para>
    ///         <paramref name="grantWitchification" /> = <c>true</c>（默认）时，每 +1 会触发联动遗物 3：
    ///         <b>+10 层【魔女化】</b>（遗物不在场时内部直接返回）。
    ///     </para>
    ///     <para>
    ///         ⚠️ <paramref name="grantWitchification" /> = <c>false</c> 时**只累加计数，不再发放【魔女化】层数**。
    ///         这是给遗物 3 自己在「回合开始按 25:1 折算写入计数」时用的：
    ///         那条路径如果再回调一次，就会变成「削层 → 加计数 → 又涨回去」的自我反馈，
    ///         用户 2026-09-28 明确裁定**不该发生**这类反馈。
    ///     </para>
    ///     <para>
    ///         ⚠️⚠️ <b>必须 await</b>：发放【魔女化】层数是异步的（<c>PowerCmd.Apply&lt;WithPower&gt;</c>）。
    ///         旧实现用 <c>TaskHelper.RunSafely</c> **fire-and-forget** ⇒ 同一个回合开始有多张带本组件的卡时，
    ///         会并发起飞多个 <c>Apply</c>：既可能**丢层**（异步交错、后写覆盖前写），
    ///         异常又会被 <c>RunSafely</c> 吞掉（表现为「偶尔不加层」且日志无痕）。
    ///         用户 2026-09-30 裁定改成**顺序发动**。
    ///     </para>
    /// </summary>
    /// <param name="amount">增加的点数。</param>
    /// <param name="grantWitchification">是否发放【魔女化】层数（遗物 3 的折算路径传 <c>false</c>）。</param>
    /// <param name="choiceContext">
    ///     调用方手上的玩家选择上下文；传 <c>null</c> 时退化成 <c>ThrowingPlayerChoiceContext</c>。
    /// </param>
    public async Task AddWitchificationCountAsync(
        int amount,
        bool grantWitchification = true,
        PlayerChoiceContext? choiceContext = null)
    {
        if (amount <= 0) return;

        WitchificationCount += amount;

        if (!grantWitchification) return;

        // 顺序发动：这里 await，绝不 fire-and-forget。
        await HextechWitchificationCore.OnWitchificationCountGained(Card?.Owner, amount, choiceContext);
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        return Card == card ? playCount + 1 : playCount;
    }

    /// <summary>
    ///     挂上组件时让这张卡「保留」（之后每个回合结束续一次），
    ///     与 <c>Characters/Sherrylin/Components/RetainCounterComponent</c> 是同一套做法。
    ///     <para>
    ///         效果：只要它一直在手牌里，每个回合开始就会自动 +1 点魔女化计数
    ///         ⇒ 触发遗物 3 的「计数每 +1 ⇒ +10 层【魔女化】」。
    ///     </para>
    /// </summary>
    protected override void OnAttach()
    {
        base.OnAttach();
        if (Card?.Pile != null)
            Card.GiveSingleTurnRetain();
    }

    public override Task BeforeSideTurnEndPostfix(
        PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants, ComponentContext componentContext)
    {
        // ⚠️ 按海克斯《对接指南》的联机口径：队友的额外回合（例如佩尔之眼）会**带着那名玩家**
        // 重新进入阵营回合钩子 —— 此时 side 仍是玩家侧、回合号也不增加。
        // 只判 `side == creature.Side` 会在**别人的回合**里多触发一次，
        // 必须判 participants 里有没有本卡的持有者。
        if (Card?.Owner?.Creature is { } creature && participants.Contains(creature))
            Card.GiveSingleTurnRetain();

        return Task.CompletedTask;
    }

    /// <summary>
    ///     玩家侧「保留」⇒ 魔女化计数 +1（会发放【魔女化】）。
    ///     <para>
    ///         ⚠️ 回合开始时就「还在手牌里」的卡，就是上一回合被保留下来的卡
    ///         （手牌在回合结束时会被清空），所以这里等价于「因为保留而加计数」。
    ///         与遗物 3 自己按 25:1 折算写入计数那条路径（<c>grantWitchification: false</c>）互不影响。
    ///     </para>
    ///     <para>
    ///         ⚠️ 这里必须 <c>await</c>（见 <see cref="AddWitchificationCountAsync" />）：
    ///         同一回合开始有多张带本组件的卡时，逐个顺序发放【魔女化】，不再并发/丢层。
    ///         顺带把手上这个 <paramref name="choiceContext" /> 传下去，避免退化成会抛异常的
    ///         <c>ThrowingPlayerChoiceContext</c>。
    ///     </para>
    /// </summary>
    public override async Task AfterPlayerTurnStartEarlyPostfix(
        PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
    {
        if (Card?.Owner != player) return;
        if (Card.Pile?.Type != PileType.Hand) return;

        MainFile.Logger.Info(
            $"[Witchification][诊断] 回合开始钩子触发：卡={Card.GetType().Name} 计数前={WitchificationCount} → +1");
        await AddWitchificationCountAsync(1, choiceContext: choiceContext);
    }
}
