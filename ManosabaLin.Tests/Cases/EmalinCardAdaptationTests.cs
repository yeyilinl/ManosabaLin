using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Emalin.Enchantments;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 艾玛（Emalin）四张「效果改到与本地化一致」的卡的回归测试：
/// 坏掉的门锁（<c>BrokenDoorLock</c>）、失控的恨意（<c>EmalinRestCeremony</c>）、
/// 便签条（<c>Data</c>）、替身的温柔（<c>SwapBodySuccess</c>）。
/// </summary>
public sealed class EmalinCardAdaptationTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Emalin>()
            .AddEnemy<BigDummy>()
            .KeepStartingRelics()
            .WithSeed("manosaba-emalin-adaptation");
    }

    // ── 坏掉的门锁：造成 8 伤害；若这一击有伤害落在格挡上，给 1 层易伤 ─────────────

    /// <summary>坏掉的门锁：目标没有格挡 → 8 点全打到血量，不给易伤。</summary>
    [Fact]
    public async Task BrokenDoorLock_no_block_deals_damage_and_no_vulnerable()
    {
        var enemy = EnemyAt(0);
        Assert.Equal(0, enemy.Block);

        var card = await AddToHand<BrokenDoorLock>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var hpBefore = enemy.CurrentHp;
        await Play(card, enemy);
        await WaitForIdle();

        Assert.Equal(hpBefore - 8, enemy.CurrentHp);
        Assert.Null(enemy.GetPower<VulnerablePower>());
    }

    /// <summary>
    /// 坏掉的门锁：目标有 20 格挡 → 8 点被格挡吃掉（血量不变、格挡 −8），
    /// 因为「有伤害落在格挡上」所以给 1 层易伤。
    /// </summary>
    [Fact]
    public async Task BrokenDoorLock_applies_vulnerable_when_damage_hits_block()
    {
        var enemy = EnemyAt(0);

        await CreatureCmd.GainBlock(enemy, 20m, ValueProp.Unpowered, null);
        await WaitForIdle();
        var blockBefore = enemy.Block;
        Assert.True(blockBefore >= 8, $"格挡只有 {blockBefore}，不足以吃掉这一击");

        var card = await AddToHand<BrokenDoorLock>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var hpBefore = enemy.CurrentHp;
        await Play(card, enemy);
        await WaitForIdle();

        Assert.Equal(hpBefore, enemy.CurrentHp);           // 全被格挡吃掉
        Assert.Equal(blockBefore - 8, enemy.Block);
        Assert.Equal(1, enemy.GetPower<VulnerablePower>()?.Amount);
    }

    // ── 便签条：抽 4 张；手牌里的疑问附魔牌可以免费打出一次 ───────────────────────

    /// <summary>
    /// 便签条：把「当下付不起」的疑问附魔牌也设成免费（卡面不带 CanPlay 前提）。
    /// 打出便签条后能量恰好为 0，若还留着 <c>CanPlay()</c> 过滤，这张 1 费牌就不会被设成免费。
    /// </summary>
    [Fact]
    public async Task Data_makes_unaffordable_doubt_card_free()
    {
        var doubtCard = await AddToHand<Investigation>();   // 1 费能力牌
        CardCmd.Enchant(ModelDb.Enchantment<Doubt>().ToMutable(), doubtCard, 1m);
        Assert.Equal(1, doubtCard.EnergyCost.GetResolved());

        var data = await AddToHand<Data>();

        await PlayerCmd.SetEnergy(1, Player);               // 恰好够打便签条，打完变 0
        await WaitForIdle();
        await Play(data);
        await WaitForIdle();

        Assert.Equal(0, Player.PlayerCombatState.Energy);
        Assert.Equal(0, doubtCard.EnergyCost.GetResolved());
    }

    // ── 失控的恨意：每打过 1 种【审判】附魔牌 +13 伤害，并随机给敌方全体 1 层易伤或虚弱 ──

    /// <summary>失控的恨意：本回合打过 1 种【审判】附魔牌 → 13 + 13 = 26，且敌方全体随机吃 1 层易伤或虚弱（恰好一种）。</summary>
    [Fact]
    public async Task EmalinRestCeremony_one_type_adds_damage_and_random_debuff()
    {
        var enemy = EnemyAt(0);

        var doubtCard = await AddToHand<Investigation>();
        CardCmd.Enchant(ModelDb.Enchantment<Doubt>().ToMutable(), doubtCard, 1m);

        var ceremony = await AddToHand<EmalinRestCeremony>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(doubtCard);                              // TargetType.Self ⇒ 不带目标
        await WaitForIdle();

        var hpBefore = enemy.CurrentHp;
        var blockBefore = enemy.Block;

        await Play(ceremony, enemy);
        await WaitForIdle();

        var dealt = (hpBefore - enemy.CurrentHp) + (enemy.Block - blockBefore);
        Assert.Equal(26m, dealt);                           // 13 + 1 种 × 13

        var vulnerable = enemy.GetPower<VulnerablePower>()?.Amount ?? 0;
        var weak = enemy.GetPower<WeakPower>()?.Amount ?? 0;
        Assert.Equal(1, vulnerable + weak);                 // 恰好 1 层，只会有一种
        Assert.True((vulnerable == 1) ^ (weak == 1), $"易伤={vulnerable} 虚弱={weak}，应恰好命中一种");
    }

    /// <summary>
    /// 失控的恨意：触发次数 = 「伤害 +13」的次数（= 本回合打过的【审判】附魔种数）。
    /// 2 种附魔 ⇒ 13 + 2×13 = 39，且敌人共吃 2 层 debuff（随机分配，可能全一种、也可能 1+1）。
    /// </summary>
    [Fact]
    public async Task EmalinRestCeremony_triggers_random_debuff_once_per_enchantment_type()
    {
        var enemy = EnemyAt(0);

        // 两种不同的【审判】附魔：Doubt（疑问）与 Rebuttal（反驳）
        var doubtCard = await AddToHand<Investigation>();
        CardCmd.Enchant(ModelDb.Enchantment<Doubt>().ToMutable(), doubtCard, 1m);
        var rebuttalCard = await AddToHand<Investigation>();
        CardCmd.Enchant(ModelDb.Enchantment<Rebuttal>().ToMutable(), rebuttalCard, 1m);

        var ceremony = await AddToHand<EmalinRestCeremony>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(doubtCard);                              // 两张都是 Self ⇒ 不带目标
        await Play(rebuttalCard);
        await WaitForIdle();

        // 前提：这两种附魔自己不会给敌人上易伤/虚弱（否则下面的期望值会飘）
        Assert.Null(enemy.GetPower<VulnerablePower>());
        Assert.Null(enemy.GetPower<WeakPower>());

        var hpBefore = enemy.CurrentHp;
        var blockBefore = enemy.Block;

        await Play(ceremony, enemy);
        await WaitForIdle();

        var dealt = (hpBefore - enemy.CurrentHp) + (enemy.Block - blockBefore);
        Assert.Equal(39m, dealt);                           // 13 + 2 种 × 13

        var vulnerable = enemy.GetPower<VulnerablePower>()?.Amount ?? 0;
        var weak = enemy.GetPower<WeakPower>()?.Amount ?? 0;
        // 每种附魔掷一次 ⇒ 共 2 层；伤害已结算完，所以 2 层不会被 ×1.5 放大
        Assert.Equal(2, vulnerable + weak);
    }

    /// <summary>失控的恨意：本回合没打过【审判】附魔牌 → 只有 13 基础伤害，且不给任何 debuff。</summary>
    [Fact]
    public async Task EmalinRestCeremony_without_enchantment_has_no_debuff()
    {
        var enemy = EnemyAt(0);
        var ceremony = await AddToHand<EmalinRestCeremony>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var hpBefore = enemy.CurrentHp;
        var blockBefore = enemy.Block;

        await Play(ceremony, enemy);
        await WaitForIdle();

        var dealt = (hpBefore - enemy.CurrentHp) + (enemy.Block - blockBefore);
        Assert.Equal(13m, dealt);
        Assert.Null(enemy.GetPower<VulnerablePower>());
        Assert.Null(enemy.GetPower<WeakPower>());
    }

    // ── 替身的温柔：获得 Cards 张「友方目标牌池」的零费牌（升级 1 → 2 张） ────────

    /// <summary>替身的温柔（未升级）：获得 1 张来自目标牌池的牌，且该牌费用为 0。</summary>
    [Fact]
    public async Task SwapBodySuccess_gains_one_free_card_base()
    {
        var card = await AddToHand<SwapBodySuccess>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var before = PileType.Hand.GetPile(Player).Cards.ToList();

        await Play(card);                                   // 单人 ⇒ 目标回落到自己
        await WaitForIdle();

        var added = PileType.Hand.GetPile(Player).Cards
            .Where(c => !before.Any(b => ReferenceEquals(b, c)))
            .ToList();

        Assert.Single(added);
        Assert.Equal(0, added[0].EnergyCost.GetResolved());
    }

    /// <summary>替身的温柔（已升级）：获得 2 张来自目标牌池的牌，且费用都为 0。</summary>
    [Fact]
    public async Task SwapBodySuccess_upgraded_gains_two_free_cards()
    {
        var card = await AddToHand<SwapBodySuccess>();
        CardCmd.Upgrade(card);
        Assert.True(card.IsUpgraded);

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var before = PileType.Hand.GetPile(Player).Cards.ToList();

        await Play(card);
        await WaitForIdle();

        var added = PileType.Hand.GetPile(Player).Cards
            .Where(c => !before.Any(b => ReferenceEquals(b, c)))
            .ToList();

        Assert.Equal(2, added.Count);
        Assert.All(added, c => Assert.Equal(0, c.EnergyCost.GetResolved()));
    }
}
