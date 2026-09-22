using ManosabaLin.Characters.Common.AncientCurses;
using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 赎罪回廊（1 费技能・罕见）：
/// 获得 5 点格挡。生成 1 张带原罪的原罪诅咒入手，并进行 1 次宽恕。
/// 升级：格挡 +3。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class RedemptionCorridor() : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block, cardPlay);

        var rng = owner.RunState.Rng.CombatCardGeneration;
        var sin = AncientSinCardCatalog.CreateRandom(CombatState, owner, rng);
        sin.TryAddComponent(new Originalsin());
        await CardPileCmd.AddGeneratedCardToCombat(sin, PileType.Hand, owner);

        if ((sin as IComponentsCardModel)?.GetComponent<Originalsin>() is { } generated)
            await generated.Forgive(choiceContext);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}