using Godot;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;

namespace ManosabaLin.Characters.Ema.Vfx;

/// <summary>
/// 魔女化翅膀特效（ema_wing.tscn 根节点脚本）。
/// 挂在 Creature.GetBackVfxContainer()（官方“角色背后”VFX 容器）下，
/// 天然绘制在所有角色立绘后面，无需手动调 ZIndex。
/// </summary>
public partial class EmaFormVfx : Node2D
{
    /// <summary>相对角色 VfxSpawnPosition 的偏移（往左 20、上移 45）。</summary>
    private static readonly Vector2 BodyOffset = new(-20f, -45f);

    private Node2D _root = null!;
    private AnimationPlayer _animPlayer = null!;

    /// <summary>跟随的目标生物（艾玛本人）。</summary>
    public Creature? Target { get; set; }

    /// <summary>可选的显式位置源（优先级高于 Target）。</summary>
    public Func<Vector2?>? PositionSource { get; set; }

    public override void _Ready()
    {
        _root = GetNode<Node2D>("Root");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Callable.From(() => _root.Visible = true).CallDeferred();
        _animPlayer.Play("spread");
        _animPlayer.AnimationFinished += OnAnimationFinished;
    }

    public override void _Process(double delta)
    {
        if (Target == null) return;

        var pos = PositionSource?.Invoke();
        if (pos != null)
        {
            GlobalPosition = pos.Value + BodyOffset;
            return;
        }

        var visuals = Target.GetCreatureNode()?.Visuals;
        if (visuals == null) return;
        GlobalPosition = visuals.VfxSpawnPosition.GlobalPosition + BodyOffset;
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (animName == "spread")
        {
            _animPlayer.Play("idle");
        }
    }
}
