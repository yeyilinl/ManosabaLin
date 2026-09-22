using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Yalisalin.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Capabilities;

/// <summary>
/// 强升温 关键词悬浮提示组件（与升温独立）。
/// 挂载到卡面描述文字中带有"强升温"效果的卡上：hover 卡面时展示强升温机制的说明。
/// 机制：敌人火色量表非空时，将最高火色提升一级（YalisalinFireColorGauge.TryStrongPromoteOnce）。
/// </summary>
[RegisterModelCapability]
[RegisterDefaultModelCapability(typeof(Deadlinehandoff))]
[RegisterDefaultModelCapability(typeof(Samewrongproblem))]
[RegisterDefaultModelCapability(typeof(Pocketmatchbox))]
public sealed class YalisalinStrongHeatHoverCapability : ManosabaCardCapability
{
    public const string LocalizationEntry = "MANOSABA_LIN_MODEL_CAPABILITY_YALISALIN_STRONG_HEAT";

    protected override string LocKeyPrefix => LocalizationEntry;

    public override IEnumerable<IHoverTip> GetHoverTips(CardModel card)
    {
        var title = LocString.GetIfExists(LocTable, $"{LocKeyPrefix}.hovertip.title");
        var description = LocString.GetIfExists(LocTable, $"{LocKeyPrefix}.hovertip.description");
        if (title != null && description != null)
        {
            DynamicVars.AddTo(title);
            DynamicVars.AddTo(description);
            yield return new HoverTip(title, description);
        }
    }
}
