using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Hiro;
using ManosabaLin.Characters.Hiro.Cards;
using ManosabaLin.Characters.Hiro.Monsters;
using ManosabaLin.Characters.Hiro.Rewards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 「战胜第一层残骸首领 ⇒ 从牌组中升级两张牌」的回归测试。
/// </summary>
/// <remarks>
///     <para>
///         ⚠️ 2026-09-27 机制改动：奖励不再由 <c>CombatRoom.AddExtraReward</c> 挂到房间奖励列表里
///         （那需要玩家在奖励屏里再点一条），而是改由 <see cref="GuardOneBossUpgradeHook" />
///         在 <c>Hook.BeforeCombatRewardOffered</c>（<c>reward.Offer()</c> 之前）弹出升级界面。
///     </para>
///     <para>
///         因此本文件从「断言房间里有额外奖励」改为：
///         ① 断言房间<b>不再</b>挂额外奖励（防止旧路径被重新引入 ⇒ 重复发放）；
///         ② 走真实的 <c>Hook.BeforeCombatRewardOffered</c> 分发，断言残骸首领房里确实升级了牌。
///         ② 走真实分发而不是直接 new 出钩子，是为了顺带验证单例真的注册并订阅上了跑局钩子流
///         （<c>[RegisterSingleton]</c> → <c>ModHelper.SubscribeForRunStateHooks</c>）。
///     </para>
/// </remarks>
public sealed class GuardOneBossRewardTests : CombatTestSuite
{
    /// <summary>
    ///     用真实的残骸首领遭遇（<see cref="RoomType.Boss" /> + <c>Encounter.Id == GuardOneEncounter</c>），
    ///     这两个条件正是钩子的守门条件，因此不能像以前那样自己拼一个 inline 遭遇。
    /// </summary>
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Hiro>()
            .Encounter<GuardOneEncounter>()
            .WithSeed("manosaba-guardone-hook");
    }

    /// <summary>牌组里已升级的卡牌张数。</summary>
    private int UpgradedCount()
        => PileType.Deck.GetPile(Player).Cards.Count(static c => c.IsUpgraded);

    /// <summary>牌组里当前可升级的卡牌张数。</summary>
    private int UpgradableCount()
        => PileType.Deck.GetPile(Player).Cards.Count(static c => c.IsUpgradable);

    /// <summary>
    ///     打出一次 1 费攻击。
    ///     纯粹是为了满足 harness 的硬要求：每个用例至少要执行一个战斗 GameAction，
    ///     否则 headless 收尾会以 TERMINATE 报错（与断言内容无关）。
    /// </summary>
    private async Task ExecuteTokenAction()
    {
        var card = await AddToHand<HiroAttack>();
        await WaitForIdle();
        await Play(card, Enemy<GuardOneMonster>());
    }

    /// <summary>
    ///     残骸首领房进入时，钩子应当复用原版升级屏幕（headless 下由测试 Entry 的自动选择器代为选牌）
    ///     并升级 <see cref="GuardOneBossUpgradeHook.MaxUpgradeCount" /> 张牌。
    /// </summary>
    [Fact]
    public async Task Boss_reward_hook_upgrades_two_cards_in_guard_one_room()
    {
        var room = Assert.IsType<CombatRoom>(Player.RunState.CurrentRoom);

        Assert.Equal(RoomType.Boss, room.RoomType);
        Assert.Equal(ModelDb.GetId<GuardOneEncounter>(), room.Encounter.Id);

        await ExecuteTokenAction();

        var upgradableBefore = UpgradableCount();
        Assert.True(
            upgradableBefore >= GuardOneBossUpgradeHook.MaxUpgradeCount,
            "测试牌组应至少有 2 张可升级牌，否则升级张数会被收窄、断言失去意义。");

        var upgradedBefore = UpgradedCount();

        // 走真实分发：这正是 CombatRoom.OfferRoomEndRewards 里的那一次调用。
        await Hook.BeforeCombatRewardOffered(new RewardsSet(Player), Player.RunState, room);

        var delta = UpgradedCount() - upgradedBefore;
        Assert.Equal(GuardOneBossUpgradeHook.MaxUpgradeCount, delta);
    }

    /// <summary>
    ///     升级奖励不再挂进房间奖励列表：旧路径若被重新引入，会和钩子重复发放。
    /// </summary>
    [Fact]
    public async Task Guard_one_room_does_not_hold_extra_reward_anymore()
    {
        await ExecuteTokenAction();

        var room = Assert.IsType<CombatRoom>(Player.RunState.CurrentRoom);

        var hasLegacyReward = room.ExtraRewards.TryGetValue(Player, out var rewards)
                              && rewards!.OfType<GuardOneBossUpgradeReward>().Any();

        Assert.False(
            hasLegacyReward,
            "残骸首领升级奖励已改由 GuardOneBossUpgradeHook 发放，不应再通过 AddExtraReward 挂到房间奖励里。");
    }

    /// <summary>牌组里没有可升级牌时，钩子应当安静跳过（不弹屏、不抛异常）。</summary>
    [Fact]
    public async Task Hook_is_noop_when_no_upgradable_card()
    {
        await ExecuteTokenAction();

        // 再把所有牌都升满，制造「无可升级」的局面。
        var deck = PileType.Deck.GetPile(Player);
        CardCmd.Upgrade(deck.Cards.ToList(), CardPreviewStyle.None);
        await WaitForIdle();

        Assert.Equal(0, UpgradableCount());

        var room = Assert.IsType<CombatRoom>(Player.RunState.CurrentRoom);
        var upgradedBefore = UpgradedCount();

        // 不应抛异常，也不应改变任何牌的升级状态。
        await Hook.BeforeCombatRewardOffered(new RewardsSet(Player), Player.RunState, room);

        Assert.Equal(upgradedBefore, UpgradedCount());
    }
}

/// <summary>
/// 反向对照：非残骸首领房不该触发升级奖励。
/// 单独一个 fixture 是因为这里的遭遇必须是 inline 的（<c>Encounter.Id != GuardOneEncounter</c>）。
/// </summary>
public sealed class GuardOneBossRewardNegativeTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Hiro>()
            .AddEnemy<GuardOneMonster>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-guardone-reward-negative");
    }

    private int UpgradedCount()
        => PileType.Deck.GetPile(Player).Cards.Count(static c => c.IsUpgraded);

    /// <summary>
    ///     击杀残骸首领（但走的是 inline 遭遇 / 非首领房）不会触发升级；
    ///     同时确认「旧机制不再发放房间额外奖励」。
    /// </summary>
    [Fact]
    public async Task Hook_skips_non_boss_room_and_no_extra_reward_is_added()
    {
        var boss = Enemy<GuardOneMonster>();
        boss.SetMaxHpInternal(13);
        await WaitForIdle();

        // 用 1 费 6 伤的 HiroAttack 打死它。
        for (var i = 0; i < 5 && boss.IsAlive; i++)
        {
            var card = await AddToHand<HiroAttack>();
            await PlayerCmd.SetEnergy(10, Player);
            await WaitForIdle();
            await Play(card, boss);
        }

        Assert.False(boss.IsAlive);

        var room = Assert.IsType<CombatRoom>(Player.RunState.CurrentRoom);
        Assert.NotEqual(ModelDb.GetId<GuardOneEncounter>(), room.Encounter.Id);

        var upgradedBefore = UpgradedCount();
        await Hook.BeforeCombatRewardOffered(new RewardsSet(Player), Player.RunState, room);

        Assert.Equal(upgradedBefore, UpgradedCount());

        var hasLegacyReward = room.ExtraRewards.TryGetValue(Player, out var rewards)
                              && rewards!.OfType<GuardOneBossUpgradeReward>().Any();
        Assert.False(hasLegacyReward, "残骸首领奖励不应再挂到房间奖励列表里。");
    }
}
