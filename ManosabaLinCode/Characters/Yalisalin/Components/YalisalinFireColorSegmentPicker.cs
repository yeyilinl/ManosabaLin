using Godot;
using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Components;

/// <summary>
/// 让玩家从目标的火色量表中"选择1格"进行封存的轻量选择器。
/// 显示目标当前的火色段为可点击色块，点击后返回所选段。
/// </summary>
public partial class YalisalinFireColorSegmentPicker : Control
{
    private const float SlotSize = 22f;
    private const float SlotGap = 4f;
    private const float LeftPadding = 10f;

    private readonly TaskCompletionSource<YalisalinFireColorSegment?> _completion = new();
    private Player? _viewer;
    private Creature? _target;

    public static async Task<YalisalinFireColorSegment?> Pick(
        Player viewer,
        Creature target,
        LocString prompt)
    {
        var node = target.GetCreatureNode();
        if (node is null)
            return null;

        var segments = YalisalinFireColorSystem.GetFireColorSegments(viewer, target);
        if (segments.Count == 0)
            return null;

        var picker = new YalisalinFireColorSegmentPicker
        {
            _viewer = viewer,
            _target = target
        };
        node.AddChild(picker);
        picker.Build(segments, prompt);
        await picker.ToSignal(picker.GetTree(), SceneTree.SignalName.ProcessFrame);
        picker.PositionToTarget();

        return await picker._completion.Task;
    }

    private void Build(
        System.Collections.Generic.IReadOnlyList<YalisalinFireColorSegment> segments,
        LocString prompt)
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 30;

        var container = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Stop
        };

        var label = new Label
        {
            Text = prompt.GetFormattedText(),
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate = new Color(1f, 1f, 1f)
        };
        container.AddChild(label);

        var row = new HBoxContainer
        {
            MouseFilter = MouseFilterEnum.Stop,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        container.AddChild(row);

        foreach (var segment in segments.OrderBy(segment => segment.Order))
        {
            var captured = segment;
            var button = new Button
            {
                Name = $"FireColorSegment_{segment.Order}",
                CustomMinimumSize = new Vector2(SlotSize, SlotSize),
                Modulate = segment.DisplayColor
            };
            button.Pressed += () => OnSegmentChosen(captured);
            row.AddChild(button);
        }

        AddChild(container);
    }

    private void PositionToTarget()
    {
        if (_target?.GetCreatureNode() is not { Hitbox: { } hitbox })
            return;

        GlobalPosition = new Vector2(
            hitbox.GlobalPosition.X - Size.X - LeftPadding,
            hitbox.GlobalPosition.Y + Mathf.Max(0f, (hitbox.Size.Y - Size.Y) * 0.5f));
    }

    private void OnSegmentChosen(YalisalinFireColorSegment segment)
    {
        _completion.TrySetResult(segment);
        QueueFree();
    }
}
