using System.Collections.Generic;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common.AncientCurses.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses;

[RegisterCard(typeof(LinCardPool))]
public sealed class RaiyaMadness : LinAncientCurseCard
{
    public RaiyaMadness() : base(3) { }

    private bool _extraing;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WhiffChance", 10m),
        new DynamicVar("ExtraChance", 0m),
    ];

    protected override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player,
        ComponentContext componentContext)
    {
        if (player != Owner) return;
        var extra = Owner.Creature.GetPower<OriginalsinRaiyaExtraPower>();
        if (extra is null)
            extra = await PowerCmd.Apply<OriginalsinRaiyaExtraPower>(
                choiceContext, Owner.Creature, DynamicVars["ExtraChance"].BaseValue, Owner.Creature, this);
        extra?.SyncFromCard(this);
    }

    protected override decimal ModifyDamageMultiplicativeC(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner?.Creature) return 1m;
        if (Pile is { Type: PileType.Hand }) return 1m;
        return ShouldWhiff() ? 0m : 1m;
    }

    protected override decimal ModifyBlockMultiplicativeC(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner?.Creature) return 1m;
        if (Pile is { Type: PileType.Hand }) return 1m;
        return ShouldWhiff() ? 0m : 1m;
    }

    protected override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource,
        ComponentContext componentContext)
    {
        if (dealer != Owner?.Creature) return;
        if (Pile is { Type: PileType.Hand }) return;
        if (result.TotalDamage <= 0) return;
        if (_extraing) return;
        if (!ShouldExtraTrigger()) return;

        _extraing = true;
        try
        {
            await CreatureCmd.Damage(
                choiceContext,
                target,
                result.TotalDamage,
                props | ValueProp.Unpowered,
                cardSource,
                null);
        }
        finally
        {
            _extraing = false;
        }
    }

    protected override async Task AfterBlockGained(
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource,
        ComponentContext componentContext)
    {
        if (creature != Owner?.Creature) return;
        if (Pile is { Type: PileType.Hand }) return;
        if (amount <= 0m) return;
        if (_extraing) return;
        if (!ShouldExtraTrigger()) return;

        _extraing = true;
        try
        {
            await CreatureCmd.GainBlock(creature, amount, props, null);
        }
        finally
        {
            _extraing = false;
        }
    }

    private bool ShouldWhiff()
    {
        if (Owner?.RunState.Rng.CombatCardGeneration is not { } rng) return false;
        var chance = (float)(DynamicVars["WhiffChance"].BaseValue / 100m);
        return rng.NextFloat() < chance;
    }

    private bool ShouldExtraTrigger()
    {
        var extra = Owner?.Creature.GetPower<OriginalsinRaiyaExtraPower>();
        if (extra != null && extra.ConsumeForcedExtra())
            return true;

        if (Owner?.RunState.Rng.CombatCardGeneration is not { } rng) return false;
        var chance = (float)(DynamicVars["ExtraChance"].BaseValue / 100m);
        return chance > 0f && rng.NextFloat() < chance;
    }
}
