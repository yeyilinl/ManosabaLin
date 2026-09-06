namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinRaiyaExtraPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public bool ForcedExtra { get; set; }

    public void SyncFromCard(RaiyaMadness card)
    {
        Amount = (int)card.DynamicVars["ExtraChance"].BaseValue;
        InvokeDisplayAmountChanged();
    }

    public bool ConsumeForcedExtra()
    {
        if (!ForcedExtra) return false;
        ForcedExtra = false;
        return true;
    }
}
