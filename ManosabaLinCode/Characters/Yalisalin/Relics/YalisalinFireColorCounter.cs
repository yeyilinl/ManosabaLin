using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using System;
using System.Linq;

namespace ManosabaLin.Characters.Yalisalin.Relics;

public partial class YalisalinFireColorCounter : Control
{
    private const float SlotSize = 22f;
    private const float SlotGap = 4f;
    private const float LeftPadding = 10f;
    private const int Columns = 1;

    private static readonly Color EmptyColor = new("2b2021");

    private Player? _viewer;
    private Creature? _target;
    private ColorRect[] _slots = [];
    private YalisalinFireColor?[] _slotColors = [];
    private string _lastSignature = string.Empty;
    private int _hoveredSlotIndex = -1;

    /// <summary>当前槽位数（默认 6；被「高塔」改造后为 12，会动态重建）。</summary>
    private int _rows = YalisalinsHairpin.MaxSegments;

    private static Vector2 ComputeSize(int rows)
    {
        return new Vector2(
            SlotSize * Columns + SlotGap * (Columns - 1),
            SlotSize * rows + SlotGap * (rows - 1));
    }

    private int CurrentMaxSegments
    {
        get
        {
            if (_viewer != null && YalisalinFireColorSystem.TryGetHairpin(_viewer, out var hairpin))
                return Math.Max(1, hairpin.CurrentMaxSegments);

            return YalisalinsHairpin.MaxSegments;
        }
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Pass;
        ZIndex = 20;

        RebuildSlots(CurrentMaxSegments);
        Refresh(force: true);
    }

    public override void _ExitTree()
    {
        NHoverTipSet.Remove(this);

        foreach (var slot in _slots)
            NHoverTipSet.Remove(slot);
    }

    public void SetContext(Player viewer, Creature target)
    {
        _viewer = viewer;
        _target = target;
        Refresh(force: true);
    }

    public override void _Process(double delta)
    {
        if (!CanShowInCurrentContext())
        {
            HideCounter();
            return;
        }

        RefreshPosition();
        Refresh();
    }

    private void RebuildSlots(int rows)
    {
        foreach (var slot in _slots)
        {
            NHoverTipSet.Remove(slot);
            RemoveChild(slot);
            slot.QueueFree();
        }

        _rows = Math.Max(1, rows);

        var size = ComputeSize(_rows);
        Size = size;
        CustomMinimumSize = size;

        _slots = Enumerable.Range(0, _rows)
            .Select(CreateSlot)
            .ToArray();

        _slotColors = new YalisalinFireColor?[_rows];

        for (var i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            var index = i;
            slot.Connect(SignalName.MouseEntered, Callable.From(() => OnSlotHovered(index)));
            slot.Connect(SignalName.MouseExited, Callable.From(() => OnSlotUnhovered(index)));
            AddChild(slot);
        }

        _hoveredSlotIndex = -1;
        _lastSignature = string.Empty;
    }

    private static ColorRect CreateSlot(int index)
    {
        return new ColorRect
        {
            Name = $"FireColorSlot{index + 1}",
            Color = EmptyColor,
            Position = new Vector2(
                (index % Columns) * (SlotSize + SlotGap),
                (index / Columns) * (SlotSize + SlotGap)),
            Size = new Vector2(SlotSize, SlotSize),
            MouseFilter = MouseFilterEnum.Stop
        };
    }

    private void RefreshPosition()
    {
        if (_target?.GetCreatureNode() is not { Hitbox: { } hitbox })
            return;

        var size = ComputeSize(_rows);

        GlobalPosition = new Vector2(
            hitbox.GlobalPosition.X - size.X - LeftPadding,
            hitbox.GlobalPosition.Y + Math.Max(0f, (hitbox.Size.Y - size.Y) * 0.5f));
    }

    private bool CanShowInCurrentContext()
    {
        if (_viewer == null || _target == null || !_target.IsAlive)
            return false;

        return IsCombatScreenActive()
               && _target.GetCreatureNode() is { } creatureNode
               && creatureNode.IsVisibleInTree();
    }

    private void HideCounter()
    {
        Visible = false;
        _lastSignature = string.Empty;

        if (_hoveredSlotIndex >= 0 && _hoveredSlotIndex < _slots.Length)
            NHoverTipSet.Remove(_slots[_hoveredSlotIndex]);
    }

    private static bool IsCombatScreenActive()
    {
        try
        {
            return ActiveScreenContext.Instance.GetCurrentScreen() is NCombatRoom;
        }
        catch
        {
            return false;
        }
    }

    private void Refresh(bool force = false)
    {
        // 「高塔」改造火色格的瞬间，槽位数量会变；这里检测到变化就重建。
        var max = CurrentMaxSegments;
        if (max != _rows)
        {
            RebuildSlots(max);
            force = true;
        }

        var segments = GetVisibleSegments();
        var signature = string.Join(';', segments.Select(segment => $"{(int)segment.Color}:{segment.Order}"));

        if (!force && signature == _lastSignature)
            return;

        _lastSignature = signature;
        Visible = segments.Count > 0;

        for (var i = 0; i < _slots.Length; i++)
        {
            _slotColors[i] = i < segments.Count ? segments[i].Color : null;
            _slots[i].Color = i < segments.Count ? segments[i].DisplayColor : EmptyColor;
        }

        if (_hoveredSlotIndex >= 0)
            ShowSlotHoverTip(_hoveredSlotIndex);
    }

    private IReadOnlyList<YalisalinFireColorSegment> GetVisibleSegments()
    {
        if (_viewer == null || _target == null || !_target.IsAlive)
            return [];

        return YalisalinFireColorSystem
            .GetFireColorSegments(_viewer, _target)
            .OrderBy(segment => segment.Order)
            .Take(_rows)
            .ToArray();
    }

    private void OnSlotHovered(int index)
    {
        _hoveredSlotIndex = index;
        ShowSlotHoverTip(index);
    }

    private void OnSlotUnhovered(int index)
    {
        if (index < 0 || index >= _slots.Length)
            return;

        NHoverTipSet.Remove(_slots[index]);
        if (_hoveredSlotIndex == index)
            _hoveredSlotIndex = -1;
    }

    private void ShowSlotHoverTip(int index)
    {
        if (index < 0 || index >= _slots.Length)
            return;

        var slot = _slots[index];
        NHoverTipSet.Remove(slot);

        var color = _slotColors.ElementAtOrDefault(index);
        if (color == null)
            return;

        NHoverTipSet.CreateAndShow(
            slot,
            CreateColorHoverTip(color.Value),
            HoverTipAlignment.Right);
    }

    private HoverTip CreateColorHoverTip(YalisalinFireColor color)
    {
        var suffix = color switch
        {
            YalisalinFireColor.LightOrange => "lightOrange",
            YalisalinFireColor.BrightYellow => "brightYellow",
            YalisalinFireColor.Red => "red",
            _ => "unknown"
        };

        var description = new LocString("relics", $"{YalisalinsHairpin.LocalizationEntry}.fireColor.{suffix}.description");
        description.Add("Damage", GetCurrentRedConsumeDamage());

        return new HoverTip(
            new LocString("relics", $"{YalisalinsHairpin.LocalizationEntry}.fireColor.{suffix}.title"),
            description);
    }

    private int GetCurrentRedConsumeDamage()
    {
        if (_viewer != null && YalisalinFireColorSystem.TryGetHairpin(_viewer, out var hairpin))
            return hairpin.RedConsumeDamage;

        return 3;
    }
}
