using ManosabaLin.Characters.Common;

namespace ManosabaLin.Characters.Ananlin.Capabilities;

/// <summary>
/// 安心组件（由【安安的素描本】在回合开始时随机给予一张手牌）：
/// 打出带本组件的卡后，若下一张打出的牌与其类型相同，则获得 1 层【安心】。
/// 组件随之被消耗，素描本会重新随机给予一张手牌安心组件。
/// </summary>
[RegisterModelCapability]
public sealed class AnanlinReassuranceMarkCapability : ManosabaCardCapability
{
    // 显式指定本地化前缀：能力注册 ID 是大写形式（MANOSABA_LIN_MODELCAPABILITY_*），
    // 与本地化文件中的“ManosabaLin.ClassName”键不匹配，基类默认用 Id.Entry 会取不到文本。
    private const string LocPrefix = "ManosabaLin.AnanlinReassuranceMarkCapability";

    protected override string LocKeyPrefix => LocPrefix;
}
