using ManosabaLin.Characters.Common.Components.Abstracts;
using ManosabaLin.Characters.Common.LinRelics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Component.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Emalin.Components;

public sealed partial class Witchification : KeywordLikeComponent
{
    /// <summary>
    ///     本卡累积的「魔女化计数」。
    ///     由联动遗物 <see cref="HextechWitchificationCore"/> 在回合开始削减【魔女化】时按 25:1 写入。
    ///     <para>
    ///         ⚠️ 刻意**不加** <c>[ComponentState]</c>（与 <c>RetainCounterComponent._counter</c> 一致）：
    ///         加进去会改变组件的二进制序列化布局，破坏新旧版本之间的联机/存档兼容。
    ///         因此该计数**不跨存档持久化**，只在当前战斗内有效。
    ///     </para>
    ///     <para>
    ///         <b>玩家侧**主动**增加计数时</b>，每 +1 会触发一次联动遗物 3：<b>+10 层【魔女化】</b>
    ///         （遗物不在场时内部直接返回）。
    ///         ⚠️ 但遗物 3 自己在回合开始「削超量层数 → 按 25:1 折算写入计数」那条路径**不会**触发
    ///         （走 <see cref="AddWitchificationCount" /> 的 <c>grantWitchification: false</c>）。
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
    /// </summary>
    public void AddWitchificationCount(int amount, bool grantWitchification = true)
    {
        if (amount <= 0) return;

        WitchificationCount += amount;

        if (grantWitchification)
            TaskHelper.RunSafely(HextechWitchificationCore.OnWitchificationCountGained(Card?.Owner, amount));
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
        if (Card?.Owner?.Creature is { } creature && side == creature.Side)
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
    /// </summary>
    public override Task AfterPlayerTurnStartEarlyPostfix(
        PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
    {
        if (Card?.Owner != player) return Task.CompletedTask;
        if (Card.Pile?.Type != PileType.Hand) return Task.CompletedTask;

        AddWitchificationCount(1);
        return Task.CompletedTask;
    }
}
