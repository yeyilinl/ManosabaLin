using ManosabaLin.Characters.Ananlin.Capabilities;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinWhatDoesItMean()
    : ManosabaCardTemplate(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    private enum RandomEffect
    {
        Free,
        DoubleDamage,
        Exhaust,
        Replace
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var target = PickRandomHandCard();
        if (target is null) return;

        var effect = Owner.RunState.Rng.CombatCardGeneration.NextItem(Enum.GetValues<RandomEffect>());

        // 先发生效果
        var undo = await ApplyEffect(choiceContext, target, effect);
        if (!IsUpgraded) return;

        // 升级后：效果已发生，再选择是否接受；拒绝则撤销
        if (!await ShouldAccept(choiceContext, target, effect) && undo is not null)
            await undo(choiceContext);
    }

    private CardModel? PickRandomHandCard()
    {
        var candidates = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card != this && AnanlinCardHelpers.IsPlayableCombatCard(card))
            .ToArray();

        return candidates.Length == 0
            ? null
            : Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
    }

    private async Task<bool> ShouldAccept(PlayerChoiceContext choiceContext, CardModel target, RandomEffect effect)
    {
        var accept = CombatState.CreateCard<AnanlinWhatDoesItMeanAcceptOption>(Owner);
        var reject = CombatState.CreateCard<AnanlinWhatDoesItMeanRejectOption>(Owner);
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            [accept, reject],
            Owner,
            new CardSelectorPrefs(new LocString("cards", $"{Id.Entry}.selectionScreenPrompt"), 1, 1))).FirstOrDefault();

        return selected is AnanlinWhatDoesItMeanAcceptOption;
    }

    private async Task<Func<PlayerChoiceContext, Task>?> ApplyEffect(
        PlayerChoiceContext choiceContext,
        CardModel target,
        RandomEffect effect)
    {
        switch (effect)
        {
            case RandomEffect.Free:
                var originalCost = target.EnergyCost.Canonical;
                target.EnergyCost.SetThisTurnOrUntilPlayed(0, reduceOnly: true);
                return ctx =>
                {
                    target.EnergyCost.SetThisTurnOrUntilPlayed(originalCost, reduceOnly: false);
                    return Task.CompletedTask;
                };
            case RandomEffect.DoubleDamage:
                target.GetOrCreateCapability<AnanlinDoubleDamageOnceCapability>();
                return ctx =>
                {
                    if (target.TryGetCapability<AnanlinDoubleDamageOnceCapability>(out var capability))
                        capability.RemoveFromOwner();
                    return Task.CompletedTask;
                };
            case RandomEffect.Exhaust:
                CardCmd.ApplyKeyword(target, CardKeyword.Exhaust);
                return ctx =>
                {
                    target.RemoveKeyword(CardKeyword.Exhaust);
                    return Task.CompletedTask;
                };
            case RandomEffect.Replace:
                var originalCanonical = target.CanonicalInstance;
                if (!await ReplaceWithSameRarity(choiceContext, target)) return null;
                return async ctx =>
                {
                    if (target.CombatState is not { } combatState) return;
                    var restore = combatState.CreateCard(originalCanonical, Owner);
                    AnanlinCardHelpers.CopyUpgradeLevel(target, restore);
                    await CardCmd.Transform(target, restore, CardPreviewStyle.None);
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
        }
    }

    private async Task<bool> ReplaceWithSameRarity(PlayerChoiceContext choiceContext, CardModel target)
    {
        if (!target.IsTransformable) return false;

        var candidates = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(card => card.Rarity == target.Rarity
                && card.Id != target.Id
                && card.CanBeGeneratedInCombat)
            .ToArray();

        if (candidates.Length == 0) return false;

        var replacement = CardFactory.GetForCombat(
                Owner,
                candidates,
                1,
                Owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();
        if (replacement is null) return false;

        AnanlinCardHelpers.CopyUpgradeLevel(target, replacement);
        await CardCmd.Transform(target, replacement, CardPreviewStyle.None);
        return true;
    }
}
