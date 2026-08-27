using Godot;
using HarmonyLib;
using ManosabaLin.Characters.Hiro.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Models;
using HiroCharacter = ManosabaLin.Characters.Hiro.Hiro;

namespace ManosabaLin.Patches;

[HarmonyPatch]
internal static class SamePlaceTruthFusionPreviewPatch
{
    private const string PreviewNodeName = "ManosabaLinSamePlacePendingTruthPreview";
    private const string FusionGlowNodeName = "ManosabaLinSamePlaceFusionGlowOverlay";
    private const float PreviewOffsetX = 258f;
    private const float CardGlowLeft = -162f;
    private const float CardGlowTop = -226f;
    private const float CardGlowRight = 162f;
    private const float CardGlowBottom = 222f;
    private const float FusionGlowLeft = CardGlowLeft;
    private const float FusionGlowTop = CardGlowTop;
    private const float FusionGlowRight = PreviewOffsetX + CardGlowRight;
    private const float FusionGlowBottom = CardGlowBottom;

    private static readonly Lazy<Shader> FusionGlowShader = new(CreateFusionGlowShader);

    private static readonly Lazy<ShaderMaterial> HiroFusionGlowMaterial = new(
        () => CreateFusionGlowMaterial(
            HiroCharacter.Color,
            new Color(1f, 0.42f, 0.36f, 1f)));

    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals), [typeof(PileType), typeof(CardPreviewMode)])]
    [HarmonyPostfix]
    private static void UpdateVisualsPostfix(NCard __instance)
    {
        Refresh(__instance);
    }

    [HarmonyPatch(typeof(NCardHolder), "OnFocus")]
    [HarmonyPostfix]
    private static void HolderFocusPostfix(NCardHolder __instance)
    {
        Refresh(__instance.CardNode);
    }

    [HarmonyPatch(typeof(NCardHolder), "OnUnfocus")]
    [HarmonyPostfix]
    private static void HolderUnfocusPostfix(NCardHolder __instance)
    {
        // 非战斗环境（图鉴、主菜单等）下 NRun.Instance 为 null，而
        // NTargetManager.Instance => NRun.Instance.GlobalUi.TargetManager，
        // getter 会直接抛 NullReferenceException，因此不能拿它判空。
        if (!IsTargetManagerAvailable())
        {
            return;
        }

        // 失焦只隐藏预览，绝不清除右键强化队列：
        // SamePlaceTruthFusionState 是进程内静态状态，失焦事件只会在主机端触发，
        // 若在主机端 Reset 而客户端队列保留，打出时 Consume 结果不一致，
        // 会直接导致多人状态分歧（desync）。
        // 队列只在正常打出（Consume）或打出被取消（NCardPlay.Cleanup(false)）时清除。
        RemovePreview(__instance.CardNode);
    }

    [HarmonyPatch(typeof(NCardPlay), "Cleanup", [typeof(bool)])]
    [HarmonyPostfix]
    private static void CardPlayCleanupPostfix(NCardPlay __instance, bool isFinished)
    {
        if (GetCurrentCard(__instance) is not SamePlaceTruth truth)
        {
            return;
        }

        if (!isFinished)
        {
            SamePlaceTruthFusionState.Reset(truth);
        }

        // 一个右键强化队列周期结束，允许下一次右键重新触发提示面板重建
        HolderTipsRefreshed.Remove(truth);

        RemovePreview(GetCurrentCardNode(__instance) ?? NCard.FindOnTable(truth));
    }

    internal static void Refresh(NCard? cardNode)
    {
        if (!IsNodeValid(cardNode))
        {
            return;
        }

        // 不可变模板（图鉴等）不处理
        if (cardNode!.Model == null)
        {
            RemovePreview(cardNode);
            return;
        }

// 战斗外的卡片（图鉴、选牌界面）不处理
        try
        {
            if (cardNode.Model.CombatState == null)
            {
                RemovePreview(cardNode);
                return;
            }
        }
        catch
        {
            RemovePreview(cardNode);
            return;
        }

        // 只有【旧识疑影】且右键已触发拼接（queue）时才显示霜覆初心预览；
        // 其他任何卡都绝不能让预览节点出现，否则所有手牌都会被覆盖一层霜覆初心。
        if (cardNode.Model is SamePlaceTruth truth && SamePlaceTruthFusionState.IsQueued(truth))
        {
            EnsurePreview(cardNode);
            // 同时把【霜覆初心】的效果提示并入本卡正在显示的悬浮提示面板
            RefreshHolderHoverTipsOnce(truth);
        }
        else
        {
            RemovePreview(cardNode);
        }
    }

    /// <summary>
    /// 每个右键强化队列周期，只把持有者提示面板重建一次（Remove + CreateHoverTips）：
    /// 面板内容在创建时从 CardModel.HoverTips 取值，而右键后 HoverTips 已包含
    /// 【霜覆初心】的效果提示（见 SamePlaceTruth.AdditionalHoverTips），
    /// 重建后面板会立即显示霜覆初心的效果本地化文本。
    /// UpdateVisuals 会高频触发，必须用标记集防抖。
    /// </summary>
    private static readonly HashSet<CardModel> HolderTipsRefreshed = new(ReferenceEqualityComparer.Instance);

    private static void RefreshHolderHoverTipsOnce(SamePlaceTruth truth)
    {
        if (!HolderTipsRefreshed.Add(truth))
        {
            return;
        }

        var holder = NPlayerHand.Instance?.GetCardHolder(truth);
        var active = holder != null && HasActiveHoverTipSet(holder);
        if (holder == null || !active)
        {
            return;
        }

        // 面板当前正在显示：按新状态重建一次；未显示时无需处理（下次悬停自然带上新提示）
        NHoverTipSet.Remove(holder);
        AccessTools.Method(typeof(NCardHolder), "CreateHoverTips")?.Invoke(holder, null);
    }

    private static bool HasActiveHoverTipSet(Control owner)
    {
        try
        {
            var active = AccessTools.Field(typeof(NHoverTipSet), "_activeHoverTips")?.GetValue(null);
            return active is System.Collections.IDictionary dict && dict.Contains(owner);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsTargetManagerAvailable()
    {
        try
        {
            return NTargetManager.Instance != null;
        }
        catch
        {
            return false;
        }
    }

    private static CardModel? GetCurrentCard(NCardPlay cardPlay)
    {
        return AccessTools.Property(typeof(NCardPlay), "Card")?.GetValue(cardPlay) as CardModel;
    }

    private static NCard? GetCurrentCardNode(NCardPlay cardPlay)
    {
        return AccessTools.Property(typeof(NCardPlay), "CardNode")?.GetValue(cardPlay) as NCard;
    }

    private static void EnsurePreview(NCard source)
    {
        var preview = source.GetNodeOrNull<NCard>(PreviewNodeName);
        if (IsNodeValid(preview))
        {
            ConfigurePreview(preview!);
            preview!.Visible = true;
            EnsureFusionGlow(source);
            preview.MoveToFront();
            return;
        }

        preview = NCard.Create(ModelDb.Card<SamePlacePendingTruth>());
        if (!IsNodeValid(preview))
        {
            return;
        }

        preview!.Name = PreviewNodeName;
        source.AddChild(preview);
        preview.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
        ConfigurePreview(preview);
        EnsureFusionGlow(source);
        preview.MoveToFront();
    }

    private static void ConfigurePreview(NCard preview)
    {
        preview.Position = new Vector2(PreviewOffsetX, 0f);
        preview.Scale = Vector2.One;
        preview.Modulate = new Color(1f, 1f, 1f, 0.96f);
        preview.MouseFilter = Control.MouseFilterEnum.Ignore;
        preview.ZIndex = 20;
        SetMouseFilterRecursive(preview);
    }

    private static void EnsureFusionGlow(NCard source)
    {
        if (GetFusionGlowParent(source) is not { } parent)
        {
            return;
        }

        var overlay = parent.GetNodeOrNull<ColorRect>(FusionGlowNodeName);
        if (!IsNodeValid(overlay))
        {
            overlay = CreateFusionGlowOverlay(parent);
        }

        ConfigureFusionGlowOverlay(overlay!);
        overlay!.Material = HiroFusionGlowMaterial.Value;
        overlay.Visible = true;
    }

    private static Control? GetFusionGlowParent(NCard source)
    {
        return source.CardVfxContainer as Control ?? source as Control;
    }

    private static ColorRect CreateFusionGlowOverlay(Control parent)
    {
        var overlay = new ColorRect
        {
            Name = FusionGlowNodeName,
            Color = Colors.White,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = HiroFusionGlowMaterial.Value,
            ZIndex = -10
        };

        parent.AddChild(overlay);
        return overlay;
    }

    private static void ConfigureFusionGlowOverlay(Control overlay)
    {
        overlay.AnchorLeft = 0.5f;
        overlay.AnchorRight = 0.5f;
        overlay.AnchorTop = 0.5f;
        overlay.AnchorBottom = 0.5f;
        overlay.OffsetLeft = FusionGlowLeft;
        overlay.OffsetTop = FusionGlowTop;
        overlay.OffsetRight = FusionGlowRight;
        overlay.OffsetBottom = FusionGlowBottom;
        overlay.CustomMinimumSize = new Vector2(FusionGlowRight - FusionGlowLeft, FusionGlowBottom - FusionGlowTop);
        overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        overlay.ZIndex = -10;
    }

    private static void RemovePreview(NCard? source)
    {
        if (!IsNodeValid(source))
        {
            return;
        }

        RemoveFusionGlow(source!);

        var preview = source!.GetNodeOrNull<NCard>(PreviewNodeName);
        if (IsNodeValid(preview))
        {
            preview!.QueueFree();
        }
    }

    private static void RemoveFusionGlow(NCard source)
    {
        var parent = GetFusionGlowParent(source);
        var overlay = parent?.GetNodeOrNull<ColorRect>(FusionGlowNodeName);
        if (IsNodeValid(overlay))
        {
            overlay!.QueueFree();
        }
    }

    private static void SetMouseFilterRecursive(Node node)
    {
        if (node is Control control)
        {
            control.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        foreach (var child in node.GetChildren())
        {
            if (child is Node childNode)
            {
                SetMouseFilterRecursive(childNode);
            }
        }
    }

    private static bool IsNodeValid(Node? node)
    {
        try
        {
            return node != null &&
                   GodotObject.IsInstanceValid(node) &&
                   !node.IsQueuedForDeletion();
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    private static ShaderMaterial CreateFusionGlowMaterial(Color primaryColor, Color secondaryColor)
    {
        var material = new ShaderMaterial
        {
            Shader = FusionGlowShader.Value
        };
        material.SetShaderParameter("primary_color", primaryColor);
        material.SetShaderParameter("secondary_color", secondaryColor);
        material.SetShaderParameter("card_spacing", PreviewOffsetX);
        material.SetShaderParameter("left_bound", FusionGlowLeft);
        material.SetShaderParameter("top_bound", FusionGlowTop);
        material.SetShaderParameter("right_bound", FusionGlowRight);
        material.SetShaderParameter("bottom_bound", FusionGlowBottom);
        return material;
    }

    private static Shader CreateFusionGlowShader()
    {
        return new Shader
        {
            Code = """
shader_type canvas_item;
render_mode blend_add;

uniform vec4 primary_color : source_color = vec4(0.72, 0.13, 0.13, 1.0);
uniform vec4 secondary_color : source_color = vec4(1.0, 0.42, 0.36, 1.0);
uniform float card_spacing = 258.0;
uniform float left_bound = -162.0;
uniform float top_bound = -226.0;
uniform float right_bound = 420.0;
uniform float bottom_bound = 222.0;
uniform float pulse_speed = 1.25;

float sd_rounded_box(vec2 p, vec2 b, float r) {
    vec2 q = abs(p) - b + vec2(r);
    return length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - r;
}

void fragment() {
    vec4 inherited_color = COLOR;
    vec2 local = vec2(
        mix(left_bound, right_bound, UV.x),
        mix(top_bound, bottom_bound, UV.y));

    float left_card = sd_rounded_box(local, vec2(162.0, 224.0), 28.0);
    float right_card = sd_rounded_box(local - vec2(card_spacing, 0.0), vec2(162.0, 224.0), 28.0);
    float d = min(left_card, right_card);

    float sharp_edge = 1.0 - smoothstep(0.0, 8.0, abs(d));
    float soft_glow = (1.0 - smoothstep(8.0, 54.0, abs(d))) * 0.48;
    float outer_haze = (1.0 - smoothstep(54.0, 90.0, abs(d))) * 0.16;
    float alpha = sharp_edge * 0.46 + soft_glow + outer_haze;

    float seam_center = card_spacing * 0.5;
    float seam_mask = 1.0 - smoothstep(28.0, 46.0, abs(local.x - seam_center));
    float vertical_middle = 1.0 - smoothstep(150.0, 216.0, abs(local.y));
    alpha *= 1.0 - seam_mask * vertical_middle * 0.96;

    float pulse = 0.78 + 0.22 * sin(TIME * pulse_speed);
    float flow = fract(UV.x * 0.55 + UV.y * 0.24 + TIME * 0.24);
    vec3 color = mix(primary_color.rgb, secondary_color.rgb, smoothstep(0.12, 0.92, flow));
    COLOR = vec4(color * inherited_color.rgb, clamp(alpha * pulse, 0.0, 0.82) * inherited_color.a);
}
"""
        };
    }
}
