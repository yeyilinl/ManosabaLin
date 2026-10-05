using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Sherrylin.Components;
using ManosabaLin.Characters.Sherrylin.Orbs;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Component.Core;
using MinionLib.Component.Interfaces;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;

namespace ManosabaLin.Characters.Sherrylin;

public interface ICaseFileCard { }

[RegisterCard(typeof(SherrylinCardPool))]
// ⚠️ 这里原先**写死** `shouldShowInCardLibrary: false` ⇒ 所有 CaseFileCard 子类（15 张情绪卡）
//    都被 `NCardLibraryGrid._Ready()`（`if (allCard.ShouldShowInCardLibrary)`）挡在图鉴之外。
//    `ShouldShowInCardLibrary` 是 CardModel 上「构造时定死的只读自动属性」，子类**无法覆写**，
//    所以只能把开关提到构造函数上，由各张卡自己决定 —— 默认 false 保持既有卡完全不变。
public abstract class CaseFileCard<T>(int energyCost, CardRarity rarity, TargetType targetType,
    bool shouldShowInCardLibrary = false)
    : ManosabaCardTemplate(energyCost, CardType.Power, rarity, targetType, shouldShowInCardLibrary), ICaseFileCard
    where T : OrbModel
{
    protected override IEnumerable<ICardComponent> CanonicalComponents =>
        [new SherrylinOrbInitializerComponent()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        await OrbCmd.Channel<T>(choiceContext, Owner);
    }
}
