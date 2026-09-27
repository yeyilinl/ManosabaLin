using ManosabaLin.Characters.Common.Components.Abstracts;
using ManosabaLin.Characters.Yalisalin.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace ManosabaLin.Characters.Yalisalin.Components;

/// <summary>
///     余火材薪：标记「这张牌可以作为余火的材薪被添火」的独立组件。
///     与原罪（<see cref="ManosabaLin.Characters.Common.Components.Originalsin" />）同为
///     <see cref="KeywordLikeComponent" /> 体系，但刻意<b>不在卡面上显示</b>任何文字：
///     既没有 .prefix 文案，<see cref="FormatPrefix" /> 也恒返回空串，
///     效果只通过 <see cref="HoverTips" /> 里的悬浮提示说明。
///     组件编号由源生成器按「根命名空间 + 类名」生成，即
///     <c>ManosabaLin.YalisalinFirewood</c>，本地化键也以此为前缀。
/// </summary>
public sealed partial class YalisalinFirewood : KeywordLikeComponent
{
    private const string LocPrefix = "ManosabaLin.YalisalinFirewood";

    // 卡面不显示：即使日后有人补了 .prefix 文案，也不会印在卡牌上。
    protected override string FormatPrefix(LocString loc)
    {
        return string.Empty;
    }

    /// <summary>
    ///     按承载的卡片给出对应的「添火后会发生什么」说明。
    ///     卡片自身的其余效果仍写在卡面文案里，这里只负责余火材薪这一部分的悬浮提示。
    /// </summary>
    public override IEnumerable<IHoverTip> HoverTips
    {
        get
        {
            var descriptionKey = Card switch
            {
                Holdmypain => $"{LocPrefix}.holdMyPain",
                Unneededgoodchild => $"{LocPrefix}.unneededGoodChild",
                Fifthselfproof => $"{LocPrefix}.fifthSelfProof",
                _ => null,
            };

            if (descriptionKey == null)
                yield break;

            yield return new HoverTip(
                new LocString("cards", $"{LocPrefix}.hovertip.title"),
                new LocString("cards", descriptionKey));
        }
    }
}
