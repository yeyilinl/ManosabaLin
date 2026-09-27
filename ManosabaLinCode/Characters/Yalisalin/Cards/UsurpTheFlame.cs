namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     夺火（1 费 攻击・先古・Lin 卡池）—— <b>占位实现</b>。
///     已定约束：本局游戏成长（每场战斗只能固定成长一次）、可重复打出（不消耗，打出后进弃牌堆）。
///     真正的效果待定，当前占位效果为「造成伤害」。
///     由「火之时代」获得；打出后另外两张分支牌被移除。
/// </summary>
[RegisterCard(typeof(LinCardPool))]
public sealed class UsurpTheFlame()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
{
    // TODO(占位)：效果待定 —— 本局游戏成长的「夺火」。
    private const int BaseDamage = 9;
    private const int UpgradedDamage = 12;

    public override CardAssetProfile AssetProfile => base.AssetProfile with
    {
        AncientTextBgPath = "ancient_empty_text_bg.png".CardsImagePath()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(BaseDamage, ValueProp.Move)];

    private const string EffectHoverLocEntry = "MANOSABA_LIN_CARD_USURP_THE_FLAME_EFFECT";

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
        if (Owner is not { } owner || cardPlay.Target is not { } target)
            return;

        await AgeOfFire.RemoveOtherBranches(choiceContext, owner, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(UpgradedDamage - BaseDamage);
    }
}
