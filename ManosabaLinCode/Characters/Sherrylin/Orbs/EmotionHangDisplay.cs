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
///     持续型情绪球的「挂在角色头顶上方」显示区（联动遗物 2）。
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
    private const float CardScale = 0.125f;
    private const float CardGap = 4f;
    private const float HeadGap = 6f;

    private static readonly Vector2 CardSize = NCard.defaultSize * CardScale;

    private readonly List<Control> _hoverTargets = [];

    private Player? _owner;
    private Creature? _creature;
    private string _signature = string.Empty;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 0;
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
        // Rebuild 在「签名没变」时会提前 return、不会把 Visible 翻回 true —— 这里兜底恢复可见，
        // 避免切到牌组等覆盖层再切回来时挂卡一直不显示。
        Visible = true;
    }

    private void OnChanged(Player player)
    {
        if (player != _owner) return;
        if (!CanShow()) return;      // 不在战斗画面/生物已不在场时别抢着建节点（下一帧 _Process 会统一收尾）
        Rebuild(force: true);
    }

    /// <summary>挂在角色头顶（血条 / 能力图标区）上方，水平居中。</summary>
    private void RefreshPosition()
    {
        var creatureNode = _creature?.GetCreatureNode();
        if (creatureNode is null) return;

        // 首选血条 / 能力区（NCreature._stateDisplay = "%HealthBar"）的顶边，挂在它上方 ⇒ 既在角色头顶上方、又不遮血条与能力图标；
        // 拿不到该节点时降级到碰撞盒顶部（GetTopOfHitbox）。
        float centerX, topY;
        if (creatureNode.GetNodeOrNull<NCreatureStateDisplay>("%HealthBar") is { } stateDisplay)
        {
            centerX = stateDisplay.GlobalPosition.X + stateDisplay.Size.X * 0.5f;
            topY = stateDisplay.GlobalPosition.Y;
        }
        else
        {
            var top = creatureNode.GetTopOfHitbox();
            centerX = top.X;
            topY = top.Y;
        }

        GlobalPosition = new Vector2(
            centerX - Size.X * 0.5f,
            topY - Size.Y - HeadGap);
    }

    private bool CanShow()
    {
        if (_owner is null || _creature is null || !_creature.IsAlive) return false;
        if (HangingEmotionOrbs.Count(_owner) == 0) return false;

        // 只在战斗主界面显示：开牌组/暂停/地图等覆盖界面时隐藏（学艾玛"耳朵"，不浮在别的界面上）。
        if (!IsCombatScreenActive()) return false;

        // 生物节点已不在场景树里（战斗结束、切场景）则不显示。
        return _creature.GetCreatureNode() is { } creatureNode && creatureNode.IsVisibleInTree();
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
