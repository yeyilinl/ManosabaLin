using ManosabaLin.Characters.Common.AncientCurses.Powers;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
/// 罪业圣盾（2 费技能・罕见）：
/// 获得 5 点格挡；本场（当前战斗节点）每发生 1 次宽恕或自惩，额外获得 1 点格挡。
/// 升级：费用 -1。
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class ZuiYeShengDun() : ManosabaCardTemplate(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
    {
        var owner = Owner;
        var me = owner.Creature;

        // 本场（当前战斗节点）宽恕/自惩总次数：战斗内隐藏计数能力，随战斗结束自动清除
        var bonus = me.GetPower<OriginalsinResolveCounterPower>()?.Amount ?? 0m;

        var block = DynamicVars.Block.BaseValue + bonus;
        await CreatureCmd.GainBlock(me, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }
}