namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
///     【不死】（由「炎拳」获得）：
///     每回合开始时，你失去等同于层数的生命（可被格挡，因此格挡会先被啃掉）；
///     你受到致命伤害时不会死亡 —— 改为以 1 点生命存活，并失去 1 层【不死】。
///     层数上限为 3（由卡牌侧夹住）。
/// </summary>
[RegisterPower]
public sealed class UndeathPower : ManosabaPowerTemplate
{
    public const int MaxStacks = 3;

    private bool _handlingPrevent;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner)
            return true;

        // 还有层数时阻止死亡；层数耗尽后正常死亡。
        return Amount <= 0;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner || _handlingPrevent)
            return;

        _handlingPrevent = true;
        try
        {
            Flash();

            if (Owner.CurrentHp < 1m)
                await CreatureCmd.SetCurrentHp(Owner, 1m);

            await PowerCmd.ModifyAmount(
                new ThrowingPlayerChoiceContext(), this, -1m, Owner, null, false);

            if (Amount <= 0)
                await PowerCmd.Remove(this);
        }
        finally
        {
            _handlingPrevent = false;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner)
            return;

        if (Owner is not { IsAlive: true } owner)
            return;

        if (Amount <= 0)
            return;

        // 「可被格挡」= 走伤害结算，不要用 Unblockable。
        await CreatureCmd.Damage(choiceContext, owner, Amount, ValueProp.Move, owner, null, null);
    }
}
