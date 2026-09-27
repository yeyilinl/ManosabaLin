using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace ManosabaLin.Patches;

/// <summary>
///     当前正在结算的卡牌所使用的玩家选择上下文。
///     <para>
///         引擎的 <c>CardCmd.Transform</c> / <c>CardPileCmd.AddGeneratedCardsToCombat</c> 都不接收
///         选择上下文，而「花朵绽放」需要在变形 / 生成的那一刻弹出玩家选择界面。
///         这里在 <see cref="CardModel.OnPlayWrapper" /> 前后登记上下文，
///         使补丁能在卡牌结算过程中取回正确的、可暂停动作队列的上下文。
///     </para>
/// </summary>
public static class CardPlayContext
{
    /// <summary>最近一次正在结算的卡牌选择上下文；不在卡牌结算过程中时为 null。</summary>
    public static PlayerChoiceContext? Current { get; private set; }

    internal static void Set(PlayerChoiceContext? context) => Current = context;
}

/// <summary>
///     在卡牌结算期间登记 / 还原 <see cref="CardPlayContext" />。
///     <para>
///         <see cref="CardModel.OnPlayWrapper" /> 是 async 方法，前缀在状态机启动前运行，
///         后缀紧接着拿到返回的 <see cref="Task" />；因此还原动作挂在返回 Task 的续体上，
///         而不是后缀本身（后缀运行时卡牌还没结算完）。
///         嵌套结算（自动打出）依靠保存 / 还原上一份值来正确回退。
///     </para>
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
internal static class CardPlayContextPatch
{
    [HarmonyPrefix]
    private static void Prefix(PlayerChoiceContext choiceContext, out PlayerChoiceContext? __state)
    {
        __state = CardPlayContext.Current;
        CardPlayContext.Set(choiceContext);
    }

    [HarmonyPostfix]
    private static void Postfix(ref Task __result, PlayerChoiceContext? __state)
    {
        if (__result is null) return;

        __result = RestoreAsync(__result, __state);
    }

    private static async Task RestoreAsync(Task task, PlayerChoiceContext? previous)
    {
        try
        {
            await task;
        }
        finally
        {
            CardPlayContext.Set(previous);
        }
    }
}
