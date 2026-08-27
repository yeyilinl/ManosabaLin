using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using System;

namespace ManosabaLin.Patches;

/// <summary>
/// 宝藏房"选择用手臂"指尖校准补丁（按 Hiro pivot 相对位置映射）。
///
/// 游戏本体 NHandImage 把角色 ArmPointingTexture 贴到固定 TextureRect
/// （383x1072, KeepAspectCentered）上，pointing 模式 PivotOffset = (163,10)，
/// 猜拳模式 PivotOffset = (197,600)。这两个 pivot 是作者按 Hiro 图手动调校的
/// 绝对像素值；其他角色手图尺寸/构图不同，直接复用会让旋转轴心（=鼠标判定点）
/// 偏离素材指尖/拳头。
///
/// 本补丁把 Hiro 的 pivot 换算成相对 Hiro 手图的归一化位置，再按每个角色
/// 自己手图的实际尺寸映射回 TextureRect 显示坐标系，作为该角色的 PivotOffset：
///   pointing : (163,10)  / Hiro pointing (749x2100) -> (0.4255, 0.0093)
///   fighting : (197,600) / Hiro rock    (1210x3461) -> (0.5147, 0.5597)
/// 映射公式（KeepAspectCentered）：
///   k = min(rectW/w, rectH/h); offset = (rectSize - 图尺寸*k)/2;
///   pivot = offset + 归一化 * 图尺寸 * k
/// </summary>
[HarmonyPatch(typeof(NHandImage))]
public static class NHandImageArmAlignPatch
{
    /// <summary>pointing（指尖）pivot 相对 Hiro 手图的归一化位置。</summary>
    private static readonly Vector2 PointingPivotNorm = new(0.4255f, 0.0093f);

    /// <summary>fighting（拳头）pivot 相对 Hiro 手图的归一化位置。</summary>
    private static readonly Vector2 FightingPivotNorm = new(0.5147f, 0.5597f);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NHandImage._Ready))]
    public static void OnReadyPostfix(NHandImage __instance)
    {
        // 初始即 pointing 模式
        ApplyMappedPivot(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NHandImage.SetIsInFight))]
    public static void OnSetIsInFightPostfix(NHandImage __instance)
    {
        // 进入/退出猜拳都重算（当前贴图决定用 pointing 还是 fighting 基准）
        ApplyMappedPivot(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NHandImage.SetTextureToFightMove))]
    public static void OnSetTextureToFightMovePostfix(NHandImage __instance)
    {
        // 换猜拳手势贴图（rock/paper/scissors）后重算拳头 pivot
        ApplyMappedPivot(__instance);
    }

    /// <summary>
    /// 按当前手图路径选择 Hiro 基准归一化，映射到该图在 TextureRect
    /// 显示坐标系中的位置，设为 PivotOffset。
    /// </summary>
    private static void ApplyMappedPivot(NHandImage handImage)
    {
        try
        {
            var textureRect = handImage._textureRect;
            if (textureRect?.Texture == null)
                return;

            var texture = textureRect.Texture;
            var path = texture.ResourcePath;
            if (string.IsNullOrEmpty(path))
                return;

            // 按文件名后缀选基准归一化；只处理 ManosabaLin 手图
            Vector2 norm;
            if (path.EndsWith("_pointing.png", StringComparison.Ordinal))
            {
                norm = PointingPivotNorm;
            }
            else if (path.EndsWith("_rock.png", StringComparison.Ordinal) ||
                     path.EndsWith("_paper.png", StringComparison.Ordinal) ||
                     path.EndsWith("_scissors.png", StringComparison.Ordinal))
            {
                norm = FightingPivotNorm;
            }
            else
            {
                return;
            }

            var rectSize = textureRect.Size;
            var textureSize = new Vector2(texture.GetWidth(), texture.GetHeight());
            if (textureSize.X <= 0f || textureSize.Y <= 0f)
                return;

            // KeepAspectCentered：等比缩放 + 居中
            var scale = Mathf.Min(rectSize.X / textureSize.X, rectSize.Y / textureSize.Y);
            var drawnSize = textureSize * scale;
            var offset = (rectSize - drawnSize) * 0.5f;

            var pivot = offset + norm * drawnSize;
            textureRect.PivotOffset = pivot;

            MainFile.Logger.Info(
                $"[NHandImageArmAlign] {path} pivot -> ({pivot.X:F1}, {pivot.Y:F1})");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[NHandImageArmAlign] failed: {ex.Message}");
        }
    }
}
