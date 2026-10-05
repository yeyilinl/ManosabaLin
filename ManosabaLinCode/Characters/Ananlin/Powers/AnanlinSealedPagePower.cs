using ManosabaLin.Characters.Ananlin.Cards;
using ManosabaLin.Characters.Ananlin.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ManosabaLin.Characters.Ananlin.Powers;

[RegisterPower]
public sealed class AnanlinSealedPagePower : ManosabaPowerTemplate
{
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

    internal void SetUpgradedPages()
    {
        UpgradedPages = true;
    }

    internal async Task AfterSilenceRightClickRewrite(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is not { } player || CombatState is null) return;

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);

        var blessedObject = player.Relics.OfType<BlessedObject>().FirstOrDefault();
        var page = blessedObject is not null
            ? blessedObject.CreateBlankPageOrReplacement(CombatState, UpgradedPages, player)
            : CombatState.CreateCard<BlankPage>(player);
        if (blessedObject is null && UpgradedPages)
            CardCmd.Upgrade(page);

        await CardPileCmd.AddGeneratedCardToCombat(page, PileType.Hand, player);
    }
}
