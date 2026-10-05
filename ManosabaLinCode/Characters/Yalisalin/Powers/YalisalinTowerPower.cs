using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Yalisalin.Components;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
///     高塔（亚里沙专属，由「高塔」打出后获得，本场战斗有效）：
///     <list type="number">
///         <item>把敌人的火色格上限从 6 格改造为 <see cref="FireColorSegments" /> 格（每色 4 格）；</item>
///         <item>每当你打出带【余火】的牌时，额外随机生成 1 张「其他角色」的牌加入本次连接选项。</item>
///     </list>
/// </summary>
[RegisterPower]
public sealed class YalisalinTowerPower : ManosabaPowerTemplate, IYalisalinFireComponentModifier
{
    /// <summary>改造后的火色格数（默认 6 格 ⇒ 12 格）。</summary>
    public int FireColorSegments => 12;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    ///     「当你打出余火牌额外连接一张其他角色的牌」：
    ///     只把一个「随机生成的其他角色牌」加进本次连接选项。
    ///     选中它 → 照常自动打出一次，然后正常进入弃牌堆；被烧掉 → 没有任何额外效果
    ///     （故意不登记为独占烧牌，也不设置 BurnOnlyExclusiveCards）。
    /// </summary>
    public void ModifyFireComponentChoiceOptions(YalisalinFireComponentContext context)
    {
        try
        {
            if (context.SourceCard is not { } source)
                return;

            if (!YalisalinFireComponentRules.HasFireComponent(source))
                return;

            if (CreateRandomOtherCharacterCard(context.Owner) is not { } extra)
                return;

            context.AddChoiceOption(extra);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
    }

    /// <summary>
    ///     从「除亚里沙以外的角色卡池」里随机取一张可生成的牌，生成一张新实例。
    ///     沿用「艾玛的筹码」的既有口径（排除 Ancient / Basic / Event，且要求 CanBeGeneratedInCombat）。
    /// </summary>
    private static CardModel? CreateRandomOtherCharacterCard(Player owner)
    {
        if (owner.Creature.CombatState is not { } combatState)
            return null;

        var candidates = owner.UnlockState.CharacterCardPools
            .Where(pool => pool != owner.Character.CardPool)
            .SelectMany(pool => pool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint))
            .Where(static card => card.Rarity != CardRarity.Ancient
                                  && card.Rarity != CardRarity.Basic
                                  && card.Rarity != CardRarity.Event
                                  && card.CanBeGeneratedInCombat)
            .ToList();

        if (candidates.Count == 0)
            return null;

        var template = owner.RunState.Rng.CombatCardGeneration.NextItem(candidates);
        return template == null ? null : combatState.CreateCard(template, owner);
    }
}
