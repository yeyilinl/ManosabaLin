using ManosabaLin.Characters.Common.Components;
using ManosabaLin.Characters.Sherrylin.Orbs;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Sherrylin.Cards.Emotions;

[RegisterCard(typeof(LinCardPool))]
// 图鉴可见（第 4 参 = shouldShowInCardLibrary；基类默认 false）。
public sealed class EmotionFriendship() : CaseFileCard<EmotionFriendshipOrb>(-1, CardRarity.Ancient, TargetType.Self, true)
{
    public override int MaxUpgradeLevel => 0;
    protected override IEnumerable<ICardComponent> CanonicalComponents =>
        [new UniqueComponent()];
}
