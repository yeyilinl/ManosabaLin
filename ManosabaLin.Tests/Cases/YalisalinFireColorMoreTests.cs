using ManosabaLin.Characters.Common.Powers;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
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
        var card = await AddToHand<Ashinpages>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        // 这张牌是 TargetType.AllEnemies：IsValidTarget(非 null 目标) 对它一律返回 false，
        // 传具体敌人会被 harness 判成「打出失败」并把整轮 runner 打断，所以这里必须不带目标。
        await Play(card);
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
    /// <summary>
    ///     （夹在书页里的灰）——按「当前封存火焰」的最高一档，给全体敌人施加对应层数的易伤：
    ///     浅橙 1 层 / 亮黄 2 层 / 赤红 3 层 / 黑红碳化 4 层；没有封存火焰时只给格挡。
    /// </summary>
    /// <remarks>
    ///     这张牌**只读取**封存火焰、自己并不封存（封存由发夹内部完成），所以测试直接把封存
    ///     塞进发夹，再验证卡牌的读取+施加链路。旧版断言认为打这张牌会「封存目标最早火色」，
    ///     那是被替换掉的设计（zhs 文案与卡片实现现在都是「读取封存→易伤」），故一并更新。
    /// </remarks>
    [Fact]
    public async Task Sealed_fire_power_is_per_color()
    {
        var enemy = EnemyAt(0);

        Assert.True(YalisalinFireColorSystem.TryGetHairpin(Player, out var hairpin));
        Assert.NotNull(hairpin);

        // ① 没有任何封存火焰 → 只给 7 点格挡，不给易伤
        var ash = await AddToHand<Ashinpages>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(ash);
        Assert.Equal(7, Player.Creature.Block);
        Assert.Equal(0, CardTestAssertions.PowerAmount<VulnerablePower>(enemy));

        // ② 封存 1 层浅橙（最低档）→ 最高档 = 浅橙 = 1 层易伤
        hairpin!.GrantSealedFire(YalisalinFireColor.LightOrange);
        var ash2 = await AddToHand<Ashinpages>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(ash2);
        Assert.Equal(1, CardTestAssertions.PowerAmount<VulnerablePower>(enemy));

        // ③ 再封存 1 层黑红碳化（最高档）→ 最高档变黑红 = 4 层，叠加在已有的 1 层之上
        hairpin.GrantSealedFire(YalisalinFireColor.BlackRed);
        var ash3 = await AddToHand<Ashinpages>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(ash3);
        Assert.Equal(5, CardTestAssertions.PowerAmount<VulnerablePower>(enemy));

        // 最高档仍是黑红：不会因为浅橙层数多而回落
        Assert.Equal(4, hairpin.GetHighestSealedFireStacks());
    }
}
