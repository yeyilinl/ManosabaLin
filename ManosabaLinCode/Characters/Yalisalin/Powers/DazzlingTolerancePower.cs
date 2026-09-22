using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 你的包容很刺眼（独立能力）：余火烧掉牌时，随机造成 1 点伤害并获得 2 点格挡（每层一次）；
/// 若烧掉的是诅咒，则触发 3 次。
/// </summary>
[RegisterPower]
public sealed class DazzlingTolerancePower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 效果说明已作为 enhancement.dazzlingTolerance 追加到余火组件悬浮提示，不在状态栏显示。
    protected override bool IsVisibleInternal => false;

    /// <summary>
    /// 由遗物的余火「烧掉牌后」钩子转发调用：每层造成 {Amount} 点伤害并获 {Amount*2} 格挡。
    /// </summary>
    public Task ResolveBurned(
        PlayerChoiceContext choiceContext,
        CardModel burned,
        CardModel source,
        CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner is null || Amount <= 0)
            return Task.CompletedTask;

        var count = burned.Type == CardType.Curse || burned.Rarity == CardRarity.Curse ? 3 : 1;
        async Task Run()
        {
            for (var i = 0; i < count; i++)
            {
                if (owner.CombatState is { } combatState)
                {
                    await DamageCmd.Attack(Amount)
                        .FromCard(source, cardPlay)
                        .TargetingRandomOpponents(combatState)
                        .Execute(choiceContext);
                }

                await CreatureCmd.GainBlock(owner, Amount * 2, ValueProp.Move, cardPlay);
            }
        }

        return Run();
    }
}