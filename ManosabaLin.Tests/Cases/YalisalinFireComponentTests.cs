using ManosabaLin.Characters.Common.Powers;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Capabilities;
using ManosabaLin.Characters.Yalisalin.Cards;
using ManosabaLin.Characters.Yalisalin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using TestTheSpire;
using STS2RitsuLib.Models.Capabilities;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
/// 火组件卡测试（带发夹）。
/// 火组件卡：给手牌卡添加【火焰组件】，组件被烧毁时触发额外效果。
/// </summary>
public sealed class YalisalinFireComponentTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .KeepStartingRelics()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-firecomponent");
    }

    // 注：Glasshug/Unwantedkindness/Beforeforgiven/Fifthselfproof 之前因 headless 弹窗崩溃被移除，
    // 2026-09-09 已在测试 Entry 安装自动卡牌选择器（CardSelectCmd.UseSelector localOnly）修复，
    // 现在重新启用测试。

    [Fact]
    public async Task Glasshug_gives_eight_block()
    {
        var card = await AddToHand<Glasshug>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(8, Player.Creature.Block);
    }

    /// <summary>
    /// （升温组件运行时验证）——验证两件事：
    /// 1) capability 是否真正挂载到升温卡上（Burntthermometerpaper 等）。
    /// 2) 引擎运行时 LocString.GetIfExists 能否查到 afterBase/afterBaseStrong 键。
    /// 若 capability 没挂上 → 注册/挂载环节问题；若键查不到 → 本地化加载环节问题。
    /// </summary>
    [Fact]
    public async Task HeatWord_capability_mounted_and_loc_keys_resolve()
    {
        var card = await AddToHand<Burntthermometerpaper>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        // 1) capability 必须挂载
        Assert.True(card.TryGetCapability<YalisalinHeatWordCapability>(out var heat),
            "Burntthermometerpaper 没有挂载 YalisalinHeatWordCapability");
        Assert.NotNull(heat);

        // 2) 运行时本地化键必须可解析（这正是 GetDescriptionFragments 依赖的键）
        var afterBase = LocString.GetIfExists("cards",
            "MANOSABA_LIN_MODEL_CAPABILITY_YALISALIN_HEAT_WORD.afterBase");
        Assert.NotNull(afterBase);
        Assert.Equal("升温", afterBase.GetRawText());

        var afterBaseStrong = LocString.GetIfExists("cards",
            "MANOSABA_LIN_MODEL_CAPABILITY_YALISALIN_HEAT_WORD.afterBaseStrong");
        Assert.NotNull(afterBaseStrong);
        Assert.Equal("强升温", afterBaseStrong.GetRawText());

        // 末尾打出一次，满足 TestTheSpire 的"至少执行 1 个战斗动作"校验（否则整批 TERMINATE）。
        var enemy = EnemyAt(0);
        await PlayerCmd.SetEnergy(10, Player);
        await Play(card, enemy);
    }

    [Fact]
    public async Task Unwantedkindness_gives_seven_block()
    {
        var card = await AddToHand<Unwantedkindness>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(7, Player.Creature.Block);
    }

    /// <summary>
    /// （不需要的善意·回归）——打出后若本次通过余火烧掉了牌，应抽 1 张。
    /// 修复前：手动打出时源卡在 Play 堆，EnumerateModifierObjects（手/抽/弃）枚举不到源卡，
    /// Unwantedkindness.AfterFireComponentBurned 永不触发 → 「烧牌没抽牌」。
    /// 自动选择器固定选第一项（源卡）→ 连接牌被烧 → 修复后应抽 1。
    /// </summary>
    [Fact]
    public async Task Unwantedkindness_burns_card_and_draws_one()
    {
        var card = await AddToHand<Unwantedkindness>();
        await AddToHand<YalisalinDefend>(); // 连接牌候选（会被烧）

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();

        var handBefore = PileType.Hand.GetPile(Player).Cards.ToArray();

        await Play(card);

        // 被烧的连接牌进入消耗堆
        var exhaust = PileType.Exhaust.GetPile(Player).Cards.ToArray();
        Assert.Single(exhaust);

        var linkedWasInHand = handBefore.Contains(exhaust[0]);

        // 修复后：手牌 = 原手牌 - 打出的本卡 - 从手牌烧掉的连接牌(若在手里) + 抽 1
        var expectedHand = handBefore.Length - 1 - (linkedWasInHand ? 1 : 0) + 1;
        Assert.Equal(expectedHand, PileType.Hand.GetPile(Player).Cards.Count);
    }

    /// <summary>
    /// （被原谅之前）——12 基础伤害；本次余火成功烧牌后额外 8 伤。
    /// 自动选择器固定选原牌 → 连接牌被烧 → 合计 12+8=20。
    /// 2026-09-14 修复源卡回调缺失（EnumerateModifierObjects 漏掉 Play 堆中的源卡）后，
    /// AfterFireComponentBurned 真正生效；旧断言 12 伤是 bug 时代的产物。
    /// </summary>
    [Fact]
    public async Task Beforeforgiven_deals_twelve_damage()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<Beforeforgiven>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(hpBefore - 20, enemy.CurrentHp);
    }

    [Fact]
    public async Task Burnedecho_deals_six_damage_without_burn_count()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<Burnedecho>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        // 本回合无组件燃烧记录 → 6 伤
        Assert.Equal(hpBefore - 6, enemy.CurrentHp);
    }

    [Fact]
    public async Task Fifthselfproof_deals_no_damage_without_fire_use_count()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<Fifthselfproof>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        // FireUseCount=0 → 无额外组件效果，本卡无基础伤害 → 敌人血量不变
        Assert.Equal(hpBefore, enemy.CurrentHp);
    }

    [Fact]
    public async Task Holdmypain_plays_without_error()
    {
        var card = await AddToHand<Holdmypain>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
    }

    [Fact]
    public async Task Dazzlingtolerance_plays_without_error()
    {
        var card = await AddToHand<Dazzlingtolerance>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
    }

    [Fact]
    public async Task Burnedapology_plays_without_error()
    {
        var card = await AddToHand<Burnedapology>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
    }

    [Fact]
    public async Task Unneededgoodchild_plays_without_error()
    {
        var card = await AddToHand<Unneededgoodchild>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
    }

    [Fact]
    public async Task Twoseparatedends_plays_without_error()
    {
        var card = await AddToHand<Twoseparatedends>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
    }

    [Fact]
    public async Task Warmthshouldnotstay_plays_without_error()
    {
        var card = await AddToHand<Warmthshouldnotstay>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
    }

    /// <summary>（归还的发带）——5 格挡 + 5 伤；弃牌堆无卡可回收时选择跳过。</summary>
    [Fact]
    public async Task Returnedhairribbon_gives_block_and_damage()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<Returnedhairribbon>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(5, Player.Creature.Block);
        Assert.Equal(hpBefore - 5, enemy.CurrentHp);
    }

    /// <summary>（不要看我）——手牌无候选（仅自身）时选择跳过，不崩溃。</summary>
    [Fact]
    public async Task Dontlookatme_plays_without_error()
    {
        var card = await AddToHand<Dontlookatme>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.True(true);
    }

    /// <summary>（破碎的信任）——6 伤；手牌无候选时选择跳过。</summary>
    [Fact]
    public async Task Brokentrust_deals_six_damage()
    {
        var enemy = EnemyAt(0);
        var hpBefore = enemy.CurrentHp;
        var card = await AddToHand<Brokentrust>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card, enemy);

        Assert.Equal(hpBefore - 6, enemy.CurrentHp);
    }

    /// <summary>（静止也会痛）——手牌仅自身时 FromHand 无候选，跳过不崩溃。</summary>
    [Fact]
    public async Task Stayingstillhurts_plays_without_error()
    {
        var card = await AddToHand<Stayingstillhurts>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.True(true);
    }

    /// <summary>（不要带我回家）——10 格挡；手牌无候选时选择跳过。</summary>
    [Fact]
    public async Task Dontbringmehome_gives_ten_block()
    {
        var card = await AddToHand<Dontbringmehome>();

        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);

        Assert.Equal(10, Player.Creature.Block);
    }
}