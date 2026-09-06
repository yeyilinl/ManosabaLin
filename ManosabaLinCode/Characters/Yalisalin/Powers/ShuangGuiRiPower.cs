using ManosabaLin.Characters.Common.Components;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 双轨日：任意原罪触发「自惩」后，可对另一张手牌原罪立刻宽恕。
/// </summary>
[RegisterPower]
public sealed class ShuangGuiRiPower : ManosabaPowerTemplate
{
    private bool _hooked;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

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

            var selected = (await CardSelectCmd.FromHand(
                choiceContext,
                player,
                new CardSelectorPrefs(new LocString("powers", $"{Id.Entry}.selectionScreenPrompt"), 0, 1),
                c => c.HasComponent<Originalsin>() && !ReferenceEquals(c, punished),
                punished)).FirstOrDefault();

            if ((selected as IComponentsCardModel)?.GetComponent<Originalsin>() is { } sin)
                await sin.Forgive(choiceContext);
        }
        catch (Exception ex)
        {
            Log.Error($"ShuangGuiRiPower.OnPunishTriggered failed: {ex}");
        }
    }
}