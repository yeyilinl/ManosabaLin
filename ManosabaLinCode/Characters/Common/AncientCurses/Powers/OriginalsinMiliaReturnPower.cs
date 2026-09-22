namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

[RegisterPower]
public sealed class OriginalsinMiliaReturnPower : ManosabaPowerTemplate
{
    private CardModel? _canonical;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    protected override bool IsVisibleInternal => false;

    public void Store(CardModel canonical) => _canonical = canonical.CanonicalInstance;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        if (_canonical is null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        if (Owner.CombatState is not { } combatState)
        {
            await PowerCmd.Remove(this);
            return;
        }

        var allies = combatState.Players.Where(p => p.Creature.IsAlive).ToList();
        if (allies.Count == 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        var rng = player.RunState.Rng.CombatCardGeneration;
        var ally = rng.NextItem(allies) ?? player;
        var returned = combatState.CreateCard(_canonical, ally);
        await CardPileCmd.AddGeneratedCardToCombat(returned, PileType.Draw, ally, CardPilePosition.Random);
        _canonical = null;
        await PowerCmd.Remove(this);
    }
}
