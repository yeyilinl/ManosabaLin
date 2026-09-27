using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Common.Components;

/// <summary>
///     选择玩家的临时卡（不进任何牌堆/卡池/图鉴）。
///     和 <see cref="YesNoChoiceCardBase" /> 同款做法：标题来自运行时注入的
///     <c>{Id.Entry}.title</c>，描述来自 <c>{Id.Entry}.description</c>。
/// </summary>
internal abstract class PlayerPickCard : CardModel
{
    private const int HiddenEnergyCost = -1;

    protected PlayerPickCard()
        : base(HiddenEnergyCost, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    /// <summary>该选项卡对应的玩家，由 <see cref="PlayerPickScreen" /> 在弹出前写入。</summary>
    internal Player? TargetPlayer { get; set; }

    public override CardPoolModel Pool => ModelDb.CardPool<YalisalinCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public override int MaxUpgradeLevel => 0;

    public override bool ShouldReceiveCombatHooks => false;

    public override string PortraitPath => "card.png".CardsImagePath();

    public override IEnumerable<string> AllPortraitPaths => new[] { PortraitPath };
}

internal sealed class PlayerPickCard0 : PlayerPickCard
{
    public PlayerPickCard0()
    {
    }
}

internal sealed class PlayerPickCard1 : PlayerPickCard
{
    public PlayerPickCard1()
    {
    }
}

internal sealed class PlayerPickCard2 : PlayerPickCard
{
    public PlayerPickCard2()
    {
    }
}

internal sealed class PlayerPickCard3 : PlayerPickCard
{
    public PlayerPickCard3()
    {
    }
}

/// <summary>
///     「从若干玩家中选一个」的选择屏。
///     引擎没有「选择生物/玩家」的命令级 API（只有卡牌选择屏），
///     所以这里沿用 <c>RedirectMoveChoiceScreen</c> / <see cref="YesNoChoiceScreen" />
///     的做法：把每个候选玩家包成一张临时卡，用标准选卡界面弹出。
/// </summary>
public static class PlayerPickScreen
{
    internal const string LocTable = "cards";

    /// <summary>
    ///     弹出选择屏。只有 1 个候选时直接返回，不弹界面。
    /// </summary>
    /// <param name="choiceContext">选择上下文。</param>
    /// <param name="chooser">做出选择的玩家（界面归属）。</param>
    /// <param name="prompt">提示文案（同时作为卡面描述与选择屏标题）。</param>
    /// <param name="candidates">候选玩家（调用方负责过滤存活/阵营）。</param>
    /// <returns>选中的玩家；取消时返回 null。</returns>
    public static async Task<Player?> Choose(
        PlayerChoiceContext choiceContext,
        Player chooser,
        LocString prompt,
        IReadOnlyList<Player> candidates)
    {
        if (candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        var cards = new List<CardModel>();
        var injected = new Dictionary<string, string>();

        for (var i = 0; i < candidates.Count && i < 4; i++)
        {
            var card = CreateSlot(i);
            if (card is null) break;

            card.Owner = chooser;
            card.TargetPlayer = candidates[i];

            cards.Add(card);
            injected[$"{card.Id.Entry}.title"] = candidates[i].Character.Title.GetFormattedText();
            injected[$"{card.Id.Entry}.description"] = prompt.GetRawText();
        }

        if (cards.Count == 0) return null;

        LocManager.Instance.GetTable(LocTable).MergeWith(injected);

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cards,
            chooser,
            new CardSelectorPrefs(prompt, 1, 1))).FirstOrDefault();

        return (selected as PlayerPickCard)?.TargetPlayer;
    }

    private static PlayerPickCard? CreateSlot(int index)
    {
        return index switch
        {
            0 => (PlayerPickCard)ModelDb.Card<PlayerPickCard0>().ToMutable(),
            1 => (PlayerPickCard)ModelDb.Card<PlayerPickCard1>().ToMutable(),
            2 => (PlayerPickCard)ModelDb.Card<PlayerPickCard2>().ToMutable(),
            3 => (PlayerPickCard)ModelDb.Card<PlayerPickCard3>().ToMutable(),
            _ => null
        };
    }
}
