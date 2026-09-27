using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     被缚的普罗米修斯（2 费 技能・稀有）：
///     本回合和下回合，每当你消耗敌人身上的火色时，其中 1 格在正常进入消耗结算的同时，
///     额外获得为你自己的封存火色；
///     下回合结束时，失去等同于你当前所有封存火色总数量的格挡（升级只失去一半）。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class BoundPrometheus()
    : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    private const int DurationTurns = 2;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<BoundPrometheusPower>(); }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<BoundPrometheusPower>(
            choiceContext, owner.Creature, DurationTurns, owner.Creature, this, false);

        if (power is not null)
            power.HalfBlockLoss = IsUpgraded;
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：结算时只失去一半格挡（在 OnPlay 时按 IsUpgraded 写入能力）。
    }
}
