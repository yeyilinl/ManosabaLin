using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 宽恕印记（2 费能力・罕见）：
/// 获得 [宽恕印记]：每次对原罪宽恕时 +1 层，下回合开始抽牌阶段后抽与层数等量的牌，然后清空。
/// 升级：清空改为最多保留 4 层。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class KuanShuYinJi() : ManosabaCardTemplate(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<KuanShuYinJiPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<KuanShuYinJiPower>(); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var power = await PowerCmd.Apply<KuanShuYinJiPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        if (power is not null)
            power.KeepFour = IsUpgraded;
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：清空改为最多保留 4 层（在 OnPlay 时按 IsUpgraded 设置 KeepFour）
    }
}