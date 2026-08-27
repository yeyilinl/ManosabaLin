using Godot;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MinionLib.RightClick;
using MinionLib.RightClick.Easy;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 封存火焰能力：封存火色时出现在能力栏，显示当前封存的各色数量。
/// 右键此能力：选择一个敌人的火色量表格子，把1个封存火色插回该位置。
/// </summary>
[RegisterPower]
public sealed class YalisalinSealedFirePower : ManosabaPowerTemplate, IEasyRightClickablePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            var hairpin = GetHairpin();
            description.Add(new IntVar("LightOrange", hairpin?.SealedLightOrange ?? 0));
            description.Add(new IntVar("BrightYellow", hairpin?.SealedBrightYellow ?? 0));
            description.Add(new IntVar("Red", hairpin?.SealedRed ?? 0));
            description.Add(new IntVar("BlackRed", hairpin?.SealedBlackRed ?? 0));
            return description;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new IntVar("LightOrange", 0);
            yield return new IntVar("BrightYellow", 0);
            yield return new IntVar("Red", 0);
            yield return new IntVar("BlackRed", 0);
        }
    }

    /// <summary>
    /// 确保封存火焰能力出现在能力栏（已有则跳过，库存为0则不出现）。
    /// 在每次成功封存火色后调用。
    /// </summary>
    public static async Task Sync(PlayerChoiceContext choiceContext, Player owner, CardModel? source)
    {
        if (owner.Creature.GetPower<YalisalinSealedFirePower>() is not null)
            return;

        if (!YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin) || !hairpin.HasAnySealedFire())
            return;

        await PowerCmd.Apply<YalisalinSealedFirePower>(
            choiceContext, owner.Creature, 1, owner.Creature, source, false);
    }

    private YalisalinsHairpin? GetHairpin()
    {
        if (Owner.Player is { } player
            && YalisalinFireColorSystem.TryGetHairpin(player, out var hairpin))
            return hairpin;

        return null;
    }

    public bool CanHandleRightClickLocal(RightClickContext context)
    {
        if (context.Model != this || context.Player != Owner.Player)
            return false;

        if (Owner.CombatState is not { } combatState)
            return false;

        var hairpin = GetHairpin();
        return hairpin is not null
               && hairpin.HasAnySealedFire()
               && combatState.HittableEnemies.Any(hairpin.CanTrack);
    }

    public async Task OnRightClick(PlayerChoiceContext choiceContext, RightClickContext clickContext)
    {
        if (clickContext.Player != Owner.Player
            || Owner.CombatState is null)
            return;

        var hairpin = GetHairpin();
        if (hairpin is null || !hairpin.HasAnySealedFire())
            return;

        var colors = Enumerable.Range(0, 4)
            .Select(index => (YalisalinFireColor)index)
            .Where(hairpin.HasSealedFireOf)
            .ToArray();
        if (colors.Length == 0)
            return;

        var color = await YalisalinSealedColorPicker.Pick(
            Owner.Player!,
            colors,
            new LocString("relics", $"{YalisalinsHairpin.LocalizationEntry}.sealedFire.colorStep"));
        if (color is not { } pickedColor)
            return;

        // 直接激活所有可追踪敌人旁边的火色量表：点击任意格子的插入位即完成插入
        // 量表已满的敌人跳过（插回被拒绝，不显示插入位，避免"替换掉原本格子"）
        var prompt = new LocString("relics", $"{YalisalinsHairpin.LocalizationEntry}.sealedFire.slotStep");
        var tasks = new List<Task<(Creature Target, int SlotIndex)?>>();
        var counters = new List<YalisalinFireColorCounter>();
        foreach (var enemy in Owner.CombatState.HittableEnemies.Where(hairpin.CanTrack).ToArray())
        {
            if (hairpin.IsFireColorFull(enemy))
                continue;

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

        if (hairpin.TryUseSealedFire(picked.Target, pickedColor, picked.SlotIndex))
        {
            hairpin.Flash();

            if (!hairpin.HasAnySealedFire())
                await PowerCmd.Remove(this);
        }
    }

    public string RightClickPrompt =>
        LocString.GetIfExists("relics", $"{YalisalinsHairpin.LocalizationEntry}.rightClickPrompt")?.GetFormattedText()
        ?? "把1个封存火色插回敌人的火色量表";
}
