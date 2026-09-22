using ManosabaLin.Characters.Ananlin.Powers;
using ManosabaLin.Characters.Ananlin.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace ManosabaLin.Characters.Ananlin.Cards;

[RegisterCard(typeof(AnanlinCardPool))]
public sealed class AnanlinMarginalNote() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<BlankPage>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        if (CombatState is null) return;

        // 每有一个敌人意图为攻击，就能在所有牌中选择1张牌变成【空白书页】
        var attackerCount = CombatState.Enemies.Count(enemy => enemy.IsAlive && HasAttackIntent(enemy));
        if (attackerCount <= 0) return;

        var candidates = PileType.Hand.GetPile(Owner).Cards
            .Concat(PileType.Draw.GetPile(Owner).Cards)
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .Where(card => card != this)
            .ToList();
        if (candidates.Count == 0) return;

        var maxSelect = Math.Min(attackerCount, candidates.Count);
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, maxSelect))).ToArray();

        foreach (var card in selected)
        {
            var page = this.CreateBlankPageOrBlessedReplacement(IsUpgraded);
            await CardCmd.Transform(card, page);
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
    }

    private bool HasAttackIntent(Creature enemy)
    {
        return this.Sketchbook()?.IsAttackIntent(enemy)
            ?? enemy.Monster?.NextMove?.Intents.Any(static intent => intent is AttackIntent) == true;
    }
}
