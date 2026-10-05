using ManosabaLin.Characters.Ananlin.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Ananlin.Powers;

[RegisterPower]
public sealed class AnanlinPageCornerPower : ManosabaPowerTemplate
{
    private const int EnergyThreshold = 4;

    [SavedProperty] public int PendingEnergy { get; set; }
    [SavedProperty] public bool UpgradedPages { get; set; }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    ///     描述随「来源牌是否升级」切键（升级也是一种达成条件）。两侧通道都切，做法对齐「共犯」/
    ///     被缚的普罗米修斯。
    /// </summary>
    public override LocString Description =>
        new LocString("powers", UpgradedPages ? $"{Id.Entry}.descriptionEnhanced" : $"{Id.Entry}.description");

    protected override string SmartDescriptionLocKey =>
        UpgradedPages ? $"{Id.Entry}.smartDescriptionEnhanced" : $"{Id.Entry}.smartDescription";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar("EnergyThreshold", EnergyThreshold)
    ];

    internal void SetUpgradedPages()
    {
        UpgradedPages = true;
    }

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (amount <= 0 || card.Owner != Owner.Player || CombatState is null) return;

        PendingEnergy += amount;
        var pages = PendingEnergy / EnergyThreshold;
        if (pages <= 0) return;

        PendingEnergy %= EnergyThreshold;
        Flash();

        for (var i = 0; i < pages; i++)
        {
            var blankPage = CombatState.CreateCard<MarginPage>(card.Owner);
            if (UpgradedPages)
                CardCmd.Upgrade(blankPage);

            await CardPileCmd.AddGeneratedCardToCombat(blankPage, PileType.Hand, card.Owner);
        }
    }
}
