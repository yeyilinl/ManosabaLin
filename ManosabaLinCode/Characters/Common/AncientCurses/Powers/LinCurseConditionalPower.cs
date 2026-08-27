using System.Linq;

namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

/// <summary>
/// "在手牌才生效"诅咒卡配套的隐藏能力基类：提供对应诅咒卡是否在手牌的判定。
/// 用 <see cref="PowerStackType.None"/> 隐藏自身（玩家看不到这个能力）。
/// </summary>
public abstract class LinCurseConditionalPower<TCurse> : ManosabaPowerTemplate where TCurse : LinAncientCurseCard
{
    protected bool IsCurseInHand()
    {
        if (Owner?.Player is not { } player) return false;
        return PileType.Hand.GetPile(player).Cards.OfType<TCurse>().Any();
    }
}
