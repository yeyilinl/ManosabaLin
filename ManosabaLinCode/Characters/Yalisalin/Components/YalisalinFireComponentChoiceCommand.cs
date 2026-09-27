using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace ManosabaLin.Characters.Yalisalin.Components;

/// <summary>
///     一次余火选择的结果：选中的牌 + 玩家在选卡界面上做过的「添火/跳过」记录。
/// </summary>
/// <param name="Card">玩家选中的牌。</param>
/// <param name="RightClickRecords">
///     右键记录，按玩家操作顺序排列。元素含义：<c>0</c> = 跳过一次添火；<c>n &gt; 0</c> = 把这次添火
///     作用在 <c>ChoiceOptions[n - 1]</c> 上。两端拿到的这份列表必须逐字节相同。
/// </param>
internal readonly record struct YalisalinFireComponentChoice(
    CardModel? Card,
    IReadOnlyList<int> RightClickRecords);

/// <summary>
///     余火组件专用的选卡命令。
///
///     与 <see cref="CardSelectCmd.FromChooseACardScreen" /> 的区别：后者只能回传「卡片索引」这一个 int，
///     而余火选择界面还允许玩家右键候选牌做「添火强化」。右键若只改本机状态、不走同步通道，联机时两端
///     会算出不同结果（实测会让房主白拿 1 层紫藤亚里沙的魔法 + 1 点能量，随后被 checksum 判定为状态分歧）。
///
///     这里改用 <see cref="PlayerChoiceResult.FromIndexes" /> 把「选中牌索引 + 全部右键记录」打包成同一份
///     玩家选择一起同步，远端用同一份索引回放，从而保证 <c>AppliedRightClicks</c> 两端一致。
///
///     刻意与引擎实现保持同构（本地/远端分支、测试选择器分支、<c>choiceId</c> 预留时机都不变），
///     以免破坏 <c>ManosabaLin.Tests</c> 里 localOnly 的自动选择器。
/// </summary>
internal static class YalisalinFireComponentChoiceCommand
{
    public static async Task<YalisalinFireComponentChoice> Choose(
        PlayerChoiceContext choiceContext,
        IReadOnlyList<CardModel> cards,
        Player player,
        YalisalinFireComponentContext fireContext)
    {
        if (cards.Count == 0)
        {
            Log.Error("A fire component selection screen was about to be shown with 0 options. Returning empty to prevent softlock.");
            return new YalisalinFireComponentChoice(null, []);
        }

        UndoEndTurnIfNecessary(player);

        // 测试用全局选择器：直接给答案，不预占 choiceId、不走网络同步（与引擎一致）。
        if (CardSelectCmd.Selector is { } globalSelector)
        {
            var picked = (await globalSelector.GetSelectedCards(cards, 0, 1)).FirstOrDefault();
            return new YalisalinFireComponentChoice(picked, []);
        }

        var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
        var choiceId = synchronizer.ReserveChoiceId(player);
        await choiceContext.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.None);

        try
        {
            if (ShouldSelectLocalCard(player))
            {
                if (CardSelectCmd.LocalSelector is { } localSelector)
                {
                    var picked = (await localSelector.GetSelectedCards(cards, 0, 1)).FirstOrDefault();
                    return new YalisalinFireComponentChoice(picked, []);
                }

                NPlayerHand.Instance?.CancelAllCardPlay();

                // headless（TestMode）下 ShowScreen 返回 null：此时按「未选牌」收尾，但仍要把索引发出去，
                // 否则远端会一直等这份选择。
                var screen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip: false);
                if (LocalContext.IsMe(player))
                {
                    foreach (var card in cards)
                        SaveManager.Instance.MarkCardAsSeen(card);
                }

                var chosen = screen == null ? null : (await screen.CardsSelected()).FirstOrDefault();
                var chosenIndex = chosen == null ? -1 : cards.IndexOf(chosen);

                var records = fireContext.RightClickRecords.ToList();
                var indexes = new List<int>(records.Count + 1) { chosenIndex };
                indexes.AddRange(records);

                synchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(indexes));

                // 本机同样用「刚发出去的那份索引」回放，保证两端走的是同一条代码路径。
                return new YalisalinFireComponentChoice(chosen, records);
            }

            var remoteIndexes = (await synchronizer.WaitForRemoteChoice(player, choiceId)).AsIndexes();
            if (remoteIndexes.Count == 0)
                return new YalisalinFireComponentChoice(null, []);

            var remoteChosenIndex = remoteIndexes[0];
            var remoteChosen = remoteChosenIndex < 0 || remoteChosenIndex >= cards.Count
                ? null
                : cards[remoteChosenIndex];

            return new YalisalinFireComponentChoice(remoteChosen, remoteIndexes.Skip(1).ToList());
        }
        finally
        {
            await choiceContext.SignalPlayerChoiceEnded();
        }
    }

    private static bool ShouldSelectLocalCard(Player player)
    {
        if (!LocalContext.IsMe(player))
            return false;

        return RunManager.Instance.NetService.Type != NetGameType.Replay;
    }

    private static void UndoEndTurnIfNecessary(Player player)
    {
        if (CombatManager.Instance.IsPlayerReadyToEndTurn(player)
            && player.Creature.CombatState != null
            && player.Creature.CombatState.CurrentSide == CombatSide.Player)
        {
            CombatManager.Instance.UndoReadyToEndTurn(player);
        }
    }
}
