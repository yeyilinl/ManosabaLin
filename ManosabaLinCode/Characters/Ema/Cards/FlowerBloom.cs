using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Ema.Powers;
using ManosabaLin.Characters.Emalin;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MinionLib.Component.Core;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Ema.Cards;

/// <summary>
///     花朵绽放 - 2 费技能，罕见，多人专属。
///     <para>选择 1 名队友：本回合内，</para>
///     <para>· 其变形卡牌时可以选择改为变形为艾玛的 1 费消耗【疏远】牌；</para>
///     <para>· 其生成卡牌时可以选择改为生成为艾玛的 1 费消耗【亲近】牌；</para>
///     <para>· 其打出的【亲近】【疏远】牌必定触发额外效果。</para>
///     <para>升级后费用减 1。</para>
/// </summary>
[RegisterCard(typeof(EmalinCardPool))]
public sealed class FlowerBloom() : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
{
    /// <summary>本地化键前缀（同时用作拦截界面文案所在的 <c>cards</c> 表条目）。</summary>
    internal const string LocEntry = "MANOSABA_LIN_CARD_FLOWER_BLOOM";

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

   

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        if (cardPlay.Target?.Player is not { } teammate) return;
        if (teammate == Owner) return;
        if (!teammate.Creature.IsAlive) return;
        if (teammate.Creature.Side != Owner.Creature.Side) return;

        var power = await PowerCmd.Apply<FlowerBloomPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        if (power is null) return;

        FlowerBloomTracker.Register(power);
        power.Watch([teammate.Creature]);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}
