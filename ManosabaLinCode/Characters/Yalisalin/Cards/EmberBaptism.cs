using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     烬火洗礼（2 费 攻击・稀有）：
///     消耗目标身上全部火色，然后对全体敌人造成「被消耗层数 × 3」点伤害。
///     升级后每 1 层改为 ×4。
///     选择目标用于决定「消耗谁的火色」，伤害本身打全体。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class EmberBaptism()
    : ManosabaCardTemplate(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("DamagePerFire", 3)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner || cardPlay.Target is not { } target)
            return;

        var segments = YalisalinFireColorSystem.GetFireColorSegments(owner, target);
        if (segments.Count == 0)
            return;

        // 消耗「全部火色」：按现有格数请求，实际消耗层数以返回值结算（保留最高火色能力可能少吃几层）。
        var consumed = await YalisalinFireColorSystem.ConsumeFireColor(
            choiceContext, owner, target, segments.Count, this);

        if (consumed.Count == 0)
            return;

        var damage = consumed.Count * DynamicVars["DamagePerFire"].BaseValue;
        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars["DamagePerFire"].UpgradeValueBy(1);
    }
}
