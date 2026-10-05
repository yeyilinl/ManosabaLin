using ManosabaLin.Characters.Common.Components.Abstracts;

namespace ManosabaLin.Characters.Yalisalin.Components;

/// <summary>
///     自罚上瘾的「原罪」显示组件（与正义的执行者 / 受厌恶之人 / 破坏侦探 / 编织谎言的沉睡公主同族）。
///     <para>
///         卡面：把 <c>ManosabaLin.SelfPunishmentAddiction.prefix</c> 印在描述最前面；
///         悬浮：显示 <c>.hovertip.title</c> + <c>.hovertip.description</c>。
///         两者都由 <see cref="KeywordLikeComponent" /> 依据组件 ID 自动取词，本类无游戏逻辑。
///     </para>
///     <para>
///         组件 ID 由 MinionLib 源生成器按「根命名空间 + 类型名」推出
///         ⇒ 本类固定为 <c>ManosabaLin.SelfPunishmentAddiction</c>，本地化键必须与之一致。
///     </para>
/// </summary>
public sealed partial class SelfPunishmentAddiction : KeywordLikeComponent
{
}
