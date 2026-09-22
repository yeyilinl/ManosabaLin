using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Characters.Common.Components;

/// <summary>
/// "是/否"选卡界面（参考原版 <see cref="KnowledgeDemon"/> 的 IChoosable 模式）：
/// 将"是/否"做成两张临时卡，通过标准选卡界面 <see cref="CardSelectCmd.FromChooseACardScreen"/>
/// 让玩家选择，选中后直接返回结果。临时卡不进任何牌堆，选"否"不再对游离卡调用
/// RemoveFromCombat，从根本上解决旧 YesNoTabPicker 覆盖层 UI 选"否"后卡牌卡屏的问题。
/// </summary>
public interface IYesNoChoice
{
    /// <summary>该卡是否代表"是"。</summary>
    bool IsYes { get; }
}

/// <summary>选卡界面中代表"是"的临时卡。</summary>
internal sealed class YesChoiceCard : YesNoChoiceCardBase, IYesNoChoice
{
    public YesChoiceCard() { }

    public bool IsYes => true;
}

/// <summary>选卡界面中代表"否"的临时卡。</summary>
internal sealed class NoChoiceCard : YesNoChoiceCardBase, IYesNoChoice
{
    public NoChoiceCard() { }

    public bool IsYes => false;
}

/// <summary>"是/否"临时卡基类：仅用于选择界面展示，永不进入卡池/牌堆/图鉴。</summary>
internal abstract class YesNoChoiceCardBase : CardModel
{
    private const int HiddenEnergyCost = -1;

    protected YesNoChoiceCardBase()
        : base(HiddenEnergyCost, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    public override CardPoolModel Pool => ModelDb.CardPool<YalisalinCardPool>();
    public override CardPoolModel VisualCardPool => Pool;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override int MaxUpgradeLevel => 0;
    public override bool ShouldReceiveCombatHooks => false;
    public override string PortraitPath => "card.png".CardsImagePath();
    public override IEnumerable<string> AllPortraitPaths => new[] { PortraitPath };
}

/// <summary>
/// "是/否"选择器：弹标准选卡界面，一张"是"卡 + 一张"否"卡（标题为是/否，描述为提示文本）。
/// 与原版 KnowledgeDemon 一致：候选卡为临时模型，选中即结算，不进牌堆。
/// 返回 true=是，false=否。
/// </summary>
public static class YesNoChoiceScreen
{
    internal const string LocTable = "cards";

    /// <summary>通用"是"选项卡标题（settings_ui 表）。</summary>
    public static LocString Yes { get; } = new("settings_ui", "MANOSABALIN_YESNO.yes");

    /// <summary>通用"否"选项卡标题（settings_ui 表）。</summary>
    public static LocString No { get; } = new("settings_ui", "MANOSABALIN_YESNO.no");

    /// <summary>
    /// 弹出"是/否"选卡界面。
    /// </summary>
    /// <param name="choiceContext">打出卡牌的玩家选择上下文。</param>
    /// <param name="viewer">做出选择的玩家。</param>
    /// <param name="prompt">两张卡共用的卡面描述（本地化，如"是否将1张【原罪诅咒】加入手牌？"）。</param>
    /// <param name="yesTitle">"是"卡标题（本地化）。</param>
    /// <param name="noTitle">"否"卡标题（本地化）。</param>
    /// <returns>true=是，false=否。</returns>
    public static async Task<bool> Pick(
        PlayerChoiceContext choiceContext,
        Player viewer,
        LocString prompt,
        LocString yesTitle,
        LocString noTitle)
    {
        var yes = (YesChoiceCard)ModelDb.Card<YesChoiceCard>().ToMutable();
        var no = (NoChoiceCard)ModelDb.Card<NoChoiceCard>().ToMutable();
        yes.Owner = viewer;
        no.Owner = viewer;

        RegisterChoiceLocalization(prompt, yesTitle, noTitle);

        var chosen = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            new CardModel[] { yes, no },
            viewer);

        return chosen is IYesNoChoice yn && yn.IsYes;
    }

    /// <summary>
    /// 运行时把"是/否"标题与提示合并进 cards 本地化表（与 RedirectMoveChoiceScreen 相同机制），
    /// 使两张临时卡的卡面标题/描述显示为本次选择的文案。
    /// </summary>
    private static void RegisterChoiceLocalization(LocString prompt, LocString yesTitle, LocString noTitle)
    {
        var yesEntry = ModelDb.GetId<YesChoiceCard>().Entry;
        var noEntry = ModelDb.GetId<NoChoiceCard>().Entry;

        LocManager.Instance.GetTable(LocTable).MergeWith(new Dictionary<string, string>
        {
            [$"{yesEntry}.title"] = yesTitle.GetRawText(),
            [$"{yesEntry}.description"] = prompt.GetRawText(),
            [$"{noEntry}.title"] = noTitle.GetRawText(),
            [$"{noEntry}.description"] = prompt.GetRawText(),
        });
    }
}
