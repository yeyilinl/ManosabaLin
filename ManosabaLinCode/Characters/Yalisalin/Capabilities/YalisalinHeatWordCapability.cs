using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Yalisalin.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Capabilities;

/// <summary>
/// 升温 / 强升温 动态文字组件。
/// 卡面默认显示"升温"；当卡牌条件满足时调用 <see cref="SetStrongHeat"/>，
/// 卡面文字会自动切换为"强升温"，行为也随之切换。
/// </summary>
[RegisterModelCapability]
[RegisterDefaultModelCapability(typeof(Unseenkindling))]
[RegisterDefaultModelCapability(typeof(Afterschooltestburn))]
[RegisterDefaultModelCapability(typeof(Ashinpages))]
[RegisterDefaultModelCapability(typeof(Burntthermometerpaper))]
[RegisterDefaultModelCapability(typeof(Grazingcritical))]
[RegisterDefaultModelCapability(typeof(Tomorrowburn))]
public sealed class YalisalinHeatWordCapability : ManosabaCardCapability
{
    public const string LocalizationEntry = "MANOSABA_LIN_MODEL_CAPABILITY_YALISALIN_HEAT_WORD";

    protected override string LocKeyPrefix => LocalizationEntry;

    public bool StrongHeat { get; private set; }

    public override IEnumerable<CardDescriptionFragment> GetDescriptionFragments(CardDescriptionContext context)
    {
        // 与 YalisalinFireComponentCapability 一致：直接用 LocString 构造（渲染时才解析），
        // 不用 GetIfExists（查不到会 yield break，导致卡面不显示）。
        var suffix = StrongHeat ? "afterBaseStrong" : "afterBase";
        yield return new CardDescriptionFragment(
            new LocString(LocTable, $"{LocKeyPrefix}.{suffix}"),
            CardDescriptionFragmentPlacement.AfterBase);
    }

    public override IEnumerable<IHoverTip> GetHoverTips(CardModel card)
    {
        // 升温 / 强升温 各自的悬浮提示：hover 到卡面文字时展示当前生效的那一条
        var suffix = StrongHeat ? "hovertip.strong" : "hovertip";
        var title = LocString.GetIfExists(LocTable, $"{LocKeyPrefix}.{suffix}.title");
        var description = LocString.GetIfExists(LocTable, $"{LocKeyPrefix}.{suffix}.description");
        if (title != null && description != null)
        {
            DynamicVars.AddTo(title);
            DynamicVars.AddTo(description);
            yield return new HoverTip(title, description);
        }
    }

    public void SetStrongHeat(bool strong)
    {
        if (StrongHeat == strong)
            return;

        StrongHeat = strong;
        RefreshCardVisuals(Owner);
    }

    private static void RefreshCardVisuals(CardModel? card)
    {
        if (card is null)
            return;

        var node = NCard.FindOnTable(card);
        if (node != null)
            node.UpdateVisuals(card.Pile?.Type ?? PileType.Hand, CardPreviewMode.Normal);
    }
}
