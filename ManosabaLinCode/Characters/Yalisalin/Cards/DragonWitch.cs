using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     龙之魔女（1 费 攻击・稀有）：
///     本回合每当你造成伤害时获得 1 层【嫌疑】，且你免疫【嫌疑】造成的减力量；
///     回合结束时把你的全部【嫌疑】改造成「等量 ×5」的【魔女化】，
///     并对所有敌人造成等同于被改造层数 ×2（升级 ×3）的伤害。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class DragonWitch()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<SuspectPower>();
            yield return HoverTipFactory.FromPower<WithPower>();
            yield return HoverTipFactory.FromPower<DragonWitchPower>();
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        var power = await PowerCmd.Apply<DragonWitchPower>(
            choiceContext, owner.Creature, 1m, owner.Creature, this, false);

        if (power is not null)
            power.StrongConversion = IsUpgraded;
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        // 升级：每层改造的伤害 2 → 3（在 OnPlay 时按 IsUpgraded 写入能力）。
    }
}
