using MinionLib.Component.Core;
using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.Components;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Ema.Cards;

[RegisterCard(typeof(LinCardPool))]
public sealed class TransformationMagic : ManosabaCardTemplate
{
    public TransformationMagic() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get { yield return CardKeyword.Retain; }
    }

    protected override IEnumerable<ICardComponent> CanonicalComponents => [new UniqueComponent()];

    // 回合开始时（在手牌中）：可选择 1 张手牌，本牌变身为该牌的复制。
    // 对应本地化 "MANOSABA_LIN_CARD_TRANSFORMATION_MAGIC.description"。
    protected override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
    {
        var source = this;

        if (player != source.Owner) return;
        if (source.Pile?.Type != PileType.Hand) return;

        // CardCmd.Transform 会直接读取 CombatState，先确认确实处于战斗中
        if (source.CombatState == null) return;

        // 无法变形的牌（如带「永恒」）直接跳过
        if (!source.IsTransformable) return;

        // 除自身外没有可选的手牌时跳过，避免弹出无目标的选择界面
        if (PileType.Hand.GetPile(source.Owner).Cards.All(card => card == source)) return;

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, 1);
        var selected = await CardSelectCmd.FromHand(choiceContext, source.Owner, prefs, card => card != source, this);
        var target = selected.FirstOrDefault();

        // 选择 0 张 = 本回合不变身，本牌保留在手牌
        if (target == null) return;
        if (target.Pile?.Type != PileType.Hand || target.Owner != source.Owner) return;

        // 完整复制目标牌：升级等级、关键词、能量/星星费用改动、附魔、诅咒、组件全部一并带走。
        // CreateClone 是基底为「复制一张战斗中的牌」提供的实现（原版 DualWield 同款），
        // 克隆牌会继承原牌当前状态（IsClone），不会重跑进入战斗时的一次性初始化。
        var replacement = target.CreateClone();

        // 本牌在原牌位变身为该复制
        await CardCmd.Transform(source, replacement);
    }
}
