using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using ManosabaLin.Characters.Common.HiroKeywords;
using ManosabaLin.Characters.Common.LinRelics;
using ManosabaLin.Characters.Hiro.Powers;
using ManosabaLin.Characters.Sherrylin.Orbs;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ManosabaLin.Patches;

// 联动遗物 2：球位满时的两种走法。
//   Channel 在容量满时先 EvokeNext(队首) 再入队 —— 这里在它动手之前判一次：
//   ① 队首是「持续型」情绪球（悲伤/愤怒/快乐/怅然/雀跃/好奇/友谊）且该玩家有遗物 2 ⇒
//      **不让它被挤掉**，而是把它「脱离球位」：从模型队列摘下 + 播一次离场动画，
//      再登记进 HangingEmotionOrbs（挂到血条下方，靠 SubscribeForCombatStateHooks 继续收钩子）。
//      ⚠️ 效果一点没搬、也没建任何能力 —— 挂着的还是那颗球自己（用户 2026-09-29 裁定）。
//   ② 其余情况 ⇒ 正常被挤出，把队首记成「刚被挤掉的球」，供 HextechEmotionOverflow
//      在 AfterOrbEvoked 里判断这一次激发是不是被挤掉的（反伤型/延迟型靠它）。
[HarmonyPatch(typeof(OrbCmd), "Channel", [typeof(PlayerChoiceContext), typeof(OrbModel), typeof(Player)])]
internal static class HextechOrbOverflowPatch
{
    [HarmonyPrefix]
    private static void Prefix(Player player)
    {
        var queue = player?.PlayerCombatState?.OrbQueue;
        if (queue is null) return;
        if (queue.Orbs.Count < queue.Capacity) return;
        if (queue.Orbs.Count == 0) return;

        var front = queue.Orbs.First();

        if (front is IEmotionOrb frontEmotion
            && HextechEmotionOverflow.IsActiveFor(player)
            && EmotionOverflowRules.IsPersistent(frontEmotion.GetEmotionCard())
            && queue.Remove(front))
        {
            try
            {
                // 视觉上让它离场（顺带把空位补回队尾，保证接下来 AddOrbAnim 能找到空位）。
                NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.EvokeOrbAnim(front);
            }
            catch (Exception ex)
            {
                MainFile.Logger.Warn($"[HextechEmotionOverflow] orb leave anim failed: {ex.Message}");
            }

        HangingEmotionOrbs.Hang(player, front);
        return;
    }

    // 只有「有溢出结算」的情绪球才记账；其余（非情绪球 / 魔女化球 / 无遗物时的持续型球）正常挤出，
    // 不占用标记 —— 避免标记被无关球消费或残留，导致新球被误判成「被挤出」（吞卡）。
    if (front is IEmotionOrb payoutEmotion
        && HextechEmotionOverflow.IsActiveFor(player)
        && EmotionOverflowRules.HasOverflowPayout(payoutEmotion.GetEmotionCard()))
    {
        HextechOrbEvokeRules.MarkSqueezedOut(front);
    }
}
}

// 联动遗物 4：把「打出轮回卡」的结算整体换成「抽牌堆里任意两张轮回卡」。
[HarmonyPatch(typeof(TransmigrationSingleton), nameof(TransmigrationSingleton.AfterCardPlayed))]
internal static class HextechTransmigrationEchoPatch
{
    [HarmonyPrefix]
    private static bool Prefix(PlayerChoiceContext context, CardPlay cardPlay, ref Task __result)
    {
        if (cardPlay.IsAutoPlay) return true;

        var card = cardPlay.Card;
        if (card is null) return true;
        if (!TransmigrationRules.HasTransmigration(card)) return true;

        var relic = card.Owner?.Relics.OfType<HextechTransmigrationEcho>().FirstOrDefault();
        if (relic is null) return true;

        __result = relic.PlayRandomTransmigrationCards(context, cardPlay);
        return false;
    }
}

// 联动遗物 5（一）：让「付得起」的卡在 UI / CanPlay 层面变成可打出。
[HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))]
internal static class HextechPerjuryEnergyCheckPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel card, ref UnplayableReason reason, ref bool __result)
    {
        if ((reason & UnplayableReason.EnergyCostTooHigh) == 0) return;

        var owner = card?.Owner;
        var relic = HextechPerjuryPayment.Find(owner);
        if (owner is null || relic is null) return;

        var energy = owner.PlayerCombatState.Energy;
        var deficit = card!.EnergyCost.GetAmountToSpend() - energy;
        if (!relic.CanCoverEnergyDeficit(deficit)) return;

        reason &= ~UnplayableReason.EnergyCostTooHigh;
        __result = reason == UnplayableReason.None;
    }
}

// 联动遗物 5（二）：真正扣费时用【伪证】顶掉能量的缺口。
[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
internal static class HextechPerjurySpendResourcesPatch
{
    [HarmonyPrefix]
    private static bool Prefix(CardModel __instance, ref Task<(int, int)> __result)
    {
        var owner = __instance.Owner;
        var relic = HextechPerjuryPayment.Find(owner);
        if (owner is null || relic is null) return true;

        var energy = owner.PlayerCombatState.Energy;
        var energyToSpend = __instance.EnergyCost.GetAmountToSpend();
        var deficit = energyToSpend - energy;
        if (deficit <= 0) return true;

        // 扣不动伪证（例如玩家压根没有【伪证】能力）就交回原逻辑，让引擎按能量不足处理。
        if (!relic.TryConsumePerjuryForDeficit(deficit)) return true;

        __result = SpendEnergyOnly(__instance, energy);
        return false;
    }

    private static async Task<(int, int)> SpendEnergyOnly(CardModel card, int energy)
    {
        var stars = Math.Max(0, card.GetStarCostWithModifiers());
        await card.SpendEnergy(energy);
        await card.SpendStars(stars);
        return (energy, stars);
    }
}

// 联动遗物 6：【正义】不再回血（保留原有的层数递减）。
[HarmonyPatch(typeof(JusticePower), nameof(JusticePower.AfterSideTurnEnd))]
internal static class HextechJusticeNoHealPatch
{
    [HarmonyPrefix]
    private static bool Prefix(JusticePower __instance, CombatSide side, ref Task __result)
    {
        if (!HextechJusticeRegen.IsActiveFor(__instance.Owner)) return true;

        __result = DecrementOnly(__instance, side);
        return false;
    }

    private static async Task DecrementOnly(JusticePower power, CombatSide side)
    {
        if (side != power.Owner.Side) return;
        if (power.Owner.IsDead) return;
        if (power.Amount <= 0) return;

        await PowerCmd.Decrement(power);
    }
}
