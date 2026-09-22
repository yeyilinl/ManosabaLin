using ManosabaLin.Characters.Common.Powers;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using ManosabaLin.Characters.Yalisalin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 火色系剩余卡（带发夹）。
/// </summary>
public sealed class YalisalinFireColorMoreTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .KeepStartingRelics()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-firemore");
    }

    /// <summary>（没被采用的结论）——Power，启用混合结论标记，打出不崩溃。</summary>
    [Fact]
    public async Task Unusedconclusion_plays_without_error()
    {
        var card = await AddToHand<Unusedconclusion>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        Assert.True(true);
    }

    /// <summary>（灰烬书页）——7 格挡；无火色可封存时只给格挡。</summary>
    [Fact]
    public async Task Ashinpages_gives_seven_block()
    {
        var enemy = EnemyAt(0);
        var card = await AddToHand<Ashinpages>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);
        Assert.Equal(7, Player.Creature.Block);
    }

    /// <summary>（烧过的温度计纸）——先自伤 2 点（不可格挡/不受力量），无火色时只自伤。</summary>
    [Fact]
    public async Task Burntthermometerpaper_damages_self_two()
    {
        var enemy = EnemyAt(0);
        var hpBefore = Player.Creature.CurrentHp;
        var card = await AddToHand<Burntthermometerpaper>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);
        Assert.Equal(hpBefore - 2, Player.Creature.CurrentHp);
    }

    /// <summary>（不要冷却）——Power，启用回合开始保留最高火色，打出不崩溃。</summary>
    [Fact]
    public async Task Dontcooldown_plays_without_error()
    {
        var card = await AddToHand<Dontcooldown>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        Assert.True(true);
    }

    /// <summary>（擦边临界）——无火色时消耗无效、不加能量，不崩溃。</summary>
    [Fact]
    public async Task Grazingcritical_plays_without_error()
    {
        var enemy = EnemyAt(0);
        var card = await AddToHand<Grazingcritical>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);
        Assert.True(true);
    }

    /// <summary>（截止转交）——3 次 4 伤，敌人无火色时共 12 伤。</summary>
    [Fact]
    public async Task Deadlinehandoff_deals_three_hits_of_four()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<Deadlinehandoff>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);
        Assert.Equal(hpBefore - 12, enemy.CurrentHp);
    }

    /// <summary>（窗边车票）——Power，启用满格保留，打出不崩溃。</summary>
    [Fact]
    public async Task Ticketonwindow_plays_without_error()
    {
        var card = await AddToHand<Ticketonwindow>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        Assert.True(true);
    }

    /// <summary>（明天的燃烧）——无火色时循环无消耗，不崩溃。</summary>
    [Fact]
    public async Task Tomorrowburn_plays_without_error()
    {
        var enemy = EnemyAt(0);
        var card = await AddToHand<Tomorrowburn>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);
        Assert.True(true);
    }

    /// <summary>（第十三位听众）——Power，启用聆听标记，打出不崩溃。</summary>
    [Fact]
    public async Task Thirteenthlistener_plays_without_error()
    {
        var card = await AddToHand<Thirteenthlistener>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        Assert.True(true);
    }

    /// <summary>
    /// （温差证明）——7 伤；敌人无火色时封存选择返回 null，仍抽 1。
    /// 2026-09-09 已补 selectionScreenPrompt 本地化键（5 语），此前打出必崩。
    /// </summary>
    [Fact]
    public async Task Temperatureproof_deals_seven_damage()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<Temperatureproof>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(hpBefore - 7, enemy.CurrentHp);
    }

    /// <summary>
    /// （口袋里的火柴盒）——敌人无火色时封存选择返回 null 直接结束，不崩溃。
    /// 2026-09-09 已补 selectionScreenPrompt 本地化键（5 语），此前打出必崩。
    /// </summary>
    [Fact]
    public async Task Pocketmatchbox_plays_without_error()
    {
        var enemy = EnemyAt(0);
        var card = await AddToHand<Pocketmatchbox>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.True(true);
    }

    /// <summary>
    /// （每色独立能力）——封存暗橘火色后，能力栏出现对应的
    /// <see cref="YalisalinSealedLightOrangeFirePower"/>，且不会出现其他 3 色的能力。
    /// </summary>
    [Fact]
    public async Task Sealed_fire_power_is_per_color()
    {
        var enemy = EnemyAt(0);

        // 先给敌人加 2 段火色（默认暗橘 LightOrange）
        var unseen = await AddToHand<Unseenkindling>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(unseen, enemy);

        // 灰烬书页：敌人有火色 → 封存最早火色（暗橘）并 Sync
        var ash = await AddToHand<Ashinpages>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(ash, enemy);

        // 只应出现暗橘封存能力
        Assert.NotNull(Player.Creature.GetPower<YalisalinSealedLightOrangeFirePower>());
        Assert.Null(Player.Creature.GetPower<YalisalinSealedBrightYellowFirePower>());
        Assert.Null(Player.Creature.GetPower<YalisalinSealedRedFirePower>());
        Assert.Null(Player.Creature.GetPower<YalisalinSealedBlackRedFirePower>());

        // 再打一次灰烬书页：已有封存 → 复制封存（暗橘+1），暗橘能力仍在且不出现其他色
        var ash2 = await AddToHand<Ashinpages>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(ash2, enemy);

        Assert.NotNull(Player.Creature.GetPower<YalisalinSealedLightOrangeFirePower>());
        Assert.Null(Player.Creature.GetPower<YalisalinSealedBrightYellowFirePower>());
        Assert.Null(Player.Creature.GetPower<YalisalinSealedRedFirePower>());
        Assert.Null(Player.Creature.GetPower<YalisalinSealedBlackRedFirePower>());
    }
}
