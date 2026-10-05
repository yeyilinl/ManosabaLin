using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.Commands;

namespace ManosabaLin.Characters.Yalisalin.Cards;

[RegisterCard(typeof(YalisalinCardPool))]
public sealed class Unusedconclusion()
    : ManosabaCardTemplate(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    /// <summary>卡面首行「给予全体敌人 2 格火色」：每名敌人各给几格。</summary>
    private const int FireColorPerEnemy = 2;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        // 卡面首行：给予全体敌人 2 格火色
        if (Owner is { } owner && owner.Creature.CombatState is { } combatState)
        {
            foreach (var enemy in combatState.Enemies.Where(static e => e.IsAlive).ToList())
                await YalisalinFireColorSystem.GiveFireColor(choiceContext, owner, enemy, FireColorPerEnemy, this);
        }

        // 每回合第一次触发火色连续时获得能量并抽牌，逻辑在能力里
        await PowerCmd.Apply<MixedConclusionPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        AddKeyword(CardKeyword.Innate);
    }
}
