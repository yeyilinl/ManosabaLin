using Godot;
using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Components;

/// <summary>
/// 封印颜色选择：与火色计数器同款色块格子，横排显示有库存的封存火色，点击即选择。
/// </summary>
public partial class YalisalinSealedColorPicker : Control
{
    private const float SlotSize = 26f;
    private const float SlotGap = 8f;
    private const float LabelHeight = 20f;

    private readonly TaskCompletionSource<YalisalinFireColor?> _completion = new();
    private Player? _viewer;

    public static async Task<YalisalinFireColor?> Pick(
        Player viewer,
        IReadOnlyList<YalisalinFireColor> colors,
        LocString prompt)
    {
        if (colors.Count == 0)
            return null;

        if (colors.Count == 1)
            return colors[0];

        var picker = new YalisalinSealedColorPicker { _viewer = viewer };
        var tree = viewer.Creature.GetCreatureNode()?.GetTree();
        if (tree is null)
            return null;

        (tree.CurrentScene ?? tree.Root).AddChild(picker);
        picker.Build(colors, prompt);
        await picker.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        return await picker._completion.Task;
    }

    private void Build(IReadOnlyList<YalisalinFireColor> colors, LocString prompt)
    {
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 45;

        var container = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Stop,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        AddChild(container);

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

        foreach (var color in colors)
        {
            var captured = color;
            var count = GetSealedCount(color);
            var button = new Button
            {
                CustomMinimumSize = new Vector2(SlotSize + 24f, SlotSize + 24f),
                Text = $"×{count}",
                Modulate = color.DisplayColor(),
                TooltipText = CreateColorName(color, count)
            };
            button.Pressed += () => OnColorChosen(captured);
            row.AddChild(button);
        }

        // 居中定位
        SetAnchorsPreset(LayoutPreset.Center);
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Both;
    }

    private int GetSealedCount(YalisalinFireColor color)
    {
        if (_viewer != null && YalisalinFireColorSystem.TryGetHairpin(_viewer, out var hairpin))
            return hairpin.GetSealedFireCount(color);

        return 0;
    }

    private static string CreateColorName(YalisalinFireColor color, int count)
    {
        var suffix = color switch
        {
            YalisalinFireColor.LightOrange => "lightOrange",
            YalisalinFireColor.BrightYellow => "brightYellow",
            YalisalinFireColor.Red => "red",
            YalisalinFireColor.BlackRed => "blackRed",
            _ => "unknown"
        };

        var name = new LocString("relics", $"{YalisalinsHairpin.LocalizationEntry}.fireColor.{suffix}.title")
            .GetFormattedText();
        return $"{name} ×{count}";
    }

    private void OnColorChosen(YalisalinFireColor color)
    {
        _completion.TrySetResult(color);
        QueueFree();
    }
}

internal static class YalisalinFireColorDisplayExtensions
{
    public static Color DisplayColor(this YalisalinFireColor color)
    {
        return color switch
        {
            YalisalinFireColor.LightOrange => new Color("#c4631c"),
            YalisalinFireColor.BrightYellow => new Color("#7f1d1d"),
            YalisalinFireColor.Red => new Color("#dc143c"),
            YalisalinFireColor.BlackRed => new Color("#2a0707"),
            _ => Colors.White
        };
    }
}
