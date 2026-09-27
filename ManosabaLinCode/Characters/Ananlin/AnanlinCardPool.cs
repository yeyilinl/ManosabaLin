using Godot;
using ManosabaLin.Extensions;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace ManosabaLin.Characters.Ananlin;

public class AnanlinCardPool : TypeListCardPoolModel, IModColorfulPhilosophersCardPool
{
    private const string CharacterIdLower = "ananlin";

    public override string Title => Ananlin.CharacterId;
    public override string EnergyColorName => CharacterIdLower;

    public override string BigEnergyIconPath => "characters/Ananlin/ananlin_energy.png".ImagePath();
    public override string TextEnergyIconPath => "characters/Ananlin/ananlin_energy_small.png".ImagePath();

    // 卡图/卡框染色：所有角色的占位角色都是 ironclad，卡框底图是原版红橙，
    // 靠这里的 HSV 参数把它染成角色色 —— h=0.68（≈244.8°）即夏目安安的 #6666cc 蓝紫，
    // s=0.55 去饱和、v=1.05 提亮。本 mod 其它池同规则（Hiro h=0 红、Sherrylin 0.55 青、Ema 0.953 粉）。
    // ⚠️ 千万不要改成恒等 HSV(0,1,1)：那会让整池卡图退回原版红橙，和亚里沙（恒等=红）撞色。
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateHsvShaderMaterial(0.68f, 0.55f, 1.05f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override Color DeckEntryCardColor => Ananlin.Color;
    public override bool IsColorless => false;
}
