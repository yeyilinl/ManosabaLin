using ManosabaLin.Characters.Ananlin.Relics;
using MinionLib.Component.Core;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace ManosabaLin.Characters.Ananlin.Cards;

/// <summary>
///     借来的留白书页 - 由「书页打击」发给所有人的 Token。
///     <para>
///         与普通 <see cref="MarginPage" /> 的区别：普通书页只认「持有者自己的素描本」，
///         队友拿到就是一张没有效果的牌；本 Token 在发放时把<b>发放者（夏目安安）记录的卡池</b>
///         快照进自己的 <see cref="SavedProperty" />，因此队友打出时用的是安安的卡池。
///     </para>
/// </summary>
[RegisterCard(typeof(AnanlinCardPool))]
public sealed class BorrowedMarginPage() : AnanlinNonRandomCardTemplate(
    0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
{
    [SavedProperty] public string Pool1 { get; set; } = string.Empty;

    [SavedProperty] public string Pool2 { get; set; } = string.Empty;

    [SavedProperty] public string Pool3 { get; set; } = string.Empty;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get { yield return CardKeyword.Exhaust; }
    }

    /// <summary>发放时把发放者记录的卡池条目写进本卡（最多 3 个）。</summary>
    internal void SetRecordedPools(IEnumerable<string> entries)
    {
        var slots = entries
            .Where(static s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .Take(AnansSketchbook.MaxRecordedPools)
            .ToArray();

        Pool1 = slots.Length > 0 ? slots[0] : string.Empty;
        Pool2 = slots.Length > 1 ? slots[1] : string.Empty;
        Pool3 = slots.Length > 2 ? slots[2] : string.Empty;
    }

    internal IReadOnlyList<string> GetRecordedPools()
    {
        return new[] { Pool1, Pool2, Pool3 }
            .Where(static s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToArray();
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await AnansSketchbook.ResolveBorrowedMarginPage(
            choiceContext, this, Owner, GetRecordedPools());
    }
}
