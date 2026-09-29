using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Emalin.Components;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MinionLib.Component.Interfaces;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物 3：魔女化。
///     <list type="number">
///         <item>【魔女化计数】每 +1，立刻获得 <b>10 层【魔女化】</b>（由 <see cref="Witchification.AddWitchificationCount" /> 回调）。</item>
///         <item>回合开始时若【魔女化】&gt; 300，则降到 100；被消耗掉的层数按 25:1 转成「计数」加到带组件的卡上。</item>
///     </list>
///     <para>
///         ⚠️ 用户 2026-09-28 裁定：「10 倍」= <b>计数每 +1 就 +10 层【魔女化】</b>，
///         <b>不是</b>「挂上组件那一刻按当前【魔女化】层数 × 10 放大一次」（旧实现已删）。
///     </para>
///     <para>
///         ⚠️ 用户 2026-09-28 二次裁定：第 2 条把计数分给组件卡时，<b>不会</b>再增加【魔女化】层数。
///         即 400 → 降到 100 → 产生 12 点计数，这 12 点<b>只分给组件卡</b>，不会让【魔女化】涨回 220。
///         实现上走 <c>AddWitchificationCount(count, grantWitchification: false)</c>。
///     </para>
///     <para>
///         另：削减产生的计数是**拆分**给带组件的卡的 —— 当玩家有多张带组件的卡时，这 `count` 点
///         由它们**瓜分**（合计恰为 <c>count</c>），逐点随机指派；<b>不是</b>每张各拿一份。
///         （用户 2026-09-29 裁定 "12 点计数是拆分"。）
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechWitchificationCore : ManosabaRelicTemplate
{
    private const int DropThreshold = 300;
    private const int DropTarget = 100;
    private const int LayersPerCount = 25;

    /// <summary>计数每 +1 兑换的【魔女化】层数。</summary>
    internal const int WithPowerPerCount = 10;

    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>
    ///     由 <see cref="Witchification.AddWitchificationCount" /> 回调：计数每 +1 ⇒ +10 层【魔女化】。
    ///     遗物不在场 / 计数没有真正增加时直接返回。
    /// </summary>
    internal static async Task OnWitchificationCountGained(Player? owner, int amount)
    {
        if (owner is null || amount <= 0) return;

        var relic = owner.Relics.OfType<HextechWitchificationCore>().FirstOrDefault();
        if (relic is null) return;

        relic.Flash();
        await PowerCmd.Apply<WithPower>(
            new ThrowingPlayerChoiceContext(),
            owner.Creature,
            amount * WithPowerPerCount,
            owner.Creature,
            null,
            false);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        var withPower = Owner.Creature.GetPower<WithPower>();
        if (withPower is null || withPower.Amount <= DropThreshold) return;

        var consumed = (int)withPower.Amount - DropTarget;
        if (consumed <= 0) return;

        Flash();
        await PowerCmd.ModifyAmount(choiceContext, withPower, -consumed, Owner.Creature, null, false);

        var count = consumed / LayersPerCount;
        if (count <= 0) return;

        // ⚠️ grantWitchification: false —— 这条路径**只把计数分给组件卡**，
        // 绝不能再回调 OnWitchificationCountGained（否则会变成
        // 「削层 → 加计数 → 又 +count×10 层涨回去」的自我反馈，用户 2026-09-28 明确否掉）。
        //
        // ⚠️ 用户 2026-09-29 裁定「12 点计数是**拆分**」：这 `count` 点是**在组件卡之间瓜分**
        // （合计恰为 count），**不是**每张卡各拿一份。分发方式按用户 2026-09-28 的原话
        // 「将 12 点计数**随机分给**所有牌中魔女化组件卡」⇒ 逐点随机指派。
        var targets = CardsWithWitchification(Owner).ToList();
        if (targets.Count == 0) return;

        if (targets.Count == 1)
        {
            targets[0].AddWitchificationCount(count, grantWitchification: false);
            return;
        }

        // 多人下一切「随机」必须走 RunState.Rng（联机 desync 铁律）。
        var rng = Owner.RunState.Rng.CombatCardSelection;
        for (var i = 0; i < count; i++)
            targets[rng.NextInt(targets.Count)].AddWitchificationCount(1, grantWitchification: false);
    }

    private static IEnumerable<Witchification> CardsWithWitchification(Player player)
    {
        foreach (var pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust })
        {
            foreach (var card in pileType.GetPile(player).Cards)
            {
                if (card is IComponentsCardModel components
                    && components.GetComponent<Witchification>() is { } witchification)
                    yield return witchification;
            }
        }
    }
}
