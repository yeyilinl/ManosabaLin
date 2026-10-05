using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common.Powers;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     羁绊漂移（<see cref="HextechBondDrift" />）左右移动时触发的效果。
///     <para>左移：所有队友（不含自己）各自获得一张其角色卡池的【多人】卡。</para>
///     <para>
///         右移：从所有队友的手牌 / 抽牌堆 / 弃牌堆里随机移除一张【多人】卡；成功则其主人获得
///         1 层「下一张多人卡费用 −1」（<see cref="HextechMultiplayerDiscountPower" />，可叠加，打出即整层清空）。
///     </para>
///     <para>
///         ⚠️ 一切「随机」都走 <c>RunState.Rng.*</c>（联机口径）：「给牌」用 <c>CombatCardGeneration</c>、
///         「挑牌移除」用 <c>CombatCardSelection</c>，与项目里既有的发牌/挑牌写法一致。
///     </para>
/// </summary>
internal static class HextechBondDriftEffects
{
    /// <summary>左移：给每个队友一张【多人】卡。</summary>
    internal static async Task OnShiftedLeft(Player owner)
    {
        if (owner.Creature.CombatState is not { } combatState) return;

        foreach (var teammate in combatState.Players
                     .Where(p => p.Creature != owner.Creature && p.Creature.IsAlive))
            await GiftMultiplayerCard(owner, combatState, teammate);
    }

    /// <summary>右移：随机移除一名队友的一张【多人】卡，并给其主人挂 1 层减费。</summary>
    internal static async Task OnShiftedRight(Player owner)
    {
        if (owner.Creature.CombatState is not { } combatState) return;

        var candidates = combatState.Players
            .Where(p => p.Creature != owner.Creature)
            .SelectMany(static p => new[] { PileType.Hand, PileType.Draw, PileType.Discard }
                .SelectMany(pileType => pileType.GetPile(p).Cards))
            .Where(static card => card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly)
            .Where(static card => !card.HasBeenRemovedFromState)
            .Distinct()
            .ToArray();

        if (candidates.Length == 0) return;

        var removed = owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (removed?.Owner is not { } victim) return;

        await CardPileCmd.RemoveFromCombat(removed);

        // 「下一次使用的多人卡费用减一」：挂在队友身上，可叠加（Counter 层数 = 减费额度）。
        var choiceContext = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<HextechMultiplayerDiscountPower>(
            choiceContext, victim.Creature, 1m, owner.Creature, null, false);
    }

    /// <summary>从该队友的角色卡池里随机抽一张【多人】卡，生成到手牌。</summary>
    private static async Task GiftMultiplayerCard(
        Player giver, ICombatState combatState, Player teammate)
    {
        // 「其角色卡池」＝ 该队友自己所玩角色的卡池。
        var pool = teammate.Character.CardPool;
        if (pool is null) return;

        var candidates = pool
            .GetUnlockedCards(teammate.UnlockState, teammate.RunState.CardMultiplayerConstraint)
            .Where(static card => card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly)
            .Where(static card => card.CanBeGeneratedInCombat)
            .ToArray();

        if (candidates.Length == 0) return;

        var canonical = giver.RunState.Rng.CombatCardGeneration.NextItem(candidates);
        if (canonical is null) return;

        var gift = combatState.CreateCard(canonical, teammate);
        await CardPileCmd.AddGeneratedCardToCombat(gift, PileType.Hand, teammate);
    }
}
