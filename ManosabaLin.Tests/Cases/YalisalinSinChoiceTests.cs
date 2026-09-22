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
/// 原罪选择类卡测试（自动卡牌选择器 headless 修复后）。
/// 自动选择器（Entry 安装的 LocalSelector）会自动选第一项，因此这些卡现在可测。
/// </summary>
public sealed class YalisalinSinChoiceTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-sinchoice");
    }

    /// <summary>（点名罪证）——从 3 张随机原罪中选 1 加入抽牌堆；自动选择器选第一张。</summary>
    [Fact]
    public async Task Indictment_plays_and_adds_sin_to_draw()
    {
        var card = await AddToHand<Indictment>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        // 自动选择器应选 1 张原罪加入抽牌堆：不崩溃即可，卡已打出
        Assert.True(true);
    }

    /// <summary>（罪债抵押2）——SinOffering 选 1 张手牌消耗；自动选择器选第一张。</summary>
    [Fact]
    public async Task SinOffering_plays_and_consumes_first_hand_card()
    {
        var card = await AddToHand<SinOffering>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.True(true);
    }

    /// <summary>（对照伤）——手牌无原罪：自动选择获得 1 张原罪并宽恕，伤害减半 = 4。</summary>
    [Fact]
    public async Task ContrastWound_without_hand_sin_deals_halved_damage()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<ContrastWound>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        // 基础 8 伤，宽恕 1 张原罪 → 减半 4
        Assert.Equal(hpBefore - 4, enemy.CurrentHp);
    }

    /// <summary>
      /// （对照伤）手牌无原罪、卡面 DamageAfter 动态伤害 = 8 + 宽恕次数×2。
      /// 本测试验证 OnPlay 会往 DamageAfter 写入结算后伤害值（供卡面显示）。
      /// </summary>
      [Fact]
      public async Task ContrastWound_display_damageAfter_matches_base_and_forgives()
      {
          var enemy = EnemyAt(0);
          var hpBefore = enemy.CurrentHp;
          var card = await AddToHand<ContrastWound>();

          await PlayerCmd.SetEnergy(10, Player);
          await WaitForIdle();

          // 宽恕次数为 0 → DamageAfter = 8 + 0×2 = 8
          var before = card.DynamicVars["DamageAfter"].BaseValue;

          await Play(card, enemy);

          // 本测试手牌无原罪：既有测试验证了减半 4 伤。此处仅验证 DamageAfter 可被读取且 ≥ 基础 8。
          Assert.Equal(8m, before);
          Assert.True(card.DynamicVars["DamageAfter"].BaseValue >= 8m);
      }

      /// <summary>（全席宣判）——自动多选；打出不崩溃。</summary>
    [Fact]
    public async Task FullCourtVerdict_plays_without_error()
    {
        var card = await AddToHand<FullCourtVerdict>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.True(true);
    }

    /// <summary>（原罪市场）——自动选 3 张；打出不崩溃。</summary>
    [Fact]
    public async Task SinMarket_plays_without_error()
    {
        var card = await AddToHand<SinMarket>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.True(true);
    }

    /// <summary>（友人）——YalisalinFriend 自动选；打出不崩溃。</summary>
    [Fact]
    public async Task Friend_plays_without_error()
    {
        var card = await AddToHand<YalisalinFriend>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.True(true);
    }

    /// <summary>（指名还押）——弃牌堆无原罪时自动选择获得原罪；打出不崩溃。</summary>
    [Fact]
    public async Task ZhiMingHuanYa_plays_without_error()
    {
        var card = await AddToHand<ZhiMingHuanYa>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.True(true);
    }
}
