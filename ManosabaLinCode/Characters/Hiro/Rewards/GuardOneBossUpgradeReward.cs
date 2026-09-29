using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using STS2RitsuLib.Combat.Rewards;

namespace ManosabaLin.Characters.Hiro.Rewards;

/// <summary>
///     击败第一层残骸首领（<see cref="Monsters.GuardOneMonster" />）后的战胜奖励：
///     从自己的牌组中选择可升级的卡牌进行升级。
/// </summary>
/// <remarks>
///     <para>
///         ⚠️ <b>已停用（2026-09-27）</b>：残骸首领的升级奖励已改由
///         <see cref="GuardOneBossUpgradeHook" />（<c>BeforeCombatRewardOffered</c> 跑局单例）发放，
///         升级界面会在奖励屏出现<b>之前</b>自动弹出，不再是奖励列表里的一条自定义奖励。
///         <b>不要再调用 <c>CombatRoom.AddExtraReward</c> 挂这个奖励</b>，否则会与钩子重复发放。
///     </para>
///     <para>
///         之所以保留这个类与 <see cref="GuardOneRewardRegistrar" /> 的注册（而不是直接删掉）：
///         自定义奖励 id 与 <c>RewardType</c> 一旦从注册表里消失，
///         旧存档在读档重建奖励时（<c>Reward.FromSerializable</c> → 按前缀查 <c>RewardType</c>）会解析失败。
///         保留注册的成本只是一条表项，所以选择保留。
///     </para>
///     <para>
///         同步说明：奖励集合里「选了哪一条」由原版 <c>RewardsSetSynchronizer</c> 同步；
///         本奖励自身的副作用（选卡 + 升级）走 <see cref="CardSelectCmd.FromDeckForUpgrade" />，
///         它内部用 <c>PlayerChoiceSynchronizer</c> 同步玩家选择，因此在联机下是确定性的。
///     </para>
/// </remarks>
public sealed class GuardOneBossUpgradeReward(Player player) : ModCustomReward(player)
{
    /// <summary>
    ///     奖励注册用的本地化 stem。注册 id =
    ///     <c>{MOD}_{REWARD}_{STEM}</c> = <c>MANOSABA_LIN_REWARD_GUARD_ONE_UPGRADE</c>。
    /// </summary>
    public const string RewardStem = "guard_one_upgrade";

    /// <summary>
    ///     奖励描述 key。本地化表沿用原版 <c>gameplay_ui</c>（与其它战斗奖励一致）。
    ///     必须与 <see cref="RewardStem" /> 推导出的注册 id 相同。
    /// </summary>
    public const string DescriptionKey = "MANOSABA_LIN_REWARD_GUARD_ONE_UPGRADE";

    /// <summary>牌组可升级卡牌足够时，一次奖励允许升级的张数。</summary>
    public const int MaxUpgradeCount = 2;

    /// <inheritdoc />
    public override RewardType ModRewardType => GuardOneRewardRegistrar.RewardType;

    /// <summary>
    ///     排在金币(1)/药水(2)/卡牌(5)等常规战后奖励之前，让玩家优先处理升级。
    /// </summary>
    public override int RewardsSetIndex => 0;

    /// <summary>奖励描述 key（本地化表沿用原版 <c>gameplay_ui</c>）。</summary>
    protected override string DescriptionLocKey => DescriptionKey;

    /// <inheritdoc />
    protected override string? RewardIconPath =>
        ImageHelper.GetImagePath("ui/reward_screen/reward_icon_card.png");

    /// <inheritdoc />
    protected override async Task<bool> OnSelect()
    {
        var upgradableCount = PileType.Deck.GetPile(Player).Cards.Count(static c => c.IsUpgradable);

        // 牌组里没有可升级的牌：无事可做，直接算作已领取，避免奖励卡在奖励屏上。
        if (upgradableCount <= 0) return true;

        // 可升级张数不足 2 张时收窄上限，否则确认按钮永远无法满足 MaxSelect（会卡死选卡界面）。
        var selectCount = Math.Min(MaxUpgradeCount, upgradableCount);

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, selectCount)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };

        var selected = (await CardSelectCmd.FromDeckForUpgrade(Player, prefs)).ToList();
        if (selected.Count == 0) return false;

        CardCmd.Upgrade(selected, CardPreviewStyle.None);
        return true;
    }

    /// <inheritdoc />
    public override void MarkContentAsSeen()
    {
    }
}

/// <summary>
///     注册「残骸首领战胜奖励」的自定义奖励类型。
///     读档重建（<c>Reward.FromSerializable</c>）与联机同步都依赖该类型已注册，因此在模组初始化时调用。
/// </summary>
public static class GuardOneRewardRegistrar
{
    private static RewardType? _rewardType;

    private static bool _registered;

    /// <summary>本模组的残骸首领奖励的稳定动态 <see cref="RewardType" />。</summary>
    public static RewardType RewardType => _rewardType
        ?? throw new InvalidOperationException(
            $"[{nameof(GuardOneRewardRegistrar)}] Reward type has not been registered yet.");

    /// <summary>幂等注册；重复调用只注册一次。</summary>
    public static void Register()
    {
        if (_registered) return;
        _registered = true;

        var definition = ModRewardRegistry.For(MainFile.ModId)
            .RegisterOwned(GuardOneBossUpgradeReward.RewardStem,
                (_, player, _) => new GuardOneBossUpgradeReward(player));

        _rewardType = definition.RewardType;
        MainFile.Logger.Info(
            $"[GuardOneReward] Registered reward '{definition.Id}' (RewardType=0x{(int)definition.RewardType:X8})");
    }
}
