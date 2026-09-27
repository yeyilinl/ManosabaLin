using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Hiro;
using ManosabaLin.Characters.Hiro.Cards;
using ManosabaLin.Characters.Hiro.Monsters;
using ManosabaLin.Characters.Hiro.Rewards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 「击杀第一层残骸首领 ⇒ 追加『升级两张牌』战胜奖励」的回归测试。
/// 重点核查：
/// 1. 残骸首领（<see cref="GuardOneMonster" />）死亡后，当前战斗房间的
///    <see cref="CombatRoom.ExtraRewards" /> 里会为本地玩家挂上一条
///    <see cref="GuardOneBossUpgradeReward" />；
/// 2. 该奖励排在常规战后奖励之前（<c>RewardsSetIndex</c> = 0）；
/// 3. 反向对照：普通杂兵死亡不会触发该奖励，确保只作用于第一层残骸。
/// </summary>
public sealed class GuardOneBossRewardTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        // 放一只残骸首领 + 一只假人：击杀首领后战斗不会直接结束，避免无头收尾噪音。
        battle
            .Player<Hiro>()
            .AddEnemy<GuardOneMonster>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-guardone-reward");
    }

    /// <summary>把目标压成小怪，方便逐张攻击把它打死。</summary>
    private static void Weaken(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
        => creature.SetMaxHpInternal(13);

    /// <summary>反复打出 1 费 6 伤的 <see cref="HiroAttack" />，直到目标死亡（最多 5 次）。</summary>
    private async Task AttackUntilDead(MegaCrit.Sts2.Core.Entities.Creatures.Creature target)
    {
        for (var i = 0; i < 5 && target.IsAlive; i++)
        {
            var card = await AddToHand<HiroAttack>();
            await PlayerCmd.SetEnergy(10, Player);
            await WaitForIdle();
            await Play(card, target);
        }
    }

    [Fact]
    public async Task Killing_guard_one_adds_deck_upgrade_reward_to_current_combat_room()
    {
        var boss = Enemy<GuardOneMonster>();
        Weaken(boss);
        await WaitForIdle();

        await AttackUntilDead(boss);

        Assert.False(boss.IsAlive);

        var room = Assert.IsType<CombatRoom>(Player.RunState.CurrentRoom);
        Assert.True(
            room.ExtraRewards.TryGetValue(Player, out var rewards),
            "击败残骸首领后没有为玩家追加任何额外奖励。");

        var reward = Assert.IsType<GuardOneBossUpgradeReward>(Assert.Single(rewards!));
        Assert.Equal(0, reward.RewardsSetIndex);
    }

    [Fact]
    public async Task Killing_normal_enemy_adds_no_deck_upgrade_reward()
    {
        var dummy = Enemy<BigDummy>();
        Weaken(dummy);
        await WaitForIdle();

        await AttackUntilDead(dummy);

        Assert.False(dummy.IsAlive);

        var room = Assert.IsType<CombatRoom>(Player.RunState.CurrentRoom);
        var hasReward = room.ExtraRewards.TryGetValue(Player, out var rewards)
                        && rewards!.OfType<GuardOneBossUpgradeReward>().Any();
        Assert.False(hasReward, "普通杂兵死亡不应该发放残骸首领奖励。");
    }
}
