using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Sherrylin.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Sherrylin.Powers;

/// <summary>
/// 怪力能力：回合开始获得20魔女化和1张冲击波的拳风。
/// </summary>
[RegisterPower]
public sealed class SuperStrengthPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>升级：每回合生成的「冲击波的拳风」直接是升级版。</summary>
    [SavedProperty] public bool CardUpgraded { get; set; }

    /// <summary>
    ///     描述随「来源牌是否升级」切键——升级也是一种达成条件。
    ///     power 悬浮（<c>GetDumbHoverTip</c>）读 <c>Description</c>，smart 通道（<c>HoverTips</c>）读
    ///     <c>SmartDescriptionLocKey</c>，两侧都切。做法对齐「共犯」（MeruruAndEmaAccomplicePower）
    ///     与被缚的普罗米修斯（BoundPrometheusPower）。
    /// </summary>
    public override LocString Description =>
        new LocString("powers", CardUpgraded ? $"{Id.Entry}.descriptionEnhanced" : $"{Id.Entry}.description");

    protected override string SmartDescriptionLocKey =>
        CardUpgraded ? $"{Id.Entry}.smartDescriptionEnhanced" : $"{Id.Entry}.smartDescription";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        Flash();

       
        var newCard = Owner.CombatState.CreateCard<ShockwaveFist>(Owner.Player);
        if (CardUpgraded)
            newCard.UpgradeInternal();
        await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, Owner.Player);
    }
}
