using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 艾玛【魔女因子】即死结算的回归测试。
/// 重点核查：
/// 1. 常规情况（无任何限伤）——生命值到阈值即死亡，行为与改动前一致；
/// 2. 「坚硬外壳」这类强制限伤（<c>ModifyHpLostBeforeOstyLate</c>）会把「伤害 = 当前生命」
///    截断，即死必须仍然成立（不能被限伤吃掉）；
/// 3. 顺带确认限伤本身在 harness 里确实生效（否则第 2 条用例就是假通过）。
/// </summary>
public sealed class EmalinWitchFactorTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        // 放两只假人：击杀其中一只时战斗不会直接结束，避免无头环境的收尾噪音。
        battle
            .Player<Emalin>()
            .AddEnemy<BigDummy>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-emalin-witch-factor");
    }

    /// <summary>把假人压成 30/30 的小怪（默认 9999 血，阈值不方便对齐）。</summary>
    private Creature PrepareWeakEnemy()
    {
        var enemy = EnemyAt(0);
        enemy.SetMaxHpInternal(30);
        return enemy;
    }

    private async Task PlayPrisonLaw(Creature enemy)
    {
        var card = await AddToHand<PrisonLaw>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);
    }

    /// <summary>基线：无限伤时，攻击造成不受格挡伤害后生命值到阈值 ⇒ 直接死亡。</summary>
    [Fact]
    public async Task WitchFactor_kills_target_when_no_damage_cap()
    {
        var enemy = PrepareWeakEnemy();

        // 阈值 = 层数 = 30 ≥ 敌人当前生命值，保证触发「生命值等于或低于层数」
        await ApplyPower<EmaWitchFactorPower>(enemy, 30);
        await WaitForIdle();

        await PlayPrisonLaw(enemy);

        Assert.False(enemy.IsAlive);
        Assert.Equal(0, enemy.CurrentHp);
    }

    /// <summary>
    /// 核心回归：敌人身上有「坚硬外壳」（本回合最多只吃 1 点伤害）时，
    /// 即死那一击会被 <c>ModifyHpLostBeforeOstyLate</c> 截断成 0 —— 必须仍然死亡。
    /// </summary>
    [Fact]
    public async Task WitchFactor_kills_through_hardened_shell_cap()
    {
        var enemy = PrepareWeakEnemy();

        await ApplyPower<EmaWitchFactorPower>(enemy, 30);
        await ApplyPower<HardenedShellPower>(enemy, 1);
        await WaitForIdle();

        await PlayPrisonLaw(enemy);

        Assert.False(enemy.IsAlive);
        Assert.Equal(0, enemy.CurrentHp);
    }

    /// <summary>
    /// 对照：单放「坚硬外壳」、不放魔女因子时，6 点攻击伤害应被限成 1 点。
    /// 这条用来证明限伤在 harness 里真的生效，第 2 条用例不是假通过。
    /// </summary>
    [Fact]
    public async Task HardenedShell_alone_caps_incoming_damage_to_one()
    {
        var enemy = PrepareWeakEnemy();

        await ApplyPower<HardenedShellPower>(enemy, 1);
        await WaitForIdle();

        await PlayPrisonLaw(enemy);

        Assert.True(enemy.IsAlive);
        Assert.Equal(29, enemy.CurrentHp);
    }
}
