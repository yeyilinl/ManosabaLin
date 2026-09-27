using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Ema.Powers;

/// <summary>
///     花朵绽放 - 施加在艾玛自己身上的「本回合」监听器。
///     <para>本回合内，被选中的队友变形 / 生成卡牌时可以被拦截成艾玛的 1 费消耗【亲近】/【疏远】牌，</para>
///     <para>且其打出的【亲近】【疏远】牌必定触发额外效果。</para>
/// </summary>
[RegisterPower]
public sealed class FlowerBloomPower : ManosabaPowerTemplate
{
    private readonly List<Creature> _watched = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>登记本回合要监听的队友（重复登记会去重）。</summary>
    public void Watch(IEnumerable<Creature> teammates)
    {
        foreach (var teammate in teammates)
            if (!_watched.Contains(teammate))
                _watched.Add(teammate);
    }

    /// <summary>该生物是否被本次花朵绽放监听。</summary>
    internal bool IsWatching(Creature creature) => _watched.Contains(creature);

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return Task.CompletedTask;

        FlowerBloomTracker.Unregister(this);
        _watched.Clear();
        RemoveInternal();
        return Task.CompletedTask;
    }
}
