using System.Collections.Generic;
using System.Linq;
using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物（安安 1/3）：**书页多送一个无色选项**。
///     <para>
///         持有者打出【空白书页】/【留白书页】/【借来的留白书页】时，
///         在原来的卡池选项之外**再追加一个选项** —— 一张随机<b>无色</b>牌，
///         且这张牌<b>本回合免费</b>（<see cref="CardModel.SetToFreeThisTurn" />），
///         选中后会照常进入手牌，等于「可以免费打出一次」。
///     </para>
///     <para>
///         实现方式：<see cref="AppendColorlessOption" /> 由 <c>AnansSketchbook</c> 的三个书页入口
///         （<c>UseBlankPage</c> / <c>UseMarginPage</c> / <c>ResolveBorrowedMarginPage</c>）
///         在**升级循环之后、选择之前**调用 ⇒ 追加的那张无色牌<b>不会</b>跟着书页一起被升级，
///         且书页本身的既有行为（卡池抽取、选项数量、跳过、诅咒等）一字未动。
///     </para>
///     <para>
///         ⚠️ 随机取牌走 <c>Player.RunState.Rng.CombatCardGeneration</c>（联机 desync 铁律：
///         一切「随机」必须走 run state 的 Rng，禁 <c>Random.Shared</c>）。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechColorlessPage : ManosabaRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>该玩家是否持有本遗物。</summary>
    internal static bool IsActiveFor(Player? player)
        => player?.Relics.OfType<HextechColorlessPage>().Any() == true;

    /// <summary>
    ///     给书页的选项列表追加「一张可免费打出一次的无色牌」。
    ///     没持遗物 / 不在战斗 / 无色池取不到合适牌时静默返回（保持原书页行为）。
    /// </summary>
    internal static void AppendColorlessOption(Player player, List<CardModel> options)
    {
        if (!IsActiveFor(player)) return;

        var combatState = player.Creature.CombatState;
        if (combatState is null) return;

        var template = RollColorlessTemplate(player);
        if (template is null) return;

        var card = combatState.CreateCard(template, player);
        card.SetToFreeThisTurn();
        options.Add(card);
    }

    private static CardModel? RollColorlessTemplate(Player player)
    {
        var candidates = ModelDb.CardPool<ColorlessCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(static card => card.CanBeGeneratedInCombat)
            .Where(static card => card.Rarity is not (CardRarity.Basic
                or CardRarity.Ancient
                or CardRarity.Event
                or CardRarity.Token
                or CardRarity.Status
                or CardRarity.Curse
                or CardRarity.Quest))
            .ToArray();

        return candidates.Length == 0
            ? null
            : player.RunState.Rng.CombatCardGeneration.NextItem(candidates);
    }
}
