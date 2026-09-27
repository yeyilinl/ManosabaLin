using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Sherrylin.Powers;

/// <summary>
///     传递的情绪 - 施加在雪莉自己身上的「本回合」监听器。
///     <para>本回合内，被选中的队友每打出一张牌，雪莉就获得 1 层【情绪】。</para>
/// </summary>
[RegisterPower]
public sealed class EmotionTransferPower : ManosabaPowerTemplate
{
    private readonly List<Creature> _watched = [];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>登记本回合要监听的队友（重复登记会去重）。</summary>
    public void Watch(IEnumerable<Creature> allies)
    {
        foreach (var ally in allies)
            if (!_watched.Contains(ally))
                _watched.Add(ally);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_watched.Count == 0) return;

        var player = cardPlay.Card.Owner?.Creature;
        if (player is null || !_watched.Contains(player)) return;

        if (Owner.GetPower<EmotionPower>() is { } emotion)
            await emotion.GainEmotion(choiceContext);
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return Task.CompletedTask;

        RemoveInternal();
        return Task.CompletedTask;
    }
}
