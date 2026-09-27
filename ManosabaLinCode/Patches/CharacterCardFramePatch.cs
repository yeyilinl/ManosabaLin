using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace ManosabaLin.Patches;

/// <summary>
///     角色卡框：给卡面叠一层装饰节点 <c>images/characters/&lt;角色&gt;/&lt;角色小写&gt;card.png</c>。
/// </summary>
/// <remarks>
///     <para>
///         做法对齐 Koishi 那套（<c>YogurtCardFramePatch</c>）：贴图按「原始尺寸 × <see cref="FrameScale" />」
///         居中盖在卡面上，节点插到 <c>TitleLabel</c> 之前。最终层级（底 → 顶）：
///     </para>
///     <code>
///     Shadow → %Highlight → %Portrait → %Lock → %Frame → %DescriptionLabel → %PortraitBorder
///     → %OverlayContainer → %TitleBanner → %AncientBanner → 【本节点】
///     → %TitleLabel → %TypePlaque → %EnergyIcon → %StarIcon → %Enchantment → CardSparkles
///     </code>
///     <para>
///         即：卡框盖住原版卡框、肖像外圈、标题横幅，但标题文字 / 类型牌 / 能量球 / 星星仍压在它上面。
///         <c>%CardContainer</c> 的 <c>clip_contents = false</c>，超出卡面的外延装饰能正常画出去。
///     </para>
///     <para>
///         ⚠️ 必须显式写 <c>ExpandMode.IgnoreSize</c>：<c>TextureRect</c> 默认 <c>KeepSize</c> 会把节点最小尺寸
///         顶到贴图原始尺寸（本 mod 卡框图 1105x1423），<see cref="FrameScale" /> 就压不下去了。
///     </para>
///     <para>
///         归属按「卡写在哪个角色目录」判定（命名空间），不按卡池：角色目录里挂在共享 <c>LinCardPool</c>
///         的卡与 <c>TokenCardPool</c> 衍生卡（后继机、火种）也算该角色的卡。没有对应 png 的角色不叠。
///     </para>
/// </remarks>
[HarmonyPatch(typeof(NCard), "Reload")]
[HarmonyPriority(Priority.Low)]
public static class CharacterCardFramePatch
{
    /// <summary>
    ///     缩放系数，调这里即可：卡框图原始像素 × 此系数 = 节点尺寸（卡面 = 300x422）。
    ///     <para>
    ///         0.33 = 「卡框开口对齐牌框」：贴图中间那块透明「内窗」× 0.33 后高度 ≈ 卡面高 422
    ///         （Ananlin 内窗 949x1259、Yalisalin 972x1304），即卡框的开口正好套住卡面。
    ///         两张内窗都比卡面「胖」（比例 0.754/0.745 vs 卡面 0.711），所以开口会略宽于卡面
    ///         ⇒ 左右两边各露约 7~10px 原版卡框（接受这个取舍，换来卡框整体更大、更贴合牌框）。
    ///     </para>
    ///     <para>
    ///         想更大就往上调（0.36 ⇒ 开口比卡面宽约 25px、外延装饰更张扬）；想严丝合缝不留缝就
    ///         调到 0.29 附近（左上那张对照图），代价是卡框明显变小、上下露一条原版卡框边。
    ///         标定脚本：<c>.local/tools/card_frame_scale_check.py --strip &lt;png&gt;...</c>。
    ///     </para>
    /// </summary>
    private const float FrameScale = 0.33f;

    /// <summary>叠加节点名。</summary>
    private const string NodeName = "ManosabaLinCardFrame";

    private const string ContainerPath = "CardContainer";

    private const string TitleLabelName = "TitleLabel";

    private static readonly string[] ArtFileNameFormats = ["{0}card.png", "{0}_card.png"];

    /// <summary>
    ///     美术资源命名后缀。部分角色的美术按「角色全名」命名，与代码里的命名空间段不一致，
    ///     例如卡在 <c>Characters.Ema.*</c> 但美术目录是 <c>Emalin</c> 且文件叫 <c>emalincard.png</c>；
    ///     希罗则是目录 <c>Hiro</c>、文件 <c>hirolincard.png</c>。目录名与文件名都要各试一遍。
    /// </summary>
    private const string LinSuffix = "lin";

    /// <summary>卡类 → 卡框图路径（null = 该卡不叠），避免每次 Reload 都打资源系统。</summary>
    private static readonly Dictionary<Type, string?> ArtPaths = [];

    private static void Postfix(NCard __instance)
    {
        var container = __instance.GetNodeOrNull<Control>(ContainerPath);
        if (container == null)
            return;

        // 无论如何先清掉旧节点：卡牌是对象池复用的，同一 NCard 会先后显示不同的卡。
        if (container.GetNodeOrNull(NodeName) is { } stale)
        {
            container.RemoveChild(stale);
            stale.QueueFree();
        }

        if (__instance.Model is not { } model)
            return;

        if (ResolveArtPath(model.GetType()) is not { } path)
            return;

        var frame = new TextureRect
        {
            Name = NodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            // 贴图整幅拉伸铺满节点（配合下面的尺寸=贴图×系数），不做二次比例适配。
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            Texture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse),
        };

        // 与 %Frame 中心对齐：Center 锚点 + 以中心为原点的 offset。
        frame.SetAnchorsPreset(Control.LayoutPreset.Center);
        var width = (frame.Texture?.GetWidth() ?? 300f) * FrameScale;
        var height = (frame.Texture?.GetHeight() ?? 422f) * FrameScale;
        frame.OffsetLeft = -width / 2f;
        frame.OffsetTop = -height / 2f;
        frame.OffsetRight = width / 2f;
        frame.OffsetBottom = height / 2f;

        container.AddChild(frame);

        // 动态找 TitleLabel（别写死索引）插到它之前：盖住原版卡框/肖像外圈/标题横幅，
        // 但标题文字与能量球等仍在它之上。找不到就留在末尾。
        var titleIndex = FindChildIndex(container, TitleLabelName);
        if (titleIndex >= 0 && titleIndex < container.GetChildCount())
            container.MoveChild(frame, titleIndex);
    }

    /// <summary>
    ///     卡 → 角色卡框图。角色段取命名空间 <c>Characters</c> 之后那一段
    ///     （<c>ManosabaLin.Characters.Ema.Cards</c> ⇒ <c>Ema</c>），
    ///     再按 <see cref="ArtPathCandidates" /> 的候选逐个试，命中即止；都取不到就不叠。
    /// </summary>
    private static string? ResolveArtPath(Type cardType)
    {
        if (ArtPaths.TryGetValue(cardType, out var cached))
            return cached;

        string? path = null;
        foreach (var candidate in ArtPathCandidates(cardType))
        {
            if (!ResourceLoader.Exists(candidate))
                continue;

            path = candidate;
            break;
        }

        ArtPaths[cardType] = path;
        return path;
    }

    /// <summary>
    ///     候选路径：目录 <c>{角色}</c> / <c>{角色}lin</c> × 文件名 stem <c>{角色小写}</c> / <c>{角色小写}lin</c>
    ///     × <c>card.png</c> / <c>_card.png</c>。同一个角色目录里只放一张卡框图，命中即止。
    /// </summary>
    private static IEnumerable<string> ArtPathCandidates(Type cardType)
    {
        var segments = cardType.Namespace?.Split('.');
        var index = segments is null ? -1 : Array.IndexOf(segments, "Characters");
        if (index < 0 || index + 1 >= segments!.Length)
            yield break;

        var character = segments[index + 1];
        var lower = character.ToLowerInvariant();
        foreach (var directory in new[] { character, character + LinSuffix })
        {
            foreach (var stem in new[] { lower, lower + LinSuffix })
            {
                foreach (var format in ArtFileNameFormats)
                    yield return string.Format(format, stem).CharacterImgPath(directory);
            }
        }
    }

    private static int FindChildIndex(Node container, string name)
    {
        for (var i = 0; i < container.GetChildCount(); i++)
        {
            if (container.GetChild(i).Name == name)
                return i;
        }

        return -1;
    }
}
