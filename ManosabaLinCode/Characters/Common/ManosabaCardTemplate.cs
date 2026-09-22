using Godot;
using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ManosabaLin.RitsuAdapters;
using STS2RitsuLib.Scaffolding.Content;
using MinionLib.Component.Core;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common;

public abstract class ManosabaCardTemplate(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : ModComponentsCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    public override bool CanBeGeneratedInCombat => Rarity != CardRarity.Token && base.CanBeGeneratedInCombat;

    public override bool CanBeGeneratedByModifiers => Rarity != CardRarity.Token && base.CanBeGeneratedByModifiers;

    public override CardAssetProfile AssetProfile
    {
        get
        {
            var slug = GetType().Name.ToLowerInvariant();
            var fileName = $"{slug}.png";

            var big = fileName.BigCardsImagePath();
            var small = fileName.CardsImagePath();
            var portrait = ResourceLoader.Exists(big)
                ? big
                : ResourceLoader.Exists(small)
                    ? small
                    : "card.png".CardsImagePath();

            var beta = $"beta/{fileName}".CardsImagePath();
            var betaPortrait = ResourceLoader.Exists(beta) ? beta : null;

            return new CardAssetProfile(portrait, betaPortrait);
        }
    }

    // 羁绊体系卡自动获得羁绊能力（任意角色使用均生效）：
    // 类似原版储君角色"使用获得辉星卡时若没有辉星则自动添加辉星计数器"。
    // 该 hook 由 Hook.BeforeCardPlayed 在卡牌 OnPlay 之前对战斗中所有模型（含卡牌）调用，
    // 手动打出与自动打出（AutoPlay）都走此流程；打出任意卡时本卡都会被通知，
    // 仅在"打出的是自己且属于羁绊体系卡且角色还没有羁绊能力"时自动获得 BondPower。
    protected override async Task BeforeCardPlayed(CardPlay cardPlay, ComponentContext componentContext)
    {
        await base.BeforeCardPlayed(cardPlay, componentContext);

        if (cardPlay.Card != this)
            return;
        if (!BondCardVisualRules.IsBondCard(this))
            return;

        var creature = Owner.Creature;
        if (creature.GetPower<BondPower>() != null)
            return;

        await PowerCmd.Apply<BondPower>(
            new ThrowingPlayerChoiceContext(), creature, 1m, creature, this, true);
    }
}
