using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Hiro.Monsters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ManosabaLin.Characters.Hiro.Rewards;

/// <summary>
///     残骸首领（<see cref="GuardOneMonster" />）战胜后的「从牌组中升级卡牌」奖励。
/// </summary>
/// <remarks>
///     <para>
///         走 <see cref="MegaCrit.Sts2.Core.Models.AbstractModel.BeforeCombatRewardOffered" /> 钩子，订阅 <c>HookType.Run</c>。
///         该钩子在 <c>CombatRoom.OfferRoomEndRewards</c> 里位于 <c>reward.Offer()</c> <b>之前</b>且被 await，
///         所以升级界面会先于金币 / 卡牌奖励出现（不需要玩家再去奖励列表里点一条自定义奖励）。
///     </para>
///     <para>
///         <b>不自己写 UI</b>：直接复用原版升级选牌屏 <c>NDeckUpgradeSelectScreen</c>
///         （休息点打磨用的是同一个屏幕，自带升级前后对比预览）。
///     </para>
///     <para>
///         <b>为什么 MaxSelect 必须收窄</b>：该屏幕的确认条件是「已选数 &gt;= <c>prefs.MaxSelect</c>」。
///         若写死 2 而牌组只剩 1 张可升级，条件永远无法达成 ⇒ <b>选卡界面卡死</b>。
///         因此这里用 <see cref="MaxUpgradeCount" /> 与「实际可升级张数」取小。
///     </para>
///     <para>
///         <b>同步</b>：<see cref="CardSelectCmd.FromDeckForUpgrade" /> 内部用 <c>PlayerChoiceSynchronizer</c>
///         同步玩家选择；多人局里每个玩家各有一个 <c>RewardsSet</c>，本机对远端玩家会等待其选择结果。
///         因此这里<b>不能</b>维护任何机器本地状态（例如"该玩家已经发过奖励"的 HashSet）——
///         一旦两端判断分叉，就会一边弹屏一边不弹，直接卡死同步。
///     </para>
///     <para>
///         <b>阵亡玩家要跳过</b>：原版 <c>RewardsSet.Offer()</c> 第一行就 <c>return Player.Creature.IsDead</c>，
///         而本钩子跑在 <c>Offer()</c> <b>之前</b>且不带这道检查 ⇒ 必须自己补上，
///         否则玩家已阵亡却会弹出升级选牌界面。
///     </para>
/// </remarks>
[RegisterSingleton]
public sealed class GuardOneBossUpgradeHook : HookedSingletonModel
{
    /// <summary>牌组可升级卡牌足够时，一次奖励允许升级的张数。</summary>
    public const int MaxUpgradeCount = 2;

    /// <summary>本奖励只发给第一层（<c>CurrentActIndex == 0</c>）的残骸首领。</summary>
    private const int GuardOneActIndex = 0;

    /// <summary>创建单例并订阅跑局钩子流。</summary>
    public GuardOneBossUpgradeHook()
        : base(HookType.Run)
    {
    }

    /// <inheritdoc />
    public override async Task BeforeCombatRewardOffered(RewardsSet rewards, CombatRoom room)
    {
        try
        {
            if (!ShouldOffer(rewards, room)) return;

            var player = rewards.Player;

            // 先按可升级张数收窄上限，再决定要不要弹屏（顺序很重要，见类型注释）。
            var upgradableCount = CountUpgradableCards(player);

            // 没有可升级的牌：直接跳过。
            // （FromDeckForUpgrade 自己也会返回空数组，但这里提前退出就不会白弹一次界面。）
            if (upgradableCount <= 0) return;

            var selectCount = Math.Min(MaxUpgradeCount, upgradableCount);

            var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, selectCount)
            {
                // 保留取消：若界面因任何原因无法确认，玩家仍有退路，不会把进度卡死在奖励流程里。
                Cancelable = true,
                RequireManualConfirmation = true
            };

            var selected = (await CardSelectCmd.FromDeckForUpgrade(player, prefs)).ToList();
            if (selected.Count == 0)
            {
                MainFile.Logger.Info(
                    $"[GuardOneBossUpgradeHook] {Describe(player)} 跳过了残骸首领升级奖励。");
                return;
            }

            // CardPreviewStyle.None：真实升级，不需要高亮数值变化（高亮是预览用的）。
            CardCmd.Upgrade(selected, CardPreviewStyle.None);

            MainFile.Logger.Info(
                $"[GuardOneBossUpgradeHook] {Describe(player)} 升级了 {selected.Count} 张牌："
                + $"{string.Join(", ", selected.Select(static c => c.Id.Entry))}");
        }
        catch (Exception ex)
        {
            // 钩子里未捕获的异常会把整段战后奖励流程吃掉，这里必须兜住。
            MainFile.Logger.Info($"[GuardOneBossUpgradeHook] BeforeCombatRewardOffered failed: {ex}");
        }
    }

    /// <summary>
    ///     判定本次奖励集合是否该发「残骸首领升级奖励」。
    /// </summary>
    private static bool ShouldOffer(RewardsSet rewards, CombatRoom room)
    {
        // 阵亡玩家不发：（原版 RewardsSet.Offer() 自己也会直接 return 跳过阵亡玩家，
        // 而钩子跑在 Offer() 之前、不带这道检查，所以这里必须自己补上，
        // 否则会出现「玩家已经死了却弹出升级选牌界面」。）
        if (rewards.Player?.Creature is not { IsAlive: true }) return false;

        // 只在首领房触发。
        if (room.RoomType != RoomType.Boss) return false;

        // 只认残骸首领：按 encounter 判定，避免第一层的其它首领也被误发。
        if (room.Encounter is not { } encounter) return false;
        if (encounter.Id != ModelDb.GetId<GuardOneEncounter>()) return false;

        // 限定第一层。若以后想让残骸首领在任何层数都给奖励，删掉下面这一行即可。
        if (rewards.Player.RunState is not { } runState) return false;
        if (runState.CurrentActIndex != GuardOneActIndex) return false;

        return true;
    }

    /// <summary>
    ///     统计玩家牌组里当前可升级的卡牌张数。
    ///     与 <see cref="CardSelectCmd.FromDeckForUpgrade" /> 的筛选口径保持一致（<c>IsUpgradable</c>）。
    /// </summary>
    private static int CountUpgradableCards(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        return PileType.Deck.GetPile(player).Cards.Count(static card => card.IsUpgradable);
    }

    private static string Describe(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        return player?.Character is { } character ? character.Id.Entry : "player";
    }
}
