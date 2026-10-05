using System.Threading.Tasks;
using ManosabaLin.Characters.Ananlin.Cards;
using ManosabaLin.Characters.Ananlin.Powers;
using ManosabaLin.Characters.Common;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ManosabaLin.Characters.Common.LinRelics;

/// <summary>
///     联动遗物（安安 2/3）：**【安心】按层数追加额外效果**。
///     <list type="bullet">
///         <item>打出<b>[b]攻击[/b]牌</b>：获得等同于当前【安心】层数的<b>活力</b>（<see cref="VigorPower" />）。</item>
///         <item>打出<b>[b]技能[/b]牌</b>：获得等同于当前【安心】层数的<b>格挡</b>。</item>
///         <item>
///             打出<b>[b]能力[/b]牌</b>：<b>消耗当前所有【安心】</b>，
///             向手牌加入<b>等量</b>的【留白书页】，<b>然后</b>再获得 3 层【安心】。
///         </item>
///     </list>
///     <para>
///         ⚠️ 用户 2026-09-29 裁定：能力牌那条的「消耗安心」**走现成的
///         <see cref="AnanlinCardHelpers.LosePeaceOfMind" />** ⇒ 一次性消耗 ≥2 层时，
///         照常触发【安心】原有的「选择一张手牌获得【重放1】」。
///     </para>
///     <para>
///         ⚠️ 能力牌那条按字面执行：**无条件**先消耗（有几层耗几层）、加等量留白书页、再补 3 层
///         ⇒ 安心为 0 时也照常补到 3 层；上限仍由 <c>AnanlinPeaceOfMindPower.MaxStacks</c>（3）兜底。
///     </para>
///     <para>
///         层数快照在**本次牌真正打出的那一刻**读取（<c>AfterCardPlayed</c>），
///         即「当前安心层数」，与卡牌自身结算无关。
///     </para>
/// </summary>
[RegisterRelic(typeof(LinRelicPool))]
public sealed class HextechPeaceOfMindEcho : ManosabaRelicTemplate
{
    /// <summary>能力牌结算完之后补回的【安心】层数。</summary>
    private const int PeaceRefund = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>
    ///     该玩家是否持有本遗物。
    ///     （与其余联动遗物同构的查询口，供将来的 patch / helper 复用；本遗物自身走 <c>AfterCardPlayed</c>，不依赖它。）
    /// </summary>
    internal static bool IsActiveFor(Creature? creature)
        => creature?.Player?.Relics.OfType<HextechPeaceOfMindEcho>().Any() == true;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner) return;

        var peace = Owner.Creature.GetPower<AnanlinPeaceOfMindPower>();
        var stacks = (int)(peace?.Amount ?? 0m);

        switch (cardPlay.Card.Type)
        {
            case CardType.Attack when stacks > 0:
                Flash();
                await PowerCmd.Apply<VigorPower>(
                    choiceContext, Owner.Creature, stacks, Owner.Creature, cardPlay.Card);
                break;

            case CardType.Skill when stacks > 0:
                Flash();
                await CreatureCmd.GainBlock(Owner.Creature, stacks, ValueProp.Move, cardPlay);
                break;

            case CardType.Power:
                Flash();
                // 走现成工具：一次消耗 ≥2 层会照常弹「选择一张手牌获得【重放1】」（用户 2026-09-29 裁定）。
                var consumed = await cardPlay.Card.LosePeaceOfMind(choiceContext, stacks);
                await AddMarginPagesToHand(consumed);
                await PowerCmd.Apply<AnanlinPeaceOfMindPower>(
                    choiceContext, Owner.Creature, PeaceRefund, Owner.Creature, cardPlay.Card);
                break;
        }
    }

    /// <summary>向手牌加入 <paramref name="count" /> 张【留白书页】。</summary>
    private async Task AddMarginPagesToHand(int count)
    {
        if (count <= 0) return;
        if (Owner.Creature.CombatState is not { } combatState) return;

        for (var i = 0; i < count; i++)
        {
            var page = combatState.CreateCard<MarginPage>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(page, PileType.Hand, Owner);
        }
    }
}
