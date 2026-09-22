using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MinionLib.RightClick;
using MinionLib.RightClick.Easy;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 封存火焰能力（基类）：每种火色一个独立能力。
/// 该能力在对应颜色封存火色有库存时出现于能力栏，显示该色封存数量；
/// 右键此能力：选择目标敌人的火色量表格子，把一个该色封存火色插回该位置。
/// 派生类固定一种 <see cref="YalisalinFireColor"/>，右键时颜色已定，无需再选色。
/// </summary>
public abstract class YalisalinSealedFirePower : ManosabaPowerTemplate, IEasyRightClickablePower
{
    /// <summary>此能力对应的封存火色。派生类固定实现。</summary>
    public abstract YalisalinFireColor SealedColor { get; }

    /// <summary>该色封存火色的数量。</summary>
    public int SealedCount => GetHairpin()?.GetSealedFireCount(SealedColor) ?? 0;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add(new IntVar("Amount", SealedCount));
            return description;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new IntVar("Amount", 0);
        }
    }

    /// <summary>
    /// 同步能力栏：遍历 4 种火色，凡该色封存火色有库存，就确保对应的颜色能力已存在。
    /// 调用时机：任意封存火色获得/变化之后。
    /// </summary>
    public static async Task Sync(PlayerChoiceContext choiceContext, Player owner, CardModel? source)
    {
        if (source == null)
            return;

        foreach (var color in Enum.GetValues<YalisalinFireColor>())
            await EnsurePowerForColor(choiceContext, owner, color, source);
    }

    private static async Task EnsurePowerForColor(
        PlayerChoiceContext choiceContext,
        Player owner,
        YalisalinFireColor color,
        CardModel? source)
    {
        if (!YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin)
            || hairpin.GetSealedFireCount(color) <= 0)
            return;

        switch (color)
        {
            case YalisalinFireColor.LightOrange:
                if (owner.Creature.GetPower<YalisalinSealedLightOrangeFirePower>() == null)
                    await PowerCmd.Apply<YalisalinSealedLightOrangeFirePower>(choiceContext, owner.Creature, 1, owner.Creature, source);
                break;
            case YalisalinFireColor.BrightYellow:
                if (owner.Creature.GetPower<YalisalinSealedBrightYellowFirePower>() == null)
                    await PowerCmd.Apply<YalisalinSealedBrightYellowFirePower>(choiceContext, owner.Creature, 1, owner.Creature, source);
                break;
            case YalisalinFireColor.Red:
                if (owner.Creature.GetPower<YalisalinSealedRedFirePower>() == null)
                    await PowerCmd.Apply<YalisalinSealedRedFirePower>(choiceContext, owner.Creature, 1, owner.Creature, source);
                break;
            case YalisalinFireColor.BlackRed:
                if (owner.Creature.GetPower<YalisalinSealedBlackRedFirePower>() == null)
                    await PowerCmd.Apply<YalisalinSealedBlackRedFirePower>(choiceContext, owner.Creature, 1, owner.Creature, source);
                break;
        }
    }

    private YalisalinsHairpin? GetHairpin()
    {
        if (Owner.Player is { } player
            && YalisalinFireColorSystem.TryGetHairpin(player, out var hairpin))
            return hairpin;

        return null;
    }

    #region IEasyRightClickablePower

    public bool CanHandleRightClickLocal(RightClickContext context)
    {
        if (context.Model != this || context.Player != Owner.Player)
            return false;

        if (Owner.CombatState is not { } combatState)
            return false;

        var hairpin = GetHairpin();
        return hairpin is not null
               && hairpin.HasSealedFireOf(SealedColor)
               && combatState.HittableEnemies.Any(hairpin.CanTrack);
    }

    public async Task OnRightClick(PlayerChoiceContext choiceContext, RightClickContext clickContext)
    {
        if (clickContext.Player != Owner.Player
            || Owner.CombatState is null)
            return;

        var hairpin = GetHairpin();
        if (hairpin is null || !hairpin.HasSealedFireOf(SealedColor))
            return;

        // 颜色已由本能力固定，无需再选色，直接选择目标敌人与插入位置。
        var prompt = new LocString("relics", $"{YalisalinsHairpin.LocalizationEntry}.sealedFire.slotStep");
        var tasks = new List<Task<(Creature Target, int SlotIndex)?>>();
        var counters = new List<YalisalinFireColorCounter>();
        foreach (var enemy in Owner.CombatState.HittableEnemies.Where(hairpin.CanTrack).ToArray())
        {
            // 满量表也可选格插入：会把该格及其后的火色下移一格，超出量表上限的火色直接消失。
            var counter = enemy.GetCreatureNode()?.GetNodeOrNull<YalisalinFireColorCounter>(
                $"YalisalinFireColorCounter_{Owner.Player!.NetId}");
            if (counter is null)
                continue;

            counters.Add(counter);
            tasks.Add(counter.PickInsertSlot(prompt));
        }

        if (tasks.Count == 0)
            return;

        var done = await Task.WhenAny(tasks);
        var result = await done;

        foreach (var counter in counters)
            counter.CancelPickInsertSlot();

        if (result is not { } picked)
            return;

        if (hairpin.TryUseSealedFire(picked.Target, SealedColor, picked.SlotIndex))
        {
            hairpin.Flash();

            // 本颜色封存耗尽 → 移除本颜色能力。
            if (!hairpin.HasSealedFireOf(SealedColor))
                await PowerCmd.Remove(this);
        }
    }

    public string RightClickPrompt =>
        LocString.GetIfExists("relics", $"{YalisalinsHairpin.LocalizationEntry}.rightClickPrompt")?.GetFormattedText()
        ?? "把1个封存火色插回敌人的火色量表";

    #endregion
}

/// <summary>封存火焰能力：暗橘（LightOrange）。</summary>
[RegisterPower]
public sealed class YalisalinSealedLightOrangeFirePower : YalisalinSealedFirePower
{
    public override YalisalinFireColor SealedColor => YalisalinFireColor.LightOrange;
}

/// <summary>封存火焰能力：暗红（BrightYellow）。</summary>
[RegisterPower]
public sealed class YalisalinSealedBrightYellowFirePower : YalisalinSealedFirePower
{
    public override YalisalinFireColor SealedColor => YalisalinFireColor.BrightYellow;
}

/// <summary>封存火焰能力：赤红（Red）。</summary>
[RegisterPower]
public sealed class YalisalinSealedRedFirePower : YalisalinSealedFirePower
{
    public override YalisalinFireColor SealedColor => YalisalinFireColor.Red;
}

/// <summary>封存火焰能力：黑红碳化（BlackRed）。</summary>
[RegisterPower]
public sealed class YalisalinSealedBlackRedFirePower : YalisalinSealedFirePower
{
    public override YalisalinFireColor SealedColor => YalisalinFireColor.BlackRed;
}