using Godot;
using ManosabaLin.Characters.Ema.Cards;
using ManosabaLin.Characters.Hiro.Cards;
using ManosabaLin.Characters.Sherrylin.Cards;
using ManosabaLin.Characters.Yalisalin;
using ManosabaLin.Characters.Yalisalin.Cards;
using ManosabaLin.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Cards;
using TestTheSpire;
using Xunit;

namespace ManosabaLin.Tests.Cases;

/// <summary>
///     角色卡框节点（<c>CharacterCardFramePatch</c>）回归测试：节点是否挂上、层级是否紧贴
///     <c>TitleLabel</c> 之前、尺寸是否 = 贴图原始尺寸 × 系数（而不是被 <c>TextureRect</c> 的最小尺寸顶回原尺寸）、
///     以及对象池复用（换掉模型）后是否清干净；另有一条覆盖「命名空间段 ≠ 美术目录名」的角色。
/// </summary>
public sealed class CharacterCardFrameTests : CombatTestSuite
{
    private const string NodeName = "ManosabaLinCardFrame";

    private const string ContainerPath = "CardContainer";

    /// <summary>卡面尺寸（%Frame 的 300x422）。</summary>
    private const float CardWidth = 300f;

    private const float CardHeight = 422f;

    /// <summary>
    ///     与 <c>CharacterCardFramePatch.FrameScale</c> 保持一致：改补丁的系数就得同步改这里
    ///     （值不符会直接让下面的尺寸断言失败，正好当同步哨兵）。
    /// </summary>
    private const float FrameScale = 0.33f;

    protected override void ConfigureBattle(CombatTestBattleBuilder battle)
    {
        battle
            .Player<Yalisalin>()
            .AddEnemy<BigDummy>()
            .WithSeed("manosaba-character-card-frame");
    }

    [Fact]
    public async Task 卡框节点紧贴TitleLabel之前且换模型后清理()
    {
        var strike = await AddToHand<YalisalinAttack>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(strike, EnemyAt(0));
        await WaitForIdle();

        var packed = ResourceLoader.Load<PackedScene>("res://scenes/cards/card.tscn");
        var card = (NCard)packed.Instantiate();
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(card);

        try
        {
            var container = card.GetNodeOrNull<Control>(ContainerPath);
            Assert.NotNull(container);

            card.Model = strike;
            await WaitForIdle();

            var frame = container!.GetNodeOrNull<TextureRect>(NodeName);
            Assert.NotNull(frame);

            var texture = frame!.Texture;
            Assert.NotNull(texture);

            // 尺寸 = 贴图原始尺寸 × 系数。各角色的卡框图**像素尺寸并不完全相同**（雪莉那张是
            // 1103x1426，其余是 1105x1423），所以别在这里写死具体像素。
            Assert.Equal(texture!.GetWidth() * FrameScale, frame.Size.X, 0.5d);
            Assert.Equal(texture.GetHeight() * FrameScale, frame.Size.Y, 0.5d);

            // 系数生效：节点比贴图小（说明 IgnoreSize 压住了 TextureRect 的最小尺寸）；节点比卡面大
            // 但也没大到离谱（0.33 ⇒ 约 365x470），且保持贴图比例。
            Assert.True(frame.Size.X < texture.GetWidth(), $"节点宽应小于贴图宽，实际 {frame.Size.X}");
            Assert.True(frame.Size.Y < texture.GetHeight(), $"节点高应小于贴图高，实际 {frame.Size.Y}");
            Assert.True(frame.Size.X > CardWidth * 1.05f && frame.Size.X < CardWidth * 1.6f,
                $"节点宽应在卡面的 1.05~1.6 倍之间，实际 {frame.Size.X}");
            Assert.True(frame.Size.Y > CardHeight * 1.02f && frame.Size.Y < CardHeight * 1.3f,
                $"节点高应在卡面的 1.02~1.3 倍之间，实际 {frame.Size.Y}");
            Assert.Equal(
                texture.GetWidth() / (double)texture.GetHeight(),
                frame.Size.X / (double)frame.Size.Y,
                0.001d);

            // 层级：紧贴在 TitleLabel 之前（底 → 顶：%TitleBanner → 本节点 → %TitleLabel → …）。
            var titleIndex = -1;
            for (var i = 0; i < container.GetChildCount(); i++)
            {
                if (container.GetChild(i).Name != "TitleLabel")
                    continue;

                titleIndex = i;
                break;
            }

            Assert.True(titleIndex > 0, "没找到 TitleLabel，无法判断卡框层级");
            Assert.Equal(titleIndex, frame.GetIndex() + 1);

            // 对象池复用：模型被换掉（含置空）后必须清掉节点，不留上一张卡的残影。
            card.Model = null;
            await WaitForIdle();
            Assert.Null(container.GetNodeOrNull(NodeName));
        }
        finally
        {
            card.QueueFree();
        }
    }

    /// <summary>
    ///     各角色的卡挂各自角色的卡框图。这几张的美术命名跟「命名空间段 + card.png」的默认规则都不一致：
    ///     艾玛的卡在 <c>Characters.Ema.*</c> 而图在 <c>images/characters/Emalin/emalincard.png</c>；
    ///     希罗目录是 <c>Hiro</c> 但文件叫 <c>hirolincard.png</c>；雪莉目录同名前缀但叫
    ///     <c>sherrylincard.png</c> —— 全靠候选回退命中。
    /// </summary>
    [Fact]
    public async Task 各角色的卡各挂自己的卡框()
    {
        // 先跑一个真实 GameAction，否则 harness 判「completed without executing any combat GameAction」。
        var strike = await AddToHand<YalisalinAttack>();
        await PlayerCmd.SetEnergy(10, Player);
        await WaitForIdle();
        await Play(strike, EnemyAt(0));
        await WaitForIdle();

        var emalinCard = await AddToHand<PrisonLaw>();
        var hiroCard = await AddToHand<Amm>();
        var sherrylinCard = await AddToHand<SherrylinCardTen>();

        var packed = ResourceLoader.Load<PackedScene>("res://scenes/cards/card.tscn");

        foreach (var (model, expectedPath) in new (CardModel Model, string Path)[]
                 {
                     (emalinCard, "emalincard.png".CharacterImgPath("Emalin")),
                     (hiroCard, "hirolincard.png".CharacterImgPath("Hiro")),
                     (sherrylinCard, "sherrylincard.png".CharacterImgPath("Sherrylin")),
                 })
        {
            var card = (NCard)packed.Instantiate();
            ((SceneTree)Engine.GetMainLoop()).Root.AddChild(card);

            try
            {
                card.Model = model;
                await WaitForIdle();

                var frame = card.GetNodeOrNull<Control>(ContainerPath)?.GetNodeOrNull<TextureRect>(NodeName);
                Assert.NotNull(frame);

                // 必须是该角色自己那张图，而不是「恰好也挂上了」别的角色的图。
                Assert.Equal(expectedPath, frame!.Texture!.ResourcePath);
                Assert.Equal(frame.Texture.GetWidth() * FrameScale, frame.Size.X, 0.5d);
            }
            finally
            {
                card.QueueFree();
            }
        }
    }
}
