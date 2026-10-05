using ManosabaLin.Characters.Common;
using ManosabaLin.Characters.Common.Components;
using ManosabaLin.Characters.Hiro.Cards;
using ManosabaLin.Characters.Yalisalin.Components;
using ManosabaLin.Characters.Yalisalin.Powers;
using ManosabaLin.Characters.Yalisalin.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ManosabaLin.Characters.Yalisalin.Cards;

/// <summary>
///     自罚上瘾（3 费 · 技能 · 金 · 自身）：
///     幕一【原罪】消耗所有牌里全部的【原罪诅咒】，每消耗 1 张随机对 1 名敌人造成 6 点伤害；
///     幕二【火色】随机给予 6 格【火色】，然后消耗这 6 格；
///     幕三【余火】消耗所有牌里全部带【余火】的牌，每消耗 1 张触发 1 次【点火】的攻击；
///     幕四【魔女化】消耗你全部的【魔女化】：每 50 层获得 1 点能量，若消耗达 100 层获得【原罪】。
///     <para>升级：3 费 → 2 费（数值不变）。</para>
/// </summary>
[RegisterCard(typeof(YalisalinCardPool))]
public sealed class ZiFaShangYin()
    : ManosabaCardTemplate(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    /// <summary>
    ///     「原罪」显示组件：卡面顶部印 [原罪]，悬浮显示卡名与结局文案
    ///     （键 <c>ManosabaLin.SelfPunishmentAddiction.prefix / .hovertip.*</c>），无游戏逻辑。
    /// </summary>
    protected override IEnumerable<ICardComponent> CanonicalComponents =>
        [new SelfPunishmentAddiction()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new IntVar("FireColor", 6)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<IgnitePower>();
            yield return HoverTipFactory.FromPower<YalisalinOriginalsinPower>();
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        var owner = Owner;
        var creature = owner.Creature;
        var rng = owner.RunState.Rng.CombatTargets;

        // ===== 幕一【原罪】：消耗所有【原罪诅咒】，每张随机对 1 名敌人造成 6 点伤害 =====
        var sins = ScanAllCards(owner)
            .Where(static card => card.HasComponent<Originalsin>())
            .ToList();

        foreach (var sin in sins)
        {
            await CardCmd.Exhaust(choiceContext, sin);

            var enemies = AliveEnemies(creature);
            if (enemies.Count == 0) continue;

            var target = rng.NextItem(enemies);
            if (target == null) continue;

            await CreatureCmd.Damage(
                choiceContext,
                target,
                DynamicVars.Damage.BaseValue,
                ValueProp.Unpowered | ValueProp.Move,
                this,
                cardPlay);
        }

        // ===== 幕二【火色】：随机给 1 名敌人 6 格，然后把这 6 格消耗掉 =====
        var fireTarget = rng.NextItem(AliveEnemies(creature));
        if (fireTarget != null)
        {
            await YalisalinFireColorSystem.GiveFireColor(
                choiceContext, owner, fireTarget, DynamicVars["FireColor"].IntValue, this);

            if (YalisalinFireColorSystem.TryGetHairpin(owner, out var hairpin))
                await hairpin.ConsumeAllFireColor(choiceContext, fireTarget, this);
        }

        // ===== 幕三【余火】：消耗所有带【余火】的牌，每张触发 1 次【点火】的攻击 =====
        var embers = ScanAllCards(owner)
            .Where(static card => YalisalinFireComponentRules.HasFireComponent(card))
            .ToList();

        foreach (var ember in embers)
        {
            await CardCmd.Exhaust(choiceContext, ember);

            // 没有【点火】层数时按原逻辑空转（用户 10-03 裁定）
            if (creature.GetPower<IgnitePower>() is not { } ignite) continue;
            await ignite.TriggerIgnite(choiceContext);
        }

        // ===== 幕四【魔女化】：一次清空，每 50 层给 1 点能量；消耗达 100 层给【原罪】 =====
        var withPower = creature.GetPower<WithPower>();
        var consumed = (int)(withPower?.Amount ?? 0m);

        var energy = consumed / 50;
        if (energy > 0)
            await PlayerCmd.GainEnergy(energy, owner);

        if (withPower != null)
            await PowerCmd.Remove(withPower);

        if (consumed >= 100)
            await PowerCmd.Apply<YalisalinOriginalsinPower>(
                choiceContext, creature, 1m, creature, this, false);
    }

    protected override void OnUpgrade(ComponentContext componentContext)
    {
        EnergyCost.UpgradeBy(-1);
    }

    /// <summary>手 / 抽 / 弃 三堆里当前可被操作的牌（排除正被选择界面锁定的牌）。</summary>
    private static IEnumerable<CardModel> ScanAllCards(Player owner)
    {
        foreach (var pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
            foreach (var card in pileType.GetPile(owner).Cards)
                if (!SamePlaceTruth.IsSelectionLocked(card))
                    yield return card;
    }

    private static List<Creature> AliveEnemies(Creature creature)
    {
        return creature.CombatState is { } combatState
            ? combatState.Enemies.Where(static enemy => enemy.IsAlive).ToList()
            : [];
    }
}
