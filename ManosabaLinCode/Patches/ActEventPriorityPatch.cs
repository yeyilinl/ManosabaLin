using HarmonyLib;
using ManosabaLin.Settings;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace ManosabaLin.Patches;

/// <summary>
///     提升 ManosabaLin 联动事件的出现概率。
///     游戏在生成章节房间时会把事件列表洗牌后存入 <see cref="RoomSet.events" />，
///     而事件房间是按列表顺序消费事件的（<c>events[eventsVisited % events.Count]</c>），
///     因此排在列表末尾的事件基本不会在本次对局中出现。
///     本补丁在生成完成后，把设置中已启用的 ManosabaLin 联动事件移动到列表头部，
///     让它们稳定占据本次章节最早的事件房间。不消耗对局 RNG，避免影响种子/联机同步。
/// </summary>
[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
internal static class ActEventPriorityPatch
{
    private static readonly string[] BoostedEventIds =
    [
        "MANOSABA_LIN_EVENT_TEAM_CARD_EXCHANGE_EVENT",
        "MANOSABA_LIN_EVENT_MULTIPLAYER_COOPERATION_EVENT",
        "MANOSABA_LIN_EVENT_KINMANEYURAKUCHO_EVENT",
    ];

    private static readonly AccessTools.FieldRef<ActModel, RoomSet> RoomsRef =
        AccessTools.FieldRefAccess<ActModel, RoomSet>("_rooms");

    private static void Postfix(ActModel __instance)
    {
        try
        {
            var events = RoomsRef(__instance).events;
            if (events.Count <= 1)
                return;

            var boosted = events
                .Where(e => e.Id.Entry is var entry && BoostedEventIds.Contains(entry) && IsEventEnabled(entry))
                .ToList();
            if (boosted.Count == 0)
                return;

            foreach (var eventModel in boosted)
                events.Remove(eventModel);

            events.InsertRange(0, boosted);

            MainFile.Logger.Info(
                $"[ActEventPriority] Boosted {boosted.Count} event(s) to the front of {__instance.Id.Entry}: "
                + string.Join(", ", boosted.Select(e => e.Id.Entry)));
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[ActEventPriority] Failed to boost ManosabaLin events: {ex.Message}");
        }
    }

    private static bool IsEventEnabled(string entry)
    {
        return entry switch
        {
            "MANOSABA_LIN_EVENT_TEAM_CARD_EXCHANGE_EVENT" => EventSettingsService.IsTeamCardExchangeEventEnabled,
            "MANOSABA_LIN_EVENT_MULTIPLAYER_COOPERATION_EVENT" => EventSettingsService.IsMultiplayerCooperationEventEnabled,
            "MANOSABA_LIN_EVENT_KINMANEYURAKUCHO_EVENT" => EventSettingsService.IsKinmaneyurakuchoEventEnabled,
            _ => false,
        };
    }
}
