using ManosabaLin.Characters.Common.Powers;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 【火之时代】（<c>AgeOfFire</c>）的三张分支牌：传火 / 灭火 / 夺火。
/// 这三张牌卡面只留风味文本（灭火 1、传火 2、夺火 3），效果改走悬浮提示
/// —— 与「我不听，我需要你」同款（<c>AncientTextBgPath</c> 用全透明图 ⇒ 无黑色遮罩，
/// <c>CardRarity.Ancient</c> ⇒ ancient 布局）。
/// </summary>
public sealed class YalisalinFlameBranchTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-flame-branch");
    }

    /// <summary>
    /// 传火（<c>LinkTheFire</c>）：8 点格挡；卡面效果改走悬浮提示 ——
    /// 提示要能挂上，且文案必须能解析出数值（键写错、或占位符没有对应变量，
    /// 都会在 <c>HoverTip</c> 构造时 <c>GetFormattedText</c> 直接抛异常）。
    /// </summary>
    [Fact]
    public async Task LinkTheFire_gains_block_and_exposes_effect_hover_tip()
    {
        var card = await AddToHand<LinkTheFire>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        await Play(card);
        await WaitForIdle();

        Assert.Equal(8, Player.Creature.Block);

        var tip = EffectTipDescription(card, "MANOSABA_LIN_CARD_LINK_THE_FIRE_EFFECT");
        Assert.NotNull(tip);
        Assert.Contains("8", tip!);
    }

    /// <summary>夺火（<c>UsurpTheFlame</c>）：9 点伤害；卡面效果改走悬浮提示。</summary>
    [Fact]
    public async Task UsurpTheFlame_deals_damage_and_exposes_effect_hover_tip()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;

        var card = await AddToHand<UsurpTheFlame>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var tip = EffectTipDescription(card, "MANOSABA_LIN_CARD_USURP_THE_FLAME_EFFECT");
        Assert.NotNull(tip);
        Assert.Contains("9", tip!);

        await Play(card, enemy);
        await WaitForIdle();

        Assert.Equal(hpBefore - 9, enemy.CurrentHp);
    }

    /// <summary>
    /// 灭火（<c>ExtinguishFlame</c>）：打出后自己获得 12 层【灭火】；
    /// 卡面效果也改走悬浮提示（与另两张分支牌对称）。
    /// </summary>
    [Fact]
    public async Task ExtinguishFlame_applies_extinguish_power_of_twelve()
    {
        var card = await AddToHand<ExtinguishFlame>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        await Play(card);
        await WaitForIdle();

        Assert.Equal(12, CardTestAssertions.PowerAmount<ExtinguishFlamePower>(Player.Creature));

        var tip = EffectTipDescription(card, "MANOSABA_LIN_CARD_EXTINGUISH_FLAME_EFFECT");
        Assert.NotNull(tip);
        Assert.Contains("12", tip!);
    }

    /// <summary>
    /// 火之时代（<c>AgeOfFire</c>）：打出后三张分支牌入手，
    /// 并且卡面悬浮提示给的是这三张牌的<b>效果</b>提示框（不再是只会显示「1/2/3」的牌面预览）。
    /// </summary>
    [Fact]
    public async Task AgeOfFire_adds_three_branches_and_shows_their_effect_tips()
    {
        var handBefore = PileType.Hand.GetPile(Player).Cards.ToList();

        var card = await AddToHand<AgeOfFire>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        await Play(card);
        await WaitForIdle();

        var added = PileType.Hand.GetPile(Player).Cards
            .Where(c => !handBefore.Any(h => ReferenceEquals(h, c)))
            .ToList();

        Assert.Contains(added, c => c is LinkTheFire);
        Assert.Contains(added, c => c is ExtinguishFlame);
        Assert.Contains(added, c => c is UsurpTheFlame);

        var linkTip = EffectTipDescription(card, "MANOSABA_LIN_CARD_LINK_THE_FIRE_EFFECT");
        var extinguishTip = EffectTipDescription(card, "MANOSABA_LIN_CARD_EXTINGUISH_FLAME_EFFECT");
        var usurpTip = EffectTipDescription(card, "MANOSABA_LIN_CARD_USURP_THE_FLAME_EFFECT");

        Assert.NotNull(linkTip);
        Assert.NotNull(extinguishTip);
        Assert.NotNull(usurpTip);

        Assert.Contains("8", linkTip!);
        Assert.Contains("12", extinguishTip!);
        Assert.Contains("9", usurpTip!);
    }

    /// <summary>
    /// 取「卡面效果悬浮提示」的已解析描述文本；没挂上则返回 <c>null</c>。
    /// <c>CardModel.HoverTips</c> 是公开的（<c>AdditionalHoverTips</c> 是 protected），
    /// 提示的 <c>Id</c> 里带着本地化键，用它定位。
    /// </summary>
    private static string? EffectTipDescription(CardModel card, string locEntry)
    {
        foreach (var tip in card.HoverTips)
            if (tip is HoverTip hoverTip && tip.Id.Contains(locEntry, StringComparison.Ordinal))
                return hoverTip.Description;

        return null;
    }
}
