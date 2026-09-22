using System;

namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class SherryVoidPower : LinCurseConditionalPower<SherryVoid>
{
    private bool _halvedThisPlay;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    protected override bool IsVisibleInternal => false;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || !IsCurseInHand()) return 0m;
        if (amount <= 0m) return 0m;

        var threshold = 13m;
        if (Owner?.Player is { } player)
        {
            var curse = PileType.Hand.GetPile(player).Cards.OfType<SherryVoid>().FirstOrDefault();
            if (curse != null && curse.DynamicVars.TryGetValue("Threshold", out var thresholdVar))
                threshold = thresholdVar.BaseValue;
        }

        if (amount >= threshold) return 0m;

        var halved = Math.Floor(amount / 2m);
        _halvedThisPlay = true;
        return -(amount - halved);
    }

    internal bool ConsumeHalvedPlay()
    {
        var had = _halvedThisPlay;
        _halvedThisPlay = false;
        return had;
    }
}
