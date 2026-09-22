using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using TestTheSpire;

namespace ManosabaLin.Tests;

[ModInitializer(nameof(Init))]
public static class Entry
{
    /// <summary>
    /// 测试进程级自动卡牌选择器作用域。
    /// 持有不释放，使 LocalSelector 在整个测试会话中生效：
    /// headless 下所有 CardSelectCmd（FromHand/FromSimpleGrid 等）自动选卡，
    /// 不再创建 NSimpleCardSelectScreen，从而避免 headless 下 InitGrid NRE 崩溃。
    /// </summary>
    private static IDisposable? _selectorScope;

    public static void Init()
    {
        CombatTestBootstrap.Initialize(Assembly.GetExecutingAssembly(), new CombatTestOptions
        {
            LogPrefix = "ManosabaLin.Tests"
        });

        InstallAutoCardSelector();

        Log.Info("[ManosabaLin.Tests] Mod initialized");
    }

    private static void InstallAutoCardSelector()
    {
        try
        {
            _selectorScope = CardSelectCmd.UseSelector(new AutoFirstCardSelector(), localOnly: true);
            Log.Info("[ManosabaLin.Tests] Installed auto card selector (LocalSelector): headless card selections auto-pick.");
        }
        catch (Exception ex)
        {
            Log.Error($"[ManosabaLin.Tests] Failed to install auto card selector: {ex}");
        }
    }

    /// <summary>
    /// 自动选择器：从可选卡中选取前 max(minSelect,1) 张。
    /// 战斗内卡牌选择（FromHand/FromSimpleGrid）的确定性自动选卡实现。
    /// </summary>
    private sealed class AutoFirstCardSelector : ICardSelector
    {
        public Task<IEnumerable<CardModel>> GetSelectedCards(
            IEnumerable<CardModel> options,
            int minSelect,
            int maxSelect)
        {
            var list = options.Take(Math.Max(minSelect, 1)).ToList();
            return Task.FromResult<IEnumerable<CardModel>>(list);
        }

        public CardRewardSelection GetSelectedCardReward(
            IReadOnlyList<CardCreationResult> options,
            IReadOnlyList<CardRewardAlternative> alternatives)
        {
            return new CardRewardSelection
            {
                card = options.Count > 0 ? options[0].Card : null
            };
        }
    }
}
