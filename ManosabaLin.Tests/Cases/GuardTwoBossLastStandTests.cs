using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Hiro.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 奈叶香的姐姐（<see cref="GuardTwoBossMonster" />）的「诈尸」机制 × 艾玛【魔女因子】即死 的交互回归。
///
/// 曾经的故障：魔女因子把 BOSS 打空血时，<c>GuardTwoBossMonster.AfterDeath</c> 在
/// 「死亡被阻止」（<c>wasRemovalPrevented == true</c>）那一趟里就去发卡牌奖励，
/// 而奖励发放会发起一次玩家选择，它嵌在伤害管线里，把同一次伤害中紧随其后的
/// <c>Hook.AfterPreventingDeath</c>（无限血 + 护盾）整段打断
/// ⇒ 诈尸音效响了、却没复活也没护盾，BOSS 停在 0 血直接死。
/// </summary>
public sealed class GuardTwoBossLastStandTests : CombatTestSuite
{
    private const int InfiniteHp = 999999999;

    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Emalin>()
            .AddEnemy<GuardTwoBossMonster>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-guardtwo-laststand");
    }

    private async Task PlayPrisonLaw(Creature target)
    {
        var card = await AddToHand<PrisonLaw>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, target);
        await WaitForIdle();
    }

    /// <summary>把 BOSS 压到 30/30 并塞 30 层【魔女因子】⇒ 第一击（6 伤）就会踩到即死阈值。</summary>
    private async Task<Creature> PrepareLethalWitchFactorBoss()
    {
        var boss = Enemy<GuardTwoBossMonster>();
        boss.SetMaxHpInternal(30);
        await ApplyPower<EmaWitchFactorPower>(boss, 30);
        await WaitForIdle();
        return boss;
    }

    /// <summary>核心回归：即死必须让 BOSS 进诈尸（无限血 + 护盾），而不是原地暴毙。</summary>
    [Fact]
    public async Task WitchFactor_kill_triggers_last_stand_with_infinite_hp_and_shield()
    {
        var boss = await PrepareLethalWitchFactorBoss();

        await PlayPrisonLaw(boss);

        Assert.True(boss.IsAlive, "诈尸没有生效：BOSS 被打空血后直接死亡了。");
        Assert.Equal(InfiniteHp, boss.MaxHp);
        Assert.Equal(InfiniteHp, boss.CurrentHp);
        Assert.True(boss.Block > 0, "诈尸没有给出护盾：下一击会被「已破盾」判定直接真杀。");
    }

    /// <summary>诈尸阶段护盾未破时不该死（这一条的「下一击」正是被 force kill 的入口）。</summary>
    [Fact]
    public async Task Last_stand_boss_cannot_die_while_its_shield_holds()
    {
        var boss = await PrepareLethalWitchFactorBoss();

        await PlayPrisonLaw(boss);
        Assert.True(boss.IsAlive);
        Assert.True(boss.Block > 0);

        for (var i = 0; i < 20 && boss.IsAlive && boss.Block > 0; i++)
        {
            var blockBefore = boss.Block;
            await PlayPrisonLaw(boss);
            Assert.True(boss.IsAlive, $"护盾还剩 {blockBefore} 点，不该在第 {i + 2} 击就被击杀。");
        }

        Assert.Equal(0, boss.Block);
        Assert.True(boss.IsAlive, "把护盾打破的那一击本身不应该击杀 BOSS。");
    }
}
