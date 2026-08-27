using System.Collections.Generic;

namespace ManosabaLin.Characters.Common.AncientCurses;

/// <summary>
/// 特殊先古诅咒卡的标记接口。图鉴过滤补丁据此把这些卡从先古图鉴挪到诅咒图鉴。
/// </summary>
public interface IManosabaAncientCurseCard { }

/// <summary>
/// 特殊先古诅咒卡基类：Lin 卡池、先古稀有度、诅咒类型、图鉴可见（默认 true）。
/// 图鉴归属（诅咒图鉴可见、先古图鉴不可见）由
/// <see cref="ManosabaLin.Patches.LinAncientCurseCardLibraryPatch"/> 实现。
/// </summary>
public abstract class LinAncientCurseCard(int energyCost = 2, TargetType targetType = TargetType.None)
    : ManosabaCardTemplate(energyCost, CardType.Curse, CardRarity.Ancient, targetType, shouldShowInCardLibrary: true),
        IManosabaAncientCurseCard
{
    public override int MaxUpgradeLevel => 0;

    
}
