using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

public sealed class YalisalinSmokeTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-smoke");
    }

    [Fact]
    public async Task Yalisalin_character_loads_and_strike_deals_six_damage()
    {
        Assert.IsType<Yalisalin>(Player.Character);
        Assert.Equal(75, Player.Creature.MaxHp);

        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var strike = await AddToHand<YalisalinAttack>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(strike, enemy);

        Assert.Equal(hpBefore - 6, enemy.CurrentHp);
    }

    [Fact]
    public async Task Yalisalin_defend_gains_five_block()
    {
        var defend = await AddToHand<YalisalinDefend>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(defend);

        Assert.Equal(5, Player.Creature.Block);
    }
}