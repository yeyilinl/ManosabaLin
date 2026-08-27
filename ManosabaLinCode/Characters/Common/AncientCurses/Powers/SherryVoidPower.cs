using System;

namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

/// <summary>
/// 空洞的隐藏能力：手牌中存在空洞时，造成的小于 13 的伤害减半（向下取整），
/// 并标记"本次打出产生了被减半的伤害"，由空洞卡在打出结算后读取以发放临时力量。
/// </summary>
[RegisterPower]
public sealed class SherryVoidPower : LinCurseConditionalPower<SherryVoid>
{
    private bool _halvedThisPlay;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || !IsCurseInHand()) return 0m;
        if (amount <= 0m || amount >= 13m) return 0m;

        var halved = Math.Floor(amount / 2m);
        _halvedThisPlay = true;
        return -(amount - halved);
    }

    /// <summary>读取并重置"本次打出产生过被减半伤害"的标记。</summary>
    internal bool ConsumeHalvedPlay()
    {
        var had = _halvedThisPlay;
        _halvedThisPlay = false;
        return had;
    }
}
