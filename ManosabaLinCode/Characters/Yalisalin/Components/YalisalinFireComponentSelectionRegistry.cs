using HarmonyLib;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace ManosabaLin.Characters.Yalisalin.Components;

internal static class YalisalinFireComponentSelectionRegistry
{
    private static readonly Stack<YalisalinFireComponentContext> Contexts = [];

    public static YalisalinFireComponentContext? Current => Contexts.Count == 0 ? null : Contexts.Peek();

    public static IDisposable Begin(YalisalinFireComponentContext context)
    {
        Contexts.Push(context);
        return new Scope(context);
    }

    internal static bool TryHandleRightClick(Control screen, CardModel card)
    {
        var context = Current;
        if (context == null)
            return false;

        if (!context.ChoiceOptions.Contains(card))
            return false;

        // 余火选择界面：右键一律接管为「添火强化」，不再打开卡牌详情预览。
        // 有可用强化则应用并刷新提示；无可用强化（次数已用完）则仅刷新提示，告知玩家。
        if (context.PendingRightClicks.Count > 0)
            context.TryApplyNextRightClick(card);
        else
            Flash(screen);

        UpdatePrompt(screen, context, card);

        if (screen is NChooseACardSelectionScreen chooseScreen)
            UpdateChooseScreen(chooseScreen);

        return true;
    }

    private static void Flash(Control screen)
    {
        var label = (Control?)screen.GetNodeOrNull<MegaRichTextLabel>("%BottomLabel");
        if (label == null)
            label = screen.GetNodeOrNull<NCommonBanner>("Banner")?.label;

        if (label == null)
            return;

        label.Modulate = new Color(1f, 0.55f, 0.5f);
        label.CreateTween()?
            .TweenProperty(label, "modulate", new Color(1f, 1f, 1f), 0.35);
    }

    internal static void TryUpdatePromptForHover(NCardHolder holder, bool isHovered)
    {
        var context = Current;
        var card = holder.CardModel;
        if (context == null || card == null || !context.ChoiceOptions.Contains(card))
            return;

        var screen = FindSelectionScreen(holder);
        if (screen == null)
            return;

        UpdatePrompt(screen, context, isHovered ? card : null);
    }

    internal static void UpdatePrompt(
        Control screen,
        YalisalinFireComponentContext context,
        CardModel? hoveredCard = null)
    {
        var label = screen.GetNodeOrNull<MegaRichTextLabel>("%BottomLabel");
        if (label != null)
            label.Text = context.SelectionPromptTextFor(hoveredCard);
        else if (screen is NChooseACardSelectionScreen chooseScreen)
        {
            var banner = screen.GetNodeOrNull<NCommonBanner>("Banner");
            if (banner?.label != null)
                banner.label.SetTextAutoSize(context.SelectionPromptTextFor(hoveredCard));

            // 保证右下角「跳过本次添火」按钮在首次出现余火上下文时就位，
            // 不依赖 _Ready Postfix 的调用时机（悬停/右键都能激活）。
            EnsureSkipButton(chooseScreen, context);
        }
    }

    /// <summary>
    /// 初始化/刷新新选择器：写入横幅动态提示，并在右下角管理「跳过本次添火」按钮。
    /// </summary>
    internal static void UpdateChooseScreen(NChooseACardSelectionScreen screen)
    {
        GD.Print($"[余火][诊断] UpdateChooseScreen 触发, Current={(Current != null)}, queue={(Current?.PendingRightClicks.Count ?? -1)}");
        var context = Current;
        if (context != null)
            UpdatePrompt(screen, context, null);
        else
            EnsureSkipButton(screen, null);
    }

    /// <summary>
    /// 确保右下角「跳过本次添火」按钮的显隐/创建。context 为 null 或无可跳过强化时隐藏。
    /// </summary>
    private static void EnsureSkipButton(NChooseACardSelectionScreen screen, YalisalinFireComponentContext? context)
    {
        var skip = screen.GetNodeOrNull<Godot.Button>(SkipButtonName);
        var skipEnabled = context != null && context.PendingRightClicks.Count > 0;

        if (!skipEnabled)
        {
            if (skip != null)
                skip.Visible = false;
            return;
        }

        if (skip == null)
        {
            skip = CreateSkipButton(screen);
            screen.AddChild(skip);
            PositionSkipButtonBottomRight(screen, skip);
            GD.Print($"[余火][诊断] 已创建跳过按钮 size={screen.Size}");
        }

        skip.Visible = true;
        skip.Disabled = false;
    }

    private static void PositionSkipButtonBottomRight(Control screen, Godot.Button skip)
    {
        // 先固定为全屏左下锚，再按父节点实际尺寸从右下角往内偏移，避免受父锚点影响而跑到屏幕外。
        skip.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
        var parentSize = screen.Size;
        const float width = 210f, height = 44f, margin = 24f;
        skip.Size = new Vector2(width, height);
        if (parentSize.X > 0 && parentSize.Y > 0)
            skip.Position = new Vector2(parentSize.X - width - margin, parentSize.Y - height - margin);
        skip.ZIndex = 100;
    }

    /// <summary>
    /// 点击「跳过本次添火」：弹出当前队首强化（不应用），切换到下一个允许的添火效果。
    /// </summary>
    private static void OnSkipStrengthenPressed(NChooseACardSelectionScreen screen)
    {
        var context = Current;
        if (context == null)
            return;

        if (!context.SkipNextRightClick())
            return;

        var skip = screen.GetNodeOrNull<Godot.Button>(SkipButtonName);
        if (skip != null)
            skip.Visible = context.PendingRightClicks.Count > 0;

        UpdatePrompt(screen, context, null);
    }

    private const string SkipButtonName = "__YalisaliSkipStrengthenButton";

    private static Godot.Button CreateSkipButton(Control screen)
    {
        var button = new Godot.Button
        {
            Name = SkipButtonName,
            Text = YalisalinFireComponentContext.Text("rightClick.skipButton"),
            MouseFilter = Godot.Control.MouseFilterEnum.Stop,
        };
        button.Pressed += () => OnSkipStrengthenPressed((NChooseACardSelectionScreen)screen);
        return button;
    }

    private static Control? FindSelectionScreen(Node node)
    {
        for (var current = node; current != null; current = current.GetParent())
        {
            if (current is NCardGridSelectionScreen or NChooseACardSelectionScreen)
                return (Control)current;
        }

        return null;
    }

    private sealed class Scope(YalisalinFireComponentContext context) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            if (Contexts.Count > 0 && ReferenceEquals(Contexts.Peek(), context))
            {
                Contexts.Pop();
                return;
            }

            var remaining = Contexts.Where(item => !ReferenceEquals(item, context)).Reverse().ToArray();
            Contexts.Clear();
            foreach (var item in remaining)
                Contexts.Push(item);
        }
    }
}

[HarmonyPatch(typeof(NCardGridSelectionScreen), "ShowCardDetail")]
internal static class YalisalinFireComponentSelectionRightClickPatch
{
    private static bool Prefix(NCardGridSelectionScreen __instance, CardModel card)
    {
        return !YalisalinFireComponentSelectionRegistry.TryHandleRightClick(__instance, card);
    }
}

/// <summary>
/// 新选择器（NChooseACardSelectionScreen）右键 = 打开预览；余火选择期间改走「添火」增强。
/// </summary>
[HarmonyPatch(typeof(NChooseACardSelectionScreen), "OpenPreviewScreen")]
internal static class YalisalinFireComponentSelectionChooseRightClickPatch
{
    private static bool Prefix(NChooseACardSelectionScreen __instance, NCardHolder cardHolder)
    {
        return !YalisalinFireComponentSelectionRegistry.TryHandleRightClick(__instance, cardHolder.CardModel);
    }
}

/// <summary>
/// 新选择器无底部提示栏，把余火的动态提示（含右键添火说明）写到顶部横幅，
/// 并在右下角放置「跳过本次添火」确认√按钮（点击跳过当前强化，切到下一个）。
/// </summary>
[HarmonyPatch(typeof(NChooseACardSelectionScreen), "_Ready")]
internal static class YalisalinFireComponentSelectionChooseReadyPatch
{
    private static void Postfix(NChooseACardSelectionScreen __instance)
    {
        YalisalinFireComponentSelectionRegistry.UpdateChooseScreen(__instance);
    }
}

[HarmonyPatch(typeof(NCardHolder), "OnFocus")]
internal static class YalisalinFireComponentSelectionHolderFocusPatch
{
    private static void Postfix(NCardHolder __instance)
    {
        YalisalinFireComponentSelectionRegistry.TryUpdatePromptForHover(__instance, isHovered: true);
    }
}

[HarmonyPatch(typeof(NCardHolder), "OnUnfocus")]
internal static class YalisalinFireComponentSelectionHolderUnfocusPatch
{
    private static void Postfix(NCardHolder __instance)
    {
        YalisalinFireComponentSelectionRegistry.TryUpdatePromptForHover(__instance, isHovered: false);
    }
}
