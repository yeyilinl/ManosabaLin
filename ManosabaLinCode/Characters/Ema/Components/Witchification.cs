using ManosabaLin.Characters.Common.Components.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ManosabaLin.Characters.Emalin.Components;

public sealed partial class Witchification : KeywordLikeComponent
{
    /// <summary>
    ///     卡面用悬浮提示（本地化键 <c>ManosabaLin.Witchification.hovertip.title / .description</c>）。
    ///     给「挂载该组件的卡」在 <c>AdditionalHoverTips</c> 里引用。
    /// </summary>
    public static IHoverTip[] Tip => GetHoverTip<Witchification>();

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        return Card == card ? playCount + 1 : playCount;
    }
}
