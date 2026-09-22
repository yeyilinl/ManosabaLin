using ManosabaLin.Characters.Common.Components;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 宽恕印记：每次对原罪「宽恕」时 +1 层；下回合抽牌阶段后抽与层数等量的牌，然后清空。
/// 升级：清空改为保留最多 4 层。
/// </summary>
[RegisterPower]
public sealed class KuanShuYinJiPower : ManosabaPowerTemplate
{
    private bool _hooked;

    /// <summary>升级版是否在抽取后保留最多 4 层。</summary>
    public bool KeepFour { get; set; }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        Hook();
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        Unhook();
        return Task.CompletedTask;
    }

    private void Hook()
    {
        if (_hooked) return;
        Originalsin.ForgiveTriggered += OnForgiveTriggered;
        _hooked = true;
    }

    private void Unhook()
    {
        if (!_hooked) return;
        Originalsin.ForgiveTriggered -= OnForgiveTriggered;
        _hooked = false;
    }

    private async void OnForgiveTriggered(PlayerChoiceContext choiceContext, CardModel forgiven)
    {
        try
        {
            if (Owner?.Player is not { } player) return;
            if (forgiven.Owner?.Creature != Owner) return;

            await PowerCmd.ModifyAmount(choiceContext, this, 1m, Owner, forgiven);
        }
        catch (Exception ex)
        {
            Log.Error($"KuanShuYinJiPower.OnForgiveTriggered failed: {ex}");
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner) return;

        var amount = Amount;
        if (amount <= 0m) return;

        await CardPileCmd.Draw(choiceContext, (int)amount, player);

        // 清空：普通版归零；升级版最多保留 4 层
        var keep = KeepFour ? Math.Min(4, (int)amount) : 0;
        await PowerCmd.ModifyAmount(choiceContext, this, keep - amount, Owner, null);
    }
}