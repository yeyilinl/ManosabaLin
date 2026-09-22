using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.Powers;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;

namespace ManosabaLin.Characters.Sherrylin.Cards;

/// <summary>
/// 没有脚印，犯人是…：
/// 清空你的减力量，选择一个目标获得等于你当前嫌疑一半的嫌疑，使其获得等量的力量，
/// 若获得3层则令目标和自己都获得一层汉娜的魔法。
/// </summary>
[RegisterCard(typeof(SherrylinCardPool))]
public sealed class NoFootprintsCulprit() : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Rare, TargetType.AnyPlayer)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<SuspectPower>();
            yield return HoverTipFactory.FromPower<HnmPower>();
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var source = this;

        await CreatureCmd.TriggerAnim(source.Owner.Creature, "Cast", source.Owner.Character.CastAnimDelay);

        // 清空减力量
        var tempStrDown = source.Owner.Creature.GetPower<TempStrengthDown>();
        if (tempStrDown != null && tempStrDown.Amount > 0)
        {
            await PowerCmd.ModifyAmount(choiceContext, tempStrDown, -tempStrDown.Amount,
                source.Owner.Creature, source, false);
        }

        // 获取当前嫌疑层数
        var suspectPower = source.Owner.Creature.GetPower<SuspectPower>();
        var suspectAmount = suspectPower?.Amount ?? 0;
        var halfSuspect = suspectAmount / 2;

        // 目标完全由 TargetType.AnyPlayer 的目标选择系统控制（手动打出时玩家可点击任一存活玩家/角色；无目标时退回自己）
        var target = cardPlay.Target ?? source.Owner.Creature;

        // 使目标获得嫌疑和力量
        if (halfSuspect > 0)
        {
            await PowerCmd.Apply<SuspectPower>(
                choiceContext, target, halfSuspect,
                source.Owner.Creature, source, false);

            await PowerCmd.Apply<TempStrength>(
                choiceContext, target, halfSuspect,
                source.Owner.Creature, source, false);

            // 若获得3层则令目标和自己都获得一层汉娜的魔法
            if (halfSuspect >= 3)
            {
                await PowerCmd.Apply<HnmPower>(
                    choiceContext, target, 1,
                    source.Owner.Creature, source, false);
                await PowerCmd.Apply<HnmPower>(
                    choiceContext, source.Owner.Creature, 1,
                    source.Owner.Creature, source, false);
            }
        }
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
    }
}