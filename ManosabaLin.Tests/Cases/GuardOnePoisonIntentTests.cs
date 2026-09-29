using System.Threading.Tasks;
using ManosabaLin.Characters.Hiro;
using ManosabaLin.Characters.Hiro.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 残骸首领（<see cref="GuardOneMonster" />）阶段一「上毒」意图的二选一判定：
/// 目标已有中毒 ⇒ 改给 1 层虚弱；目标没有中毒 ⇒ 照常上毒。
/// </summary>
/// <remarks>
///     行动顺序由状态机写死为 <c>ATTACK_MOVE → POISON_MOVE</c>，
///     所以「结束两个玩家回合」就必然走到上毒意图，不需要额外的随机控制。
///     ⚠️ 中毒按「每经过一个玩家回合衰减 1 点」结算，因此怪物刚上 6 层时读到的会是 5，
///     断言中毒层数时要留出这个衰减量。
/// </remarks>
public sealed class GuardOnePoisonIntentTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Hiro>()
            .AddEnemy<GuardOneMonster>()
            .WithSeed("manosaba-guardone-poison-intent");
    }

    /// <summary>把玩家回合推进到怪物的「上毒」意图结算完。</summary>
    private async Task AdvanceToPoisonMove()
    {
        // 第 1 个玩家回合结束 ⇒ 怪物 ATTACK_MOVE
        await EndTurn();
        // 第 2 个玩家回合结束 ⇒ 怪物 POISON_MOVE
        await EndTurn();
    }

    /// <summary>玩家已经有中毒 ⇒ 本次上毒意图改为给 1 层虚弱，且不再叠毒。</summary>
    [Fact]
    public async Task Poison_move_grants_weak_when_player_already_poisoned()
    {
        // 预置中毒（每过一个玩家回合衰减 1 点，所以只断言「仍然 > 0」而不是具体层数）。
        await ApplyPower<PoisonPower>(Player.Creature, 5);

        await AdvanceToPoisonMove();

        var weak = Player.Creature.GetPower<WeakPower>();
        Assert.NotNull(weak);
        Assert.Equal(1m, weak!.Amount);

        var poison = Player.Creature.GetPower<PoisonPower>();
        Assert.True(poison is { Amount: > 0 }, "已有中毒时不应再叠毒，但原有中毒也不该被清掉。");
    }

    /// <summary>玩家没有中毒 ⇒ 照常上毒，且不给虚弱。</summary>
    [Fact]
    public async Task Poison_move_applies_poison_when_player_has_none()
    {
        await AdvanceToPoisonMove();

        var poison = Player.Creature.GetPower<PoisonPower>();
        Assert.NotNull(poison);
        // 上毒 6 层，但结算完已经过了一个玩家回合（中毒衰减 1 点），所以读到 5。
        Assert.InRange(poison!.Amount, 5m, 6m);

        Assert.Null(Player.Creature.GetPower<WeakPower>());
    }
}
