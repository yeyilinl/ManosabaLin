using System.Linq;
using System.Threading.Tasks;
using ManosabaLin.Characters.Hiro.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物 5：没有能量时，可以消耗「能量三倍的【伪证】」代替能量打出卡；
///     在此过程中，1 层【正义】会自动转为 4 层【伪证】。
///     <para>
///         支付必须**同步且确定性**（prefix 里直接扣），所以不能用异步的 <c>PowerCmd.Apply</c>。
///         全部走同步 API：已有的 Power 用 <c>SetAmount(silent: true)</c> / <c>RemoveInternal()</c>；
///         <b>玩家原本一层【伪证】都没有</b>时，用
///         <c>ModelDb.Power&lt;PerjuryPower&gt;().ToMutable()</c> + <c>ApplyInternal(...)</c> 现场建一个
///         （等价于 <c>PowerCmd.Apply</c> 的同步内核：<c>SetAmount</c> + <c>Owner.ApplyPowerInternal</c>）。
///         ⇒ 只要有【正义】就能生效，不再要求玩家预先持有【伪证】。
///     </para>
///     <para>
///         用 <c>SetAmount(silent: true)</c> / <c>ApplyInternal</c> 写入伪证，都会**绕过**
///         <c>AfterPowerAmountChanged</c> 与 <c>AfterApplied</c> ⇒ 不会触发 <c>PerjuryPower</c>
///         「满 5 层自动转正义」的结算。这正是我们要的：兑换后**多出来的层数应当原样留成【伪证】**，
///         而不是又被转回【正义】。（与项目内 <c>PerjuryPower.ConsumeForGuard</c> 同一套做法。）
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechPerjuryPayment : ManosabaRelicTemplate
{
    /// <summary>1 点能量的缺口 = 3 层【伪证】。</summary>
    public const int PerjuryPerEnergy = 3;

    /// <summary>1 层【正义】→ 4 层【伪证】（本遗物的兑换率；项目原版机制是 1:5）。</summary>
    public const int PerjuryPerJustice = 4;

    public override RelicRarity Rarity => RelicRarity.Starter;

    internal static HextechPerjuryPayment? Find(Player? player)
        => player?.Relics.OfType<HextechPerjuryPayment>().FirstOrDefault();

    /// <summary>
    ///     差额是否付得起（只读，供 <c>HasEnoughResourcesFor</c> 判定；不产生副作用）。
    ///     <para>玩家没有【伪证】能力时，只要有【正义】也判为付得起（消费时会现场建立【伪证】）。</para>
    /// </summary>
    internal bool CanCoverEnergyDeficit(int deficit)
    {
        if (deficit <= 0) return true;

        var perjury = Owner.Creature.GetPower<PerjuryPower>();
        var available = perjury is null ? 0 : (int)perjury.Amount;

        var justice = Owner.Creature.GetPower<JusticePower>();
        if (justice is not null && justice.Amount > 0)
            available += (int)justice.Amount * PerjuryPerJustice;

        return available >= deficit * PerjuryPerEnergy;
    }

    /// <summary>
    ///     同步扣掉差额对应的【伪证】；不足时按 1:4 把【正义】换成【伪证】。
    ///     兑换后多出来的层数**保留为【伪证】**（玩家原本没有【伪证】能力时会现场建一个）。
    ///     返回是否支付成功。
    /// </summary>
    internal bool TryConsumePerjuryForDeficit(int deficit)
    {
        if (deficit <= 0) return true;

        var perjury = Owner.Creature.GetPower<PerjuryPower>();
        var perjuryAmount = perjury is null ? 0 : (int)perjury.Amount;

        var justice = Owner.Creature.GetPower<JusticePower>();
        var justiceAmount = justice is null ? 0 : (int)justice.Amount;

        var need = deficit * PerjuryPerEnergy;

        var guard = 0;
        while (perjuryAmount < need && justiceAmount > 0 && guard++ < 256)
        {
            justiceAmount--;
            perjuryAmount += PerjuryPerJustice;
        }

        if (perjuryAmount < need) return false;

        Flash();

        // 扣掉用来兑换的【正义】层数。
        if (justice is not null && justiceAmount != (int)justice.Amount)
        {
            if (justiceAmount <= 0) justice.RemoveInternal();
            else justice.SetAmount(justiceAmount, true);
        }

        // 【伪证】：付掉缺口，剩下的（含兑换产生的多余）留成【伪证】层数。
        var remaining = perjuryAmount - need;
        if (perjury is not null)
        {
            if (remaining <= 0) perjury.RemoveInternal();
            else perjury.SetAmount(remaining, true);
        }
        else if (remaining > 0)
        {
            // 玩家原本没有【伪证】能力 ⇒ 同步建一个（PowerCmd.Apply 是异步的，在 prefix 里用不了）。
            var created = ModelDb.Power<PerjuryPower>().ToMutable();
            created.ApplyInternal(Owner.Creature, remaining, true);
        }

        return true;
    }
}
