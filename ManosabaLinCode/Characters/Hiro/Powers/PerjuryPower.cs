using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System;

namespace ManosabaLin.Characters.Hiro.Powers;

[RegisterPower]
public class PerjuryPower : ManosabaPowerTemplate
{
    // 防止转化过程中再次触发，造成无限递归
    private bool _isConverting;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, // ★ 新增
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // 只处理伪证，且是层数增加的情况
        // ModifyAmount 的 Postfix 是同步方法，用 TaskHelper.RunSafely 派发异步任务
        if (power.Amount >= 5) TaskHelper.RunSafely(CheckAndConvert());
        return Task.CompletedTask;
    }

    // 转化逻辑（在 Power 内部，可以调用 protected 的 Flash()）
    public async Task CheckAndConvert()
    {
        if (_isConverting) return;
        if (Owner == null || Amount < 5) return;

        _isConverting = true;
        try
        {
            var currentStacks = Amount;
            var justiceGain = currentStacks / 5;
            var remainder = currentStacks % 5;

            Flash(); // ✅ 在 Power 内部调用，不会报错

            // 把伪证层数改为余数（或移除）
            if (remainder > 0)
            {
                SetAmount(remainder, true);
            }
            else
            {
                SetAmount(0, true);
                RemoveInternal();
            }

            // 给予正义层数
            await PowerCmd.Apply<JusticePower>(
                new ThrowingPlayerChoiceContext(),
                Owner,
                justiceGain,
                Owner,
                null,
                false
            );
        }
        finally
        {
            _isConverting = false;
        }
    }

    // 路径A：Power 首次被添加时（PowerCmd.Apply 新建 Power 后调用）
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await CheckAndConvert();
    }

    /// <summary>
    ///     「我才是正确」用：按需要的层数消耗伪证；伪证不足时把 1 层【正义】兑换成 5 层伪证一起消耗，
    ///     兑换产生的多余层数保留为伪证。
    ///     <para>全程用 <c>SetAmount(silent: true)</c> 写入，避免触发本类「满 5 层自动转正义」的结算。</para>
    /// </summary>
    /// <returns>实际消耗掉的伪证层数。</returns>
    public async Task<int> ConsumeForGuard(int amount)
    {
        if (amount <= 0 || Owner is null) return 0;

        var consumed = 0;

        for (var guard = 0; consumed < amount && guard < 64; guard++)
        {
            if (Amount > 0)
            {
                var take = Math.Min(amount - consumed, Amount);
                SetAmount(Amount - take, true);
                consumed += take;
                continue;
            }

            var justice = Owner.GetPower<JusticePower>();
            if (justice is null || justice.Amount <= 0) break;

            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), justice, -1, Owner, null);
            SetAmount(Amount + 5, true);
        }

        if (consumed > 0) Flash();
        if (Amount <= 0) RemoveInternal();

        return consumed;
    }
}