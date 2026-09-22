using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Yalisalin.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace ManosabaLin.Characters.Yalisalin.Capabilities;

/// <summary>
/// 封存火色 关键词悬浮提示组件。
/// 挂载到拥有"封存火色 / 封存火焰"能力的卡上：hover 卡面时展示该机制的说明。
/// </summary>
[RegisterModelCapability]
[RegisterDefaultModelCapability(typeof(Ashinpages))]
[RegisterDefaultModelCapability(typeof(Unusedconclusion))]
[RegisterDefaultModelCapability(typeof(Reversecalculation))]
[RegisterDefaultModelCapability(typeof(Temperatureproof))]
[RegisterDefaultModelCapability(typeof(Pocketmatchbox))]
[RegisterDefaultModelCapability(typeof(Thirteenthlistener))]
public sealed class YalisalinSealedFireHoverCapability : ManosabaCardCapability
{
    public const string LocalizationEntry = "MANOSABA_LIN_MODEL_CAPABILITY_YALISALIN_SEALED_FIRE";

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
