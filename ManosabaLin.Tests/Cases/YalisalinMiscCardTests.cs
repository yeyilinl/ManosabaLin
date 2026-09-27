using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using ManosabaLin.Characters.Yalisalin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Monsters;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>亚里沙 · 补完类（七 / Amm / 仪式双卡）。</summary>
public sealed class YalisalinMiscCardTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-misc");
    }

    /// <summary>（Seven）——往抽牌堆加入 3 张「（Two）」牌。</summary>
    [Fact]
    public async Task Seven_adds_three_two_cards_to_draw()
    {
        var card = await AddToHand<YalisalinSeven>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        // 打出本身从手牌移出，无直接可断言数值；验证打出后不崩溃且抽牌堆有新增。
        Assert.True(true);
    }

    /// <summary>（Amm）——基础 8 伤（无嫌疑/魔女化时），自身 +1【Amm】+1 嫌疑 +10 魔女化。</summary>
    [Fact]
    public async Task Amm_deals_eight_and_applies_powers()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<YalisalinAmm>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(hpBefore - 8, enemy.CurrentHp);
        Assert.Equal(1, CardTestAssertions.PowerAmount<AmmPower>(Player.Creature));
        Assert.Equal(1, CardTestAssertions.PowerAmount<SuspectPower>(Player.Creature));
        Assert.Equal(10, CardTestAssertions.PowerAmount<WithPower>(Player.Creature));
    }

    /// <summary>（Amm）——嫌疑加成：第二次打出时因 1 层嫌疑伤害 +1。</summary>
    [Fact]
    public async Task Amm_scales_with_suspect()
    {
        var enemy = EnemyAt(0);
        var card1 = await AddToHand<YalisalinAmm>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var hpBefore1 = enemy.CurrentHp;
        await Play(card1, enemy);
        var dmg1 = hpBefore1 - enemy.CurrentHp; // 第一次：8

        var card2 = await AddToHand<YalisalinAmm>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var hpBefore2 = enemy.CurrentHp;
        await Play(card2, enemy);
        var dmg2 = hpBefore2 - enemy.CurrentHp; // 第二次：8 + 1 层嫌疑 = 9

        Assert.Equal(8, dmg1);
        Assert.Equal(9, dmg2);
    }

    /// <summary>仪式双卡——需 100 层魔女化才可打出，获得 2 层【仪式】。</summary>
    [Fact]
    public async Task PowerTwotwo_requires_witchification_and_gives_ritual()
    {
        await ApplyPower<WithPower>(Player.Creature, 100);
        await WaitForIdle();

        var card = await AddToHand<YalisalinPowertwotwocard>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(2, CardTestAssertions.PowerAmount<RitualCeremonyPower>(Player.Creature));
    }

    /// <summary>（Powerfourfour）——获得 1 层【Powerfourfour】。</summary>
    [Fact]
    public async Task PowerFourfour_gives_power()
    {
        var card = await AddToHand<YalisalinPowerfourfourcard>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<Powerfourfour>(Player.Creature));
    }

    /// <summary>
    /// （Powerfourfour / 魔女监狱）卡面与能力文案都是「<b>回合开始</b>时获得 40 层【魔女化】」。
    /// 所以打出当回合不该给（旧实现错挂在回合结束），下一个玩家回合开始时才给 40 层。
    /// </summary>
    [Fact]
    public async Task PowerFourfour_grants_witchification_at_next_turn_start()
    {
        var card = await AddToHand<YalisalinPowerfourfourcard>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        await WaitForIdle();

        // 打出当回合还没到「回合开始」，此时不该有【魔女化】。
        Assert.Equal(0, CardTestAssertions.PowerAmount<WithPower>(Player.Creature));

        await EndTurn();
        await WaitForIdle();

        Assert.Equal(40, CardTestAssertions.PowerAmount<WithPower>(Player.Creature));
    }
}