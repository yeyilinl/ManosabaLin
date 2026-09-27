using MinionLib.Component;
using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.HiroKeywords;
using ManosabaLin.Characters.Hiro.Components;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using System.Linq;

namespace ManosabaLin.Characters.Hiro.Cards;

/// <summary>
///     即刻轮回 - 2 费攻击，罕见，多人专属。
///     <para>
///         选择自己 1 张带【轮回】的手牌，复制 1 张加入目标友方的抽牌堆，
///         并给「原卡」和「复制品」都加上【即刻轮回】组件。
///     </para>
///     <para>
///         任一方打出带该组件的卡时，会自动再打出其他友方牌堆里至多 2 张同名且带该组件的卡，
///         这些追加打出都算作「打出第一张即刻轮回卡的人」打出的。
///     </para>
/// </summary>
[RegisterCard(typeof(HirolinCardPool))]
public sealed class InstantTransmigration : ManosabaCardTemplate
{
    public InstantTransmigration() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        if (cardPlay.Target?.Player is not { } ally) return;
        if (ally == Owner || ally.Creature.Side != Owner.Creature.Side || !ally.Creature.IsAlive) return;

        var rebirth = TransmigrationRules.TransmigrationCardKeyword;

        // 自己手牌中带【轮回】的卡
        var candidates = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.HasModKeyword(rebirth))
            .ToList();
        if (candidates.Count == 0) return;

        var picked = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            Owner,
            new CardSelectorPrefs(
                new LocString("cards", $"{Id.Entry}.selectionScreenPrompt"), 1, 1))).FirstOrDefault();
        if (picked is null) return;

        // 原卡加上【即刻轮回】组件
        if (picked is ComponentsCardModel originalComponents)
            originalComponents.AddComponent(new InstantTransmigrationComponent());

        // 复制 1 张进目标友方抽牌堆，并同样加上组件（保留【轮回】）
        var copy = CombatState.CreateCard(picked.CanonicalInstance, ally);
        copy.AddModKeyword(rebirth);
        if (copy is ComponentsCardModel copyComponents)
            copyComponents.AddComponent(new InstantTransmigrationComponent());

        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Draw, ally);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
