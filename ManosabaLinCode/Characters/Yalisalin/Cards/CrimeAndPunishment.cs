using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     罪与罚（2 费 攻击・稀有，升级 1 费）：
///     本场战斗中你每「自惩」过 1 次，获得 1 点力量；
///     你每「宽恕」过 1 次，回复 2 点生命；
///     每「自惩」1 次会让「宽恕」额外回复 1 点生命。
///     每回合本卡只能使用一次。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class CrimeAndPunishment()
    : ManosabaCardTemplate(3, CardType.Attack, CardRarity.Rare, TargetType.Self)
{
    private const int BaseHealPerForgive = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("HealPerForgive", BaseHealPerForgive)];

    /// <summary>每回合限一次：靠一个下回合开始时自动消失的隐藏标记来夹住。</summary>
    protected override bool IsPlayableC =>
        base.IsPlayableC
        && Owner is { } owner
        && owner.Creature.GetPower<CrimeAndPunishmentUsedPower>() is null;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        await PowerCmd.Apply<CrimeAndPunishmentUsedPower>(
            choiceContext, owner.Creature, 1m, owner.Creature, this, true);

        var punish = 0;
        var forgive = 0;
        if (YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin))
        {
            punish = hairpin.SinPunishThisCombat;
            forgive = hairpin.SinForgiveThisCombat;
        }

        if (punish > 0)
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, owner.Creature, punish, owner.Creature, this, false);

        // 每次宽恕回 2 点生命，每「自惩」1 次再额外多回 1 点。
        var heal = forgive * (BaseHealPerForgive + punish);
        if (heal > 0)
            await CreatureCmd.Heal(owner.Creature, heal);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}

