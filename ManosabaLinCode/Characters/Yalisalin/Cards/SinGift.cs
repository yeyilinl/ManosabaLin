using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 未命名111（1 费攻击・普通）：
/// 造成 8 点伤害，随机获得一张带原罪的原罪诅咒。
/// 升级：伤害 +2。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class SinGift() : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var owner = Owner;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var rng = owner.RunState.Rng.CombatCardGeneration;
        var sin = AncientSinCardCatalog.CreateRandom(CombatState, owner, rng);
        sin.TryAddComponent(new Originalsin());
        await CardPileCmd.AddGeneratedCardToCombat(sin, PileType.Hand, owner);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}