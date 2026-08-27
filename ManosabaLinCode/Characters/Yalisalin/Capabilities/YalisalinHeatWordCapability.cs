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
        var suffix = StrongHeat ? "afterBaseStrong" : "afterBase";
        if (LocString.GetIfExists(LocTable, $"{LocKeyPrefix}.{suffix}") is { } loc)
            yield return new CardDescriptionFragment(loc, CardDescriptionFragmentPlacement.AfterBase);
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
