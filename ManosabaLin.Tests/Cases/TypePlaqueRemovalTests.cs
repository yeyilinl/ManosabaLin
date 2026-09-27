using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using ManosabaLin.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
///     类型牌匾（攻击 / 技能 / 能力）移除白名单的回归测试。
///     <para>
///         「卡面不显示卡牌类型」<b>不是</b> <c>CardRarity.Ancient</c> 自动带来的 ——
///         ancient 布局照样有类型牌匾，必须被列进
///         <see cref="LyXlTypePlaquePatch.RemovesTypePlaque" /> 才会被移除。
///         2026-09-25 就踩过这个坑：灭火 / 传火 / 夺火 改了「卡面只留风味文本」，
///         却漏了加白名单，卡面上仍然显示「能力 / 技能 / 攻击」。
///     </para>
/// </summary>
public sealed class TypePlaqueRemovalTests : CombatTestSuite
{
    /// <summary>「卡面只留风味文本」用的全透明文本背景图。</summary>
    private const string EmptyAncientTextBg = "ancient_empty_text_bg.png";

    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-type-plaque");
    }

    /// <summary>
    ///     ① 三张火卡在名单里；
    ///     ② 不变式：凡是「卡面只留风味文本」（<c>AncientTextBgPath</c> 指向全透明图）的卡，
    ///     都必须在这个名单里 —— 否则卡面上还会显示类型牌匾。
    /// </summary>
    [Fact]
    public async Task Every_flavor_text_only_card_removes_its_type_plaque()
    {
        // harness 铁律 ①：每个用例至少执行 1 个战斗 GameAction，否则整轮 runner 会被 TERMINATE 打断。
        // 本用例断言的是静态白名单，所以先随便打一张牌充当动作。
        var card = await AddToHand<LinkTheFire>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        await WaitForIdle();

        Assert.True(LyXlTypePlaquePatch.RemovesTypePlaque(ModelDb.Card<LinkTheFire>()));
        Assert.True(LyXlTypePlaquePatch.RemovesTypePlaque(ModelDb.Card<ExtinguishFlame>()));
        Assert.True(LyXlTypePlaquePatch.RemovesTypePlaque(ModelDb.Card<UsurpTheFlame>()));

        var offenders = ModelDb.AllCards
            .Where(c => c is ManosabaCardTemplate template &&
                        (template.AssetProfile.AncientTextBgPath ?? string.Empty)
                        .EndsWith(EmptyAncientTextBg, StringComparison.Ordinal))
            .Where(c => !LyXlTypePlaquePatch.RemovesTypePlaque(c))
            .Select(c => c.Id.Entry)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "以下卡「卡面只留风味文本」但没加进类型牌匾移除白名单（卡面会显示攻击/技能/能力）："
            + string.Join(", ", offenders));
    }
}
