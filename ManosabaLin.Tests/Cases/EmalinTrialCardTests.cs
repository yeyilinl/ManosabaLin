using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using ManosabaLin.Characters.Emalin.Enchantments;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 艾玛「审判」附魔相关卡的回归测试。
/// 重点核查：
/// 1. <c>EmalinCombatHelper</c> 的本回合「疑问牌」统计（含只有疑问关键字、没吃到附魔的牌）；
/// 2. <c>InvestigationPower</c> 检视的是抽牌堆<b>顶部</b>而不是底部。
/// </summary>
public sealed class EmalinTrialCardTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Emalin>()
            .AddEnemy<BigDummy>()
            .KeepStartingRelics()
            .WithSeed("manosaba-emalin-trial");
    }

    /// <summary>监狱的法则：本回合没打过疑问牌 → 只有基础伤害 6，且不施加易伤。</summary>
    [Fact]
    public async Task PrisonLaw_without_doubt_deals_base_damage()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<PrisonLaw>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(hpBefore - 6, enemy.CurrentHp);
        Assert.Null(enemy.GetPower<VulnerablePower>());
    }

    /// <summary>
    /// 监狱的法则：本回合打过 1 张带【疑问】附魔的牌 → 先给 2 层易伤，再结算 6 + 9 = 15 点伤害。
    /// 因为易伤在伤害之前施加，本次伤害会吃到 +50%（15 × 1.5 = 22.5，向下取整 22）。
    /// </summary>
    [Fact]
    public async Task PrisonLaw_after_doubt_play_applies_bonus()
    {
        var enemy = EnemyAt(0);

        // 造一张带【疑问】附魔的牌（Investigation 是能力牌，自身不改血量/护盾以外的状态）
        var doubtCard = await AddToHand<Investigation>();
        CardCmd.Enchant(ModelDb.Enchantment<Doubt>().ToMutable(), doubtCard, 1m);

        var law = await AddToHand<PrisonLaw>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        await Play(doubtCard);

        var hpBeforeLaw = enemy.CurrentHp;
        var blockBeforeLaw = enemy.Block;

        await Play(law, enemy);

        var dealt = (hpBeforeLaw - enemy.CurrentHp) + (enemy.Block - blockBeforeLaw);

        // 易伤必须先落到目标身上，且要在本次伤害结算之前（否则 dealt 会停在 15）
        Assert.Equal(2, enemy.GetPower<VulnerablePower>()?.Amount);
        Assert.Equal(22m, dealt);
    }

    /// <summary>
    /// 监狱的法则：只有疑问关键字、没吃到附魔的牌<b>不算</b>疑问牌（按设计如此）。
    /// 战斗中段入手的卡不会被 <c>EmaTrialBadge.EnchantAllTrialCards</c> 附魔，不该触发加伤与易伤。
    /// </summary>
    [Fact]
    public async Task PrisonLaw_ignores_keyword_doubt_card_without_enchantment()
    {
        var enemy = EnemyAt(0);

        var doubtCard = await AddToHand<AnnSketchbook>();
        Assert.True(EmalinKeywordRules.HasDoubtKeyword(doubtCard), "AnnSketchbook 应带疑问关键字");
        Assert.Null(doubtCard.Enchantment); // 战斗中段入手的卡不会被附魔

        var law = await AddToHand<PrisonLaw>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(doubtCard);

        var hpBeforeLaw = enemy.CurrentHp;
        var blockBeforeLaw = enemy.Block;

        await Play(law, enemy);

        var dealt = (hpBeforeLaw - enemy.CurrentHp) + (enemy.Block - blockBeforeLaw);

        Assert.Equal(6m, dealt);
        Assert.Null(enemy.GetPower<VulnerablePower>());
    }

    /// <summary>
    /// 监狱的法则：疑问牌是<b>上一回合</b>打的 → 本回合不该加伤、也不该给易伤。
    /// 用来确认 <c>HappenedThisTurn</c> 的回合隔离是有效的（而不是恒真）。
    /// </summary>
    [Fact]
    public async Task PrisonLaw_doubt_played_last_turn_gives_no_bonus()
    {
        var enemy = EnemyAt(0);

        var doubtCard = await AddToHand<Investigation>();
        CardCmd.Enchant(ModelDb.Enchantment<Doubt>().ToMutable(), doubtCard, 1m);

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(doubtCard);

        // 结束回合，疑问牌的记录应被隔离到上一回合
        await EndTurn();
        await WaitForIdle();

        var law = await AddToHand<PrisonLaw>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var hpBeforeLaw = enemy.CurrentHp;
        var blockBeforeLaw = enemy.Block;

        await Play(law, enemy);

        var dealt = (hpBeforeLaw - enemy.CurrentHp) + (enemy.Block - blockBeforeLaw);

        Assert.Equal(6m, dealt);
        Assert.Null(enemy.GetPower<VulnerablePower>());
    }

    /// <summary>
    /// 推理成立：本回合「每打过 1 <b>种</b>【审判】附魔牌」额外造成 1 段伤害。
    /// 同一种附魔打出 2 张也只算 1 段 —— 数的是种类，不是张数。
    /// </summary>
    [Fact]
    public async Task ReasoningEstablished_counts_enchantment_types_not_plays()
    {
        var enemy = EnemyAt(0);

        // 两张都吃【疑问】附魔：种类数 = 1，张数 = 2
        var firstDoubt = await AddToHand<Investigation>();
        CardCmd.Enchant(ModelDb.Enchantment<Doubt>().ToMutable(), firstDoubt, 1m);
        var secondDoubt = await AddToHand<Investigation>();
        CardCmd.Enchant(ModelDb.Enchantment<Doubt>().ToMutable(), secondDoubt, 1m);

        var reasoning = await AddToHand<ReasoningEstablished>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        await Play(firstDoubt);
        await Play(secondDoubt);
        await WaitForIdle();

        var hpBefore = enemy.CurrentHp;
        var blockBefore = enemy.Block;

        await Play(reasoning, enemy);

        var dealt = (hpBefore - enemy.CurrentHp) + (enemy.Block - blockBefore);

        // 4 基础 + 1 种 × 2 = 6。若错误地按张数计，会是 4 + 2 × 2 = 8。
        Assert.Equal(6m, dealt);
    }

    /// <summary>
    /// 搜查（InvestigationPower）：应在抽牌堆<b>顶部</b>弃牌，不能弃到底部的牌。
    /// 修 bug 之前实现用的是 <c>drawPile.Last()</c>（堆底），这个测试会失败。
    /// </summary>
    [Fact]
    public async Task Investigation_power_discards_top_card_not_bottom()
    {
        var power = await ApplyPower<InvestigationPower>(Player.Creature, 1);
        Assert.NotNull(power);

        // 往抽牌堆底部补牌，好让「顶部」和「底部」分得清（CardPile.Add 追加到列表末尾）
        for (var i = 0; i < 8; i++)
        {
            var filler = await AddToHand<Investigation>();
            await CardPileCmd.Add(filler, PileType.Draw);
        }

        var before = PileType.Draw.GetPile(Player).Cards.ToList();
        Assert.True(before.Count >= 10, $"抽牌堆只有 {before.Count} 张牌，无法区分顶部与底部");

        var bottom = before[^1];
        var topRegion = before.Take(9).ToList();

        await EndTurn();
        await WaitForIdle();

        var discard = PileType.Discard.GetPile(Player).Cards.ToList();

        // 注意：CardModel 的相等性是按 Id 比较的，同名卡会互相匹配，所以必须用引用相等判定
        Assert.DoesNotContain(discard, c => ReferenceEquals(c, bottom));
        Assert.Contains(discard, c => topRegion.Any(t => ReferenceEquals(t, c)));
    }

    /// <summary>
    /// 魔女审判（<c>Emamonv</c>）：选亲近/疏远后，生成一张带【审判】附魔、且<b>可以免费打出一次</b>的牌。
    /// 免费走 <c>SetToFreeThisTurn</c>（引擎里即「本回合或直到打出为止」）。
    /// </summary>
    [Fact]
    public async Task Emamonv_generates_trial_card_free_to_play_once()
    {
        var bond = await ApplyPower<BondPower>(Player.Creature, 1);
        Assert.NotNull(bond);
        var affinityBefore = bond!.Affinity;

        var handBefore = PileType.Hand.GetPile(Player).Cards.ToList();

        var card = await AddToHand<Emamonv>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        Assert.True(card.CanPlay());

        await Play(card);
        await WaitForIdle();

        // ① 自动选第一张 Token（亲近）→ 亲近 +1
        Assert.Equal(affinityBefore + 1, bond.Affinity);

        // ② 生成了一张带【审判】附魔的牌（赞同 / 反驳 / 疑问）
        var gift = PileType.Hand.GetPile(Player).Cards
            .Where(c => !handBefore.Any(h => ReferenceEquals(h, c)))
            .FirstOrDefault(c => c.Enchantment is Agreement or Rebuttal or Doubt);
        Assert.NotNull(gift);

        // ③ 该牌可以免费打出一次（当前费用解析为 0）
        Assert.Equal(0, gift!.EnergyCost.GetResolved());

        // ④ 费用为 0 ⇒ 把能量清空后它依然可打出
        await PlayerCmd.SetEnergy(0, Player);
        await WaitForIdle();
        Assert.True(gift.CanPlay());
    }
}
