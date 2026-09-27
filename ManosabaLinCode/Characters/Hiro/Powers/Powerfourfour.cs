using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ManosabaLin.Characters.Hiro.Powers;

/// <summary>
/// 魔女监狱：<b>回合开始时</b>获得 40 层【魔女化】。
/// （卡面文案「回合开始时获得40层【魔女化】」是 specs，五个角色的「魔女监狱」共用本能力。）
/// </summary>
[RegisterPower]
public class Powerfourfour : ManosabaPowerTemplate
{
    private const decimal WitchificationAmount = 40m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner) return;
        if (Owner.IsDead) return;

        Flash();

        await PowerCmd.Apply<WithPower>(
            choiceContext, Owner, WitchificationAmount,
            Owner, null, false
        );
    }
}
