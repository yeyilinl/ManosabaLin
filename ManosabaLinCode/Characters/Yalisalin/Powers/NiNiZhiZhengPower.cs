using ManosabaLin.Characters.Common.Components;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 忤逆之证：每次对原罪「自惩」时 +1 层，每层获得 1 点力量。
/// </summary>
[RegisterPower]
public sealed class NiNiZhiZhengPower : ManosabaPowerTemplate
{
    private bool _hooked;

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
        Originalsin.PunishTriggered += OnPunishTriggered;
        _hooked = true;
    }

    private void Unhook()
    {
        if (!_hooked) return;
        Originalsin.PunishTriggered -= OnPunishTriggered;
        _hooked = false;
    }

    private async void OnPunishTriggered(PlayerChoiceContext choiceContext, CardModel punished)
    {
        try
        {
            if (Owner?.Player is not { } player) return;
            if (punished.Owner?.Creature != Owner) return;

            // 每次自惩：+1 层，并因层数获得 1 点力量
            await PowerCmd.ModifyAmount(choiceContext, this, 1m, Owner, punished);
            if (Owner is null) return;
            var strengthGain = Amount;
            if (strengthGain > 0m)
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, strengthGain, Owner, punished);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
    }
}