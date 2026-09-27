using ManosabaLin.Characters.Hiro.Powers;

namespace ManosabaLin.Characters.Common.AncientCurses.Powers;

/// <summary>
///     【魔女化】原罪「宽恕」的代价：<b>下个玩家回合开始时</b>失去 <c>{Amount}</c> 层【魔女化】。
///     <para>
///         卡面文案写的是「下回合开始时失去10魔女化」⇒ 「宽恕」当场<b>不扣魔女化</b>，
///         只负责登记这个延迟效果（多次宽恕会叠加层数），真正的扣减发生在这里。
///         写法参照同目录的 <see cref="OriginalsinMiliaReturnPower" />（也是「下回合...」专用隐藏能力）。
///     </para>
/// </summary>
[RegisterPower]
public sealed class OriginalsinWitchificationLossPower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // clamp 到当前层数，不扣成负数（记忆点：掉到 0 层的能力由 ManosabaPowerTemplate 自行移除）
        var current = Owner.GetPower<WithPower>()?.Amount ?? 0m;
        if (current > 0m)
        {
            await PowerCmd.Apply<WithPower>(
                choiceContext, Owner, -Math.Min(Amount, current), Owner, null, false);
        }

        await PowerCmd.Remove(this);
    }
}
