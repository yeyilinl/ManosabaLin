using ManosabaLin.Characters.Sherrylin;
using ManosabaLin.Characters.Sherrylin.Cards;
using ManosabaLin.Characters.Sherrylin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Monsters;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 雪莉卡牌回归测试。
/// </summary>
public sealed class SherrylinCardTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Sherrylin>()
            .AddEnemy<BigDummy>()
            .KeepStartingRelics()
            .WithSeed("manosaba-sherrylin-cards");
    }

    /// <summary>
    /// 被称为冲击波的拳风：带 <c>RemoveOnPlayComponent</c>（卡面【移除】提示），
    /// 打出后从战斗中移除 —— 不再是任何牌堆的成员（<c>CardModel.Pile</c> 为 null）。
    /// 加组件之前实现在打出后会回到弃牌堆，这个测试会失败。
    /// </summary>
    [Fact]
    public async Task ShockwaveFist_is_removed_from_combat_after_play()
    {
        var enemy = EnemyAt(0);
        var card = await AddToHand<ShockwaveFist>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        Assert.True(card.CanPlay());
        Assert.NotNull(card.Pile);

        await Play(card, enemy);
        await WaitForIdle();

        Assert.Null(card.Pile);
    }
}
