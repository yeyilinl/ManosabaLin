using ManosabaLin.Characters.Yalisalin.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     炎拳（3 费 能力・稀有，升级 2 费）：
///     获得 3 层【不死】（层数上限 3）。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class FlameFist()
    : ManosabaCardTemplate(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<UndeathPower>(UndeathPower.MaxStacks)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<UndeathPower>(); }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (Owner is not { } owner)
            return;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        var existing = owner.Creature.GetPower<UndeathPower>();
        if (existing is null)
        {
            await PowerCmd.Apply<UndeathPower>(
                choiceContext, owner.Creature, UndeathPower.MaxStacks, owner.Creature, this, false);
            return;
        }

        var missing = UndeathPower.MaxStacks - existing.Amount;
        if (missing <= 0)
            return;

        await PowerCmd.ModifyAmount(choiceContext, existing, missing, owner.Creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
