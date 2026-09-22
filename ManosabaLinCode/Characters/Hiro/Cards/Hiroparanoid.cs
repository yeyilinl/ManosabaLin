using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.AncientCurses;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Hiro.Cards;

[RegisterCard(typeof(LinCardPool))]
public sealed class Hiroparanoid : LinAncientCurseCard
{

    // 偏执属于特殊先古诅咒卡：1 费、可打出（覆盖基类的默认"无法打出"）。
    public Hiroparanoid() : base(1, TargetType.Self) { }

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        // 可打出，无 Unplayable；不声明 Exhaust 关键词（不显示"消耗"标签），
        // 但保留"打出并消耗"的机制——在 OnPlay 末尾手动 CardCmd.Exhaust
        get { yield break; }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("DiscardCount", 1m); }
    }

    // ★ 被抽到时失去 1 点能量
    protected override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw, ComponentContext componentContext)
    {
        var source = this;
        if (card != source) return;

        await PlayerCmd.LoseEnergy(1m, source.Owner);
    }

    // ★ 打出：选一张牌丢弃
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;

        var card = (await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            source.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt,
                source.DynamicVars["DiscardCount"].IntValue),
            null,
            source
        )).FirstOrDefault();

        if (card == null) return;

        await CardCmd.Discard(choiceContext, card);

        // 不出现在关键词里，但机制上"打出并消耗"：打完后将自身移入消耗堆。
        // 单词之后再消耗，避免影响上面从手牌选牌丢弃的逻辑。
        await CardCmd.Exhaust(choiceContext, source);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
    }
}
