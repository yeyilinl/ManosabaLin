namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 特殊原罪诅咒卡基类：Lin 卡池、普通诅咒稀有度、诅咒类型、图鉴可见（默认 true）。
/// 卡框视觉（先古金框）由
/// <see cref="ManosabaLin.Patches.LinAncientCurseFramePatch"/> 提供，
/// 稀有度数据本身保持 Curse，与普通诅咒卡一致。
/// </summary>
public abstract class LinAncientCurseCard(int energyCost = 2, TargetType targetType = TargetType.None)
    : ManosabaCardTemplate(energyCost, CardType.Curse, CardRarity.Curse, targetType, shouldShowInCardLibrary: true)
{
    public override int MaxUpgradeLevel => 0;
}
