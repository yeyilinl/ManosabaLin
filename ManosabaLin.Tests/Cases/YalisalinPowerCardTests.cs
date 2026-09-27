using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.ManosabaLinCode.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>亚里沙 · Power 类卡（无卡牌选择 UI，可直接打出并断言 Power 层数）。</summary>
public sealed class YalisalinPowerCardTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-power");
    }

    /// <summary>（Aam Power）——自身 +3 嫌疑，目标敌人 +1【Aam】。</summary>
    [Fact]
    public async Task Aam_gives_self_suspect_and_enemy_aam()
    {
        var enemy = EnemyAt(0);
        var card = await AddToHand<YalisalinAam>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(3, CardTestAssertions.PowerAmount<SuspectPower>(Player.Creature));
        Assert.Equal(1, CardTestAssertions.PowerAmount<AamPower>(enemy));
    }

    /// <summary>（Nym）——6 伤，自身 +1 嫌疑、+10 魔女化，敌人 +1【Nym】。</summary>
    [Fact]
    public async Task Nym_deals_six_and_applies_powers()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<YalisalinNym>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(hpBefore - 6, enemy.CurrentHp);
        Assert.Equal(1, CardTestAssertions.PowerAmount<SuspectPower>(Player.Creature));
        Assert.Equal(10, CardTestAssertions.PowerAmount<WithPower>(Player.Creature));
        Assert.Equal(1, CardTestAssertions.PowerAmount<NymPower>(enemy));
    }

    /// <summary>宽恕印记——获得【宽恕印记】Power。</summary>
    [Fact]
    public async Task KuanShuYinJi_gives_power()
    {
        var card = await AddToHand<KuanShuYinJi>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<KuanShuYinJiPower>(Player.Creature));
    }

    /// <summary>忤逆之证——获得【忤逆之证】Power。</summary>
    [Fact]
    public async Task NiNiZhiZheng_gives_power()
    {
        var card = await AddToHand<NiNiZhiZheng>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<NiNiZhiZhengPower>(Player.Creature));
    }

    /// <summary>（GuiltyChain）——获得【GuiltyChain】Power。</summary>
    [Fact]
    public async Task GuiltyChain_gives_power()
    {
        var card = await AddToHand<YalisalinGuiltyChain>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<GuiltyChainPower>(Player.Creature));
    }

    /// <summary>刑枷加身——获得【刑枷】Power。</summary>
    [Fact]
    public async Task XingJiaJiaShen_gives_power()
    {
        var card = await AddToHand<XingJiaJiaShen>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<XingJiaJiaShenPower>(Player.Creature));
    }

    /// <summary>循环法典——获得【循环法典】Power。</summary>
    [Fact]
    public async Task XunHuanFaDian_gives_power()
    {
        var card = await AddToHand<XunHuanFaDian>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<XunHuanFaDianPower>(Player.Creature));
    }

    /// <summary>点火（专属魔法卡）——2 费能力牌，获得 1 层【点火】。</summary>
    [Fact]
    public async Task Ignite_gives_power()
    {
        var card = await AddToHand<Ignite>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<IgnitePower>(Player.Creature));
    }

    /// <summary>（Mgm）——自身 +1 嫌疑、+1【Mgm】，+5 格挡。</summary>
    [Fact]
    public async Task Mgm_gives_suspect_mgm_and_block()
    {
        var card = await AddToHand<YalisalinMgm>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(1, CardTestAssertions.PowerAmount<SuspectPower>(Player.Creature));
        Assert.Equal(1, CardTestAssertions.PowerAmount<MgmPower>(Player.Creature));
        Assert.Equal(5, Player.Creature.Block);
    }

    /// <summary>（Mllm）——目标 15 真伤 + 目标【Mllm】，自身 +1 嫌疑。</summary>
    [Fact]
    public async Task Mllm_deals_fifteen_and_applies_powers()
    {
        var hpBefore = Player.Creature.CurrentHp;
        var card = await AddToHand<YalisalinMllm>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, Player.Creature);

        Assert.Equal(hpBefore - 15, Player.Creature.CurrentHp);
        Assert.Equal(1, CardTestAssertions.PowerAmount<MllmPower>(Player.Creature));
        Assert.Equal(1, CardTestAssertions.PowerAmount<SuspectPower>(Player.Creature));
    }

    /// <summary>（Lym）——自身 +5 嫌疑，敌人 +1【Lym】。</summary>
    [Fact]
    public async Task Lym_gives_suspect_and_enemy_lym()
    {
        var enemy = EnemyAt(0);
        var card = await AddToHand<YalisalinLym>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(5, CardTestAssertions.PowerAmount<SuspectPower>(Player.Creature));
        Assert.Equal(1, CardTestAssertions.PowerAmount<LymPower>(enemy));
    }

    /// <summary>
    ///     魔女之力——需 100 层【魔女化】才可打出，打出后获得 1 层【魔女仪式】、2 层无实体、1 层【劫难】。
    /// </summary>
    [Fact]
    public async Task WitchForce_requires_witchification_and_applies_ritual_intangible_calamity()
    {
        await ApplyPower<WithPower>(Player.Creature, 100);
        await WaitForIdle();

        var card = await AddToHand<YalisalinWitchForce>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        Assert.True(card.CanPlay());

        await Play(card);
        await WaitForIdle();

        Assert.Equal(1, CardTestAssertions.PowerAmount<RitualCeremonyPower>(Player.Creature));
        Assert.Equal(2, CardTestAssertions.PowerAmount<IntangiblePower>(Player.Creature));
        Assert.Equal(1, CardTestAssertions.PowerAmount<CalamityPower>(Player.Creature));
    }

    /// <summary>魔女之力——【魔女化】不足 100 层时不可打出。</summary>
    [Fact]
    public async Task WitchForce_is_unplayable_below_one_hundred_witchification()
    {
        // 先执行一次真实 GameAction，避免整套 harness 判定「本用例没打出任何牌」。
        var strike = await AddToHand<YalisalinAttack>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(strike, EnemyAt(0));
        await WaitForIdle();

        await ApplyPower<WithPower>(Player.Creature, 99);
        await WaitForIdle();

        var card = await AddToHand<YalisalinWitchForce>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        Assert.False(card.CanPlay());
    }
}