using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.Powers;

/// <summary>
///     「下一张【多人】卡费用 −1」：由羁绊漂移（<see cref="LinRelics.HextechBondDrift" />）右移成功移除队友一张
///     【多人】卡后挂在该队友身上。
///     <para>
///         层数 = 减费额度，<b>可叠加</b>（<c>PowerStackType.Counter</c>）；只对【多人】卡生效，
///         且<b>一旦该队友打出任意一张【多人】卡，整层立刻清空</b>（用户 2026-10-01 裁定）。
///     </para>
///     <para>
///         ⚠️ 清除放在 <c>BeforeCardPlayed</c>：此时费用已经结算完（<c>TryModifyEnergyCostInCombat</c> 在更早的
///         「能不能打 / 要花多少」阶段就调用过），所以这一张仍然吃到减免，只是打完之后不再留给下一张。
///     </para>
/// </summary>
[RegisterPower]
public sealed class HextechMultiplayerDiscountPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombat(
        CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (Amount <= 0) return false;
        if (!IsMultiplayerCard(card)) return false;
        if (card.Owner?.Creature != Owner) return false;

        modifiedCost = Math.Max(0m, originalCost - Amount);
        return modifiedCost != originalCost;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (Amount <= 0) return;
        if (cardPlay.Card is not { } card) return;
        if (!IsMultiplayerCard(card)) return;
        if (card.Owner?.Creature != Owner) return;

        Flash();
        await PowerCmd.Remove(this);
    }

    /// <summary>是否【多人】卡。</summary>
    internal static bool IsMultiplayerCard(CardModel? card)
        => card?.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly;
}
