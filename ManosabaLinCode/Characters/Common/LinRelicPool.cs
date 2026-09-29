using Godot;
using ManosabaLin.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ManosabaLin.Characters.Common;

/// <summary>
///     本 mod 的共享遗物池（「Lin 池」）。
///     <para>
///         它不绑定任何角色，登记为 <b>共享遗物池</b> ⇒ 进入 <c>ModelDb.AllSharedRelicPools</c>，
///         所有角色都能持有池内的遗物。与 <c>LinCardPool</c>（<c>[RegisterSharedCardPool]</c>）同构。
///     </para>
///     <para>
///         海克斯联动遗物注册在这个池里；装了海克斯时它们**额外**会通过
///         <c>HextechRunesInterop.RegisterPlayerRune</c> 拿到「海克斯符文」的第二身份（软依赖、零 patch）。
///     </para>
/// </summary>
[RegisterSharedRelicPool]
public class LinRelicPool : TypeListRelicPoolModel
{
    private const string CharacterIdLower = "linenergy";

    public override string EnergyColorName => CharacterIdLower;

    public override Color LabOutlineColor => new("B72222");

    public override string BigEnergyIconPath => "characters/Hiro/linenergy.png".ImagePath();

    public override string TextEnergyIconPath => "characters/Hiro/linenergy_small.png".ImagePath();
}
