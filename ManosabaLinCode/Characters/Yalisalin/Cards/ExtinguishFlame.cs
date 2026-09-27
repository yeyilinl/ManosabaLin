using ManosabaLin.Characters.Common.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     灭火（2 费 能力・先古・Lin 卡池）：
///     世界之火熄灭 —— 你与所有敌人造成的伤害减半；
///     每回合开始时你获得 12 点格挡（升级 18），且你的格挡不再于回合开始时消失。
///     由「火之时代」获得；打出后另外两张分支牌被移除。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class ExtinguishFlame()
    : ManosabaCardTemplate(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    private const int BaseBlockPerTurn = 12;
    private const int UpgradedBlockPerTurn = 18;

    public override CardAssetProfile AssetProfile => base.AssetProfile with
    {
        AncientTextBgPath = "ancient_empty_text_bg.png".CardsImagePath()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(BaseBlockPerTurn, ValueProp.Move)];

    private const string EffectHoverLocEntry = "MANOSABA_LIN_CARD_EXTINGUISH_FLAME_EFFECT";

    /// <summary>
    ///     卡面只留风味文本，效果改走悬浮提示（与「我不听，我需要你」同款）。
    ///     提示文案与【灭火】能力的说明逐字相同，故不再额外挂能力提示，避免重复两条一样的框。
    /// </summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return CardEffectHoverTipFactory.FromCard(this, EffectHoverLocEntry); }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        await AgeOfFire.RemoveOtherBranches(choiceContext, owner, this);

        var perTurn = IsUpgraded ? UpgradedBlockPerTurn : BaseBlockPerTurn;
        await PowerCmd.Apply<ExtinguishFlamePower>(
            choiceContext, owner.Creature, perTurn, owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(UpgradedBlockPerTurn - BaseBlockPerTurn);
    }
}
