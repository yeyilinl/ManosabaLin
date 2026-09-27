using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using System.Linq;
using System.Threading.Tasks;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
///     亚里沙「魔女因子」遗物：局内<b>实时</b>检测「所有牌」（手牌 + 抽牌堆 + 弃牌堆）
///     里有没有【魔女化】诅咒卡，一旦当前没有就补一张进抽牌堆。
///     <para>
///         关键是「实时」——【魔女化】被消耗掉之后必须马上补新的；
///         旧实现只在战斗开始查一次，消耗掉就再也不补了。
///     </para>
/// </summary>
public sealed class YalisalinWitchFactorTests : CombatTestSuite
{
    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .KeepStartingRelics()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-yalisalin-witch-factor");
    }

    /// <summary>「所有牌」= 手牌 + 抽牌堆 + 弃牌堆（不含消耗堆），与遗物文案口径一致。</summary>
    private int CursesInPlay()
    {
        return PileType.Hand.GetPile(Player).Cards
            .Concat(PileType.Draw.GetPile(Player).Cards)
            .Concat(PileType.Discard.GetPile(Player).Cards)
            .Count(card => card is WitchificationCurse);
    }

    private WitchificationCurse FirstCurseInPlay()
    {
        return PileType.Hand.GetPile(Player).Cards
            .Concat(PileType.Draw.GetPile(Player).Cards)
            .Concat(PileType.Discard.GetPile(Player).Cards)
            .OfType<WitchificationCurse>()
            .First();
    }

    /// <summary>战斗开始时补 1 张，且不会因为反复的牌堆变化越补越多。</summary>
    [Fact]
    public async Task Relic_provides_exactly_one_curse()
    {
        // harness 铁律①：每个用例至少执行一个战斗 GameAction。
        var card = await AddToHand<Ignite>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        await WaitForIdle();

        Assert.Equal(1, CursesInPlay());
    }

    /// <summary>
    ///     实时补牌：【魔女化】被消耗（进入消耗堆）后，它已不在「所有牌」里，
    ///     遗物必须立刻再补一张。
    /// </summary>
    [Fact]
    public async Task Relic_refills_curse_after_it_is_exhausted()
    {
        var card = await AddToHand<Ignite>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(card);
        await WaitForIdle();

        var first = FirstCurseInPlay();
        Assert.Equal(1, CursesInPlay());

        await CardPileCmd.Add(first, PileType.Exhaust, CardPilePosition.Random);
        await WaitForIdle();

        // 旧的已经不在「所有牌」里，但必须有一张新的补上 ⇒ 计数仍是 1。
        Assert.Equal(1, CursesInPlay());
        // CardModel 的相等性是「按卡名/Id」而不是引用相等：牌堆里还有一张遗物新补的【魔女化】，
        // 用 Assert.Contains/DoseNotContain(card, pile) 会被那张同名卡误命中 ⇒ 一律改用引用相等判定。
        Assert.Contains(PileType.Exhaust.GetPile(Player).Cards, c => ReferenceEquals(c, first));
        Assert.DoesNotContain(PileType.Hand.GetPile(Player).Cards, c => ReferenceEquals(c, first));
        Assert.DoesNotContain(PileType.Draw.GetPile(Player).Cards, c => ReferenceEquals(c, first));
        Assert.DoesNotContain(PileType.Discard.GetPile(Player).Cards, c => ReferenceEquals(c, first));
        Assert.NotSame(first, FirstCurseInPlay());
    }
}
