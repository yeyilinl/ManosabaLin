namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     传火（1 费 技能・先古・Lin 卡池）—— <b>占位实现</b>。
///     已定约束：本场战斗成长、可重复打出（不消耗，打出后进弃牌堆，因此可以再抽回来）。
///     真正的效果待定，当前占位效果为「获得格挡」。
///     由「火之时代」获得；打出后另外两张分支牌被移除。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class LinkTheFire()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
{
    // TODO(占位)：效果待定 —— 本场战斗成长的「传火」。
    private const int BaseBlock = 8;
    private const int UpgradedBlock = 11;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => base.AssetProfile with
    {
        AncientTextBgPath = "ancient_empty_text_bg.png".CardsImagePath()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(BaseBlock, ValueProp.Move)];

    private const string EffectHoverLocEntry = "MANOSABA_LIN_CARD_LINK_THE_FIRE_EFFECT";

    /// <summary>卡面只留风味文本，效果改走悬浮提示（与「我不听，我需要你」同款）。</summary>
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

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(UpgradedBlock - BaseBlock);
    }
}
