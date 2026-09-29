using System.Collections.Generic;
using System.Linq;
using Godot;
using ManosabaLin.Characters.Common.LinRelics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace ManosabaLin.Characters.Sherrylin.Orbs;

/// <summary>
///     持续型情绪球的「挂在血条下方」显示区（联动遗物 2）。
///     <para>
///         ⚠️ <b>这是纯显示层</b>：效果一个字都没搬过来 —— 真正的效果由
///         <see cref="HangingEmotionOrbs" /> 里那些**球对象自己**继续跑（引擎的
///         <c>SubscribeForCombatStateHooks</c> 让它们离开球位后照样收战斗钩子）。
///         所以这里既没有能力，也没有重写任何效果逻辑。
///     </para>
///     <para>
///         挂载方式与定位照抄同项目先例 <c>YalisalinFireColorCounter</c>（挂在 <c>NCreature</c> 上、
///         每帧跟锚点走）；卡面渲染照抄 <c>EmotionOrbVisualPatch</c>（球位里那张卡也是这么画的）。
///     </para>
/// </summary>
public partial class EmotionHangDisplay : Control
{
    private const float CardScale = 0.5f;
    private const float CardGap = 8f;
    private const float BarGap = 10f;
    private const float FallbackAnchorHeight = 24f;

    private static readonly Vector2 CardSize = NCard.defaultSize * CardScale;

    private readonly List<Control> _hoverTargets = [];

    private Player? _owner;
    private Creature? _creature;
    private Control? _anchor;
    private string _signature = string.Empty;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 20;
        Visible = false;

        HangingEmotionOrbs.Changed += OnChanged;
    }

    public override void _ExitTree()
    {
        HangingEmotionOrbs.Changed -= OnChanged;

        foreach (var target in _hoverTargets)
            NHoverTipSet.Remove(target);

        _hoverTargets.Clear();
    }

    public void SetContext(Player owner, Creature creature)
    {
        _owner = owner;
        _creature = creature;
        _anchor = null;
        Rebuild(force: true);
    }

    public override void _Process(double delta)
    {
        if (!CanShow())
        {
            Visible = false;
            return;
        }

        RefreshPosition();
        Rebuild();
    }

    private void OnChanged(Player player)
    {
        if (player != _owner) return;
        if (!CanShow()) return;      // 不在战斗画面/生物已不在场时别抢着建节点（下一帧 _Process 会统一收尾）
        Rebuild(force: true);
    }

    /// <summary>贴在血条（<c>NHealthBar.HpBarContainer</c>）正下方。</summary>
    private void RefreshPosition()
    {
        var creatureNode = _creature?.GetCreatureNode();
        if (creatureNode is null) return;

        if (_anchor is null || !IsInstanceValid(_anchor))
        {
            // NCreature._stateDisplay = GetNode<NCreatureStateDisplay>("%HealthBar")
            // NCreatureStateDisplay._healthBar = GetNode<NHealthBar>("%HealthBar")（各自场景内的唯一名）
            var stateDisplay = creatureNode.GetNodeOrNull<NCreatureStateDisplay>("%HealthBar");
            _anchor = stateDisplay?.GetNodeOrNull<NHealthBar>("%HealthBar")?.HpBarContainer;
            if (_anchor is null) return;
        }

        GlobalPosition = new Vector2(
            _anchor.GlobalPosition.X,
            _anchor.GlobalPosition.Y + Math.Max(_anchor.Size.Y, FallbackAnchorHeight) + BarGap);
    }

    private bool CanShow()
    {
        if (_owner is null || _creature is null || !_creature.IsAlive) return false;
        if (HangingEmotionOrbs.Count(_owner) == 0) return false;

        return IsCombatScreenActive()
               && _creature.GetCreatureNode() is { } creatureNode
               && creatureNode.IsVisibleInTree();
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

    private void Rebuild(bool force = false)
    {
        if (_owner is null)
        {
            Visible = false;
            return;
        }

        var orbs = HangingEmotionOrbs.For(_owner);
        var signature = string.Join(';', orbs.Select(orb => orb.Id.Entry));

        if (!force && signature == _signature) return;
        _signature = signature;

        foreach (var target in _hoverTargets)
            NHoverTipSet.Remove(target);
        _hoverTargets.Clear();

        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        if (orbs.Count == 0)
        {
            Visible = false;
            return;
        }

        for (var i = 0; i < orbs.Count; i++)
        {
            var entry = BuildEntry(orbs[i], i);
            if (entry is not null)
                AddChild(entry);
        }

        Size = new Vector2(
            orbs.Count * CardSize.X + Math.Max(0, orbs.Count - 1) * CardGap,
            CardSize.Y);

        Visible = true;
    }

    private Control? BuildEntry(OrbModel orb, int index)
    {
        if (orb is not IEmotionOrb emotionOrb) return null;

        var card = emotionOrb.GetEmotionCard();

        var container = new Control
        {
            Name = $"HangingEmotion{index}",
            Position = new Vector2(index * (CardSize.X + CardGap), 0f),
            Size = CardSize,
            MouseFilter = MouseFilterEnum.Ignore
        };

        var nCard = NCard.Create(card);
        if (nCard is not null)
        {
            nCard.Modulate = nCard.Modulate with { A = 0 };
            nCard.Scale = new Vector2(CardScale, CardScale);
            container.AddChild(nCard);
            nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

            // 小卡只留卡面；具体效果靠鼠标悬浮看（照抄 EmotionOrbVisualPatch 的做法）。
            nCard._descriptionLabel.QueueFree();
            nCard._ancientTextBg.QueueFree();
            nCard._typePlaque.QueueFree();
            nCard._typeLabel.QueueFree();
            nCard._titleLabel.QueueFree();
            nCard._ancientBanner.QueueFree();
            nCard._energyIcon.QueueFree();
            nCard._energyLabel.QueueFree();

            var tween = CreateTween();
            tween.TweenProperty(nCard, "modulate:a", 1.0f, 0.4f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
        }

        // 透明的悬浮靶子：只负责收鼠标事件 ⇒ 显示这张卡的效果。
        var hover = new ColorRect
        {
            Name = $"HangingEmotionHover{index}",
            Color = new Color(0f, 0f, 0f, 0f),
            Position = Vector2.Zero,
            Size = CardSize,
            MouseFilter = MouseFilterEnum.Stop
        };
        hover.Connect(SignalName.MouseEntered, Callable.From(() => ShowTip(hover, card)));
        hover.Connect(SignalName.MouseExited, Callable.From(() => NHoverTipSet.Remove(hover)));
        container.AddChild(hover);
        _hoverTargets.Add(hover);

        return container;
    }

    private static void ShowTip(Control target, CardModel card)
    {
        NHoverTipSet.Remove(target);
        NHoverTipSet.CreateAndShow(target, HoverTipFactory.FromCard(card), HoverTipAlignment.Right);
    }
}
