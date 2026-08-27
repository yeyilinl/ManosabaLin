using ManosabaLin.Characters.Common.Components.Abstracts;
using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Hiro.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Common.Components;

/// <summary>
/// 组件「原罪」：由其他卡牌运行时 <see cref="AddComponent"/> 添加到目标诅咒卡上，
/// 不是诅咒卡的 CanonicalComponents（卡牌自带）。挂上后：
/// 1. 卡面描述区顶部显示「原罪」（prefix 本地化）；
/// 2. 悬浮提示按宿主卡类型分发到对应的本地化键（一张卡一个键），
///    描述该卡「宽恕」（保留）/「自惩」（打出）的具体效果；
/// 3. 回合结束自动临时保留（<see cref="GiveSingleTurnRetain"/>，不添加保留关键词）；
/// 4. 「宽恕」：被保留的下一回合开始卡仍返回手牌时触发（按宿主卡分发）；
/// 5. 「自惩」：打出时触发（按宿主卡分发）。
/// 数值类效果通过修改卡自身 DynamicVar 的 BaseValue 实现，卡面文字引用同名变量自动同步。
/// </summary>
public sealed partial class Originalsin : KeywordLikeComponent
{
    private const string HoverTipTitleKey = "ManosabaLin.Originalsin.hovertip.title";
    private const string HiroparanoidTipKey = "ManosabaLin.Originalsin.Hiroparanoid.hovertip.description";
    private const string MargeCharmTipKey = "ManosabaLin.Originalsin.MargeCharm.hovertip.description";

    public override IEnumerable<IHoverTip> HoverTips
    {
        get
        {
            var descriptionKey = Card switch
            {
                Hiroparanoid => HiroparanoidTipKey,
                MargeCharm => MargeCharmTipKey,
                _ => null,
            };
            if (descriptionKey == null) yield break;
            yield return new HoverTip(
                new LocString("cards", HoverTipTitleKey),
                new LocString("cards", descriptionKey));
        }
    }

    // ── 回合结束：临时保留（不添加 Retain 关键词）──
    public override Task BeforeSideTurnEndPostfix(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants,
        ComponentContext componentContext)
    {
        if (Card?.Owner?.Creature is { } creature && side == creature.Side)
        {
            Card.GiveSingleTurnRetain();
        }
        return Task.CompletedTask;
    }

    // ── 宽恕：保留后的下一回合开始卡仍在手牌 ──
    public override async Task AfterPlayerTurnStartEarlyPostfix(
        PlayerChoiceContext choiceContext,
        Player player,
        ComponentContext componentContext)
    {
        if (Card?.Owner != player) return;
        if (Card.Pile?.Type != PileType.Hand) return;

        switch (Card)
        {
            case Hiroparanoid:
                await ForgiveHiroparanoid(choiceContext);
                break;
            case MargeCharm:
                await ForgiveMargeCharm(choiceContext);
                break;
        }
    }

    // ── 自惩：打出时 ──
    public override async Task OnPlayPostfix(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        switch (Card)
        {
            case Hiroparanoid:
                await PunishHiroparanoid(choiceContext);
                break;
            case MargeCharm:
                await PunishMargeCharm(choiceContext);
                break;
        }
    }

    // ── 二阶堂希罗的偏执：宽恕 ──
    // 获得1层下回合开始获得1点能量，并使丢弃牌的数量加1
    private async Task ForgiveHiroparanoid(PlayerChoiceContext choiceContext)
    {
        var card = Card!;
        var owner = card.Owner!;

        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext, owner.Creature, 1m, owner.Creature, card);

        card.DynamicVars["DiscardCount"].BaseValue += 1m;
    }

    // ── 二阶堂希罗的偏执：自惩 ──
    // 抽取1张卡牌，选择消耗1张手卡
    private async Task PunishHiroparanoid(PlayerChoiceContext choiceContext)
    {
        var card = Card!;
        var owner = card.Owner!;

        await CardPileCmd.Draw(choiceContext, 1, owner);

        var selected = (await CardSelectCmd.FromHand(
            choiceContext,
            owner,
            new CardSelectorPrefs(
                new LocString("cards", $"{card.Id.Entry}.selectionScreenPrompt"), 1, 1),
            null,
            card)).FirstOrDefault();

        if (selected != null)
            await CardCmd.Exhaust(choiceContext, selected);
    }

    // ── 宝生玛格的惑情：宽恕 ──
    // 概率增加5%，使随机敌人获得等于你再生或覆甲的一半的再生或覆甲
    private async Task ForgiveMargeCharm(PlayerChoiceContext choiceContext)
    {
        var card = Card!;
        var owner = card.Owner!;

        card.DynamicVars["Chance"].BaseValue += 5m;

        var me = owner.Creature;
        var combatState = me.CombatState;
        if (combatState == null) return;

        var enemies = combatState.Creatures
            .Where(c => c.IsAlive && c.Side != me.Side)
            .ToList();
        if (enemies.Count == 0) return;

        var rng = owner.RunState?.Rng.CombatCardGeneration;
        if (rng == null) return;

        var target = rng.NextItem(enemies);
        if (target == null) return;

        var regen = me.GetPower<RegenPower>()?.Amount ?? 0m;
        var plating = me.GetPower<PlatingPower>()?.Amount ?? 0m;
        if (regen <= 0 && plating <= 0) return;

        if (regen >= plating)
            await PowerCmd.Apply<RegenPower>(choiceContext, target, Half(regen), me, card);
        else
            await PowerCmd.Apply<PlatingPower>(choiceContext, target, Half(plating), me, card);
    }

    // ── 宝生玛格的惑情：自惩 ──
    // 获得等于敌人再生或覆甲的一半再生或覆甲，失去等量血量
    private async Task PunishMargeCharm(PlayerChoiceContext choiceContext)
    {
        var card = Card!;
        var owner = card.Owner!;
        var me = owner.Creature;

        var combatState = me.CombatState;
        if (combatState == null) return;

        var enemies = combatState.Creatures
            .Where(c => c.IsAlive && c.Side != me.Side)
            .ToList();
        if (enemies.Count == 0) return;

        var rng = owner.RunState?.Rng.CombatCardGeneration;
        if (rng == null) return;

        var enemy = rng.NextItem(enemies);
        if (enemy == null) return;

        var regen = enemy.GetPower<RegenPower>()?.Amount ?? 0m;
        var plating = enemy.GetPower<PlatingPower>()?.Amount ?? 0m;
        if (regen <= 0 && plating <= 0) return;

        var amount = Half(regen >= plating ? regen : plating);

        if (regen >= plating)
            await PowerCmd.Apply<RegenPower>(choiceContext, me, amount, me, card);
        else
            await PowerCmd.Apply<PlatingPower>(choiceContext, me, amount, me, card);

        await CreatureCmd.Damage(
            choiceContext,
            me,
            amount,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            card,
            null);
    }

    private static decimal Half(decimal value) => Math.Max(1m, value / 2m);
}