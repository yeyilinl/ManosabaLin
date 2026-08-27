using System.Collections.Generic;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 自控的颜料：无法打出的普通诅咒卡（不进入任何图鉴）。由城崎诺亚的裹挟生成。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class SelfControlledPaint : ManosabaCardTemplate
{
    public SelfControlledPaint()
        : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
}
