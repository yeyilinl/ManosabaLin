using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common.Components;
using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 嫉恨反噬（2 费攻击・罕见）：
/// 造成 13 点伤害，获得 1 张带原罪的原罪诅咒；
/// 本回合至少自惩过 1 张原罪 → 伤害翻倍；上回合宽恕过 → 再获得等量格挡（回合计数由初始遗物发夹提供）。
/// 升级：伤害 13 → 15。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class JiHenFanShi() : ManosabaCardTemplate(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var me = owner.Creature;

        var punishThisTurn = 0;
        var forgiveLastTurn = 0;
        if (YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin))
        {
            punishThisTurn = hairpin.SinPunishThisTurn;
            forgiveLastTurn = hairpin.SinForgiveLastTurn;
        }

        var damage = DynamicVars.Damage.BaseValue;
        if (punishThisTurn > 0)
            damage *= 2m;

        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 上回合宽恕过 → 再获得等量格挡
        if (forgiveLastTurn > 0)
            await CreatureCmd.GainBlock(me, damage, ValueProp.Move, cardPlay);

        // 获得 1 张带原罪的原罪诅咒
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