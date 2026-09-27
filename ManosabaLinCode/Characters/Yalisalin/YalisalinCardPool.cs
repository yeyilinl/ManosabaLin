using Godot;
using ManosabaLin.Extensions;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace ManosabaLin.Characters.Yalisalin;

public class YalisalinCardPool : TypeListCardPoolModel, IModColorfulPhilosophersCardPool
{
    private const string CharacterIdLower = "yalisalin";

    public override string Title => Yalisalin.CharacterId;
    public override string EnergyColorName => CharacterIdLower;

    public override string BigEnergyIconPath => "characters/Yalisalin/yalisalin_energy.png".ImagePath();
    public override string TextEnergyIconPath => "characters/Yalisalin/yalisalin_energy_small.png".ImagePath();

    // yalisalincard 卡框美术自带正确配色，用恒等 HSV（h=0，s=1，v=1）走原版着色管线，
    // 整池卡框不再被主题色染色。
    public override Material? PoolFrameMaterial => MaterialUtils.CreateUnmodulatedHsvShaderMaterial();

    public override Color DeckEntryCardColor => Yalisalin.Color;
    public override bool IsColorless => false;
}
