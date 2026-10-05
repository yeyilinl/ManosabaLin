using System.Linq;
using System.Threading.Tasks;

namespace ManosabaLin.Characters.Yalisalin.Powers;

/// <summary>
/// 点火（亚里沙的专属魔法卡能力）：
/// 回合开始或回合结束时，按当前层数 N 随机攻击 N 次：
/// 每次随机选择全场目标（敌我均可），对其造成等于当前层数的伤害。
/// 命中敌方：所有友方获得 1 点格挡；若该敌方没有易伤，给予 1 层易伤。
/// 命中友方：改为造成 1 点伤害，然后本能力 +2 层。
/// </summary>
[RegisterPower]
public sealed class IgnitePower : ManosabaPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    ///     点火的一次攻击命中<b>友方</b>（含玩家自己；<c>combatState.Allies</c> 包含自己）后触发。
    ///     参数为点火能力的持有者（<see cref="Player" />）。
    ///     「自罚上瘾」的【原罪】引擎靠它把「点火打到自己人」转成【余火】。
    /// </summary>
    public static event Action<PlayerChoiceContext, Player>? AllyHit;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.Creature != Owner) return;
        await TriggerIgnite(choiceContext);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await TriggerIgnite(choiceContext);
    }

    /// <summary>
    ///     立刻执行一次完整点火（按当前层数随机攻击 N 次）。回合开始/结束自动调用；
    ///     卡牌（如「自罚上瘾」）也可以手动调用它来「触发一次点火能力的攻击」。
    /// </summary>
    internal async Task TriggerIgnite(PlayerChoiceContext choiceContext)
    {
        if (Owner?.CombatState is not { } combatState) return;
        if (Owner?.Player is not { } player) return;

        var hitCount = (int)Amount;
        if (hitCount <= 0) return;

        Flash();

        var rng = player.RunState.Rng.CombatTargets;

        for (var i = 0; i < hitCount; i++)
        {
            var allies = combatState.Allies.Where(c => c.IsAlive).ToArray();
            var enemies = combatState.Enemies.Where(c => c.IsAlive).ToArray();
            var candidates = allies.Concat(enemies).ToArray();
            if (candidates.Length == 0) break;

            var target = rng.NextItem(candidates);
            if (target == null) continue;

            var isEnemy = enemies.Contains(target);

            // 命中敌方：造成等于当前层数一半的伤害（向下取整，至少 1）
            // 命中友方：改为造成 1 点伤害
            // 能力来源伤害统一用 Unpowered（不受力量/魔女化影响，可被格挡）
            var damage = isEnemy ? Math.Max(1m, Math.Floor(Amount / 2m)) : 1m;
            await CreatureCmd.Damage(
                choiceContext, target, damage,
                ValueProp.Unpowered,
                Owner, null, null);
            if (Owner == null) return;

            if (isEnemy)
            {
                // 所有友方获得 1 点格挡（卡面文案为「所有友方」，不是随机一名）
                foreach (var ally in allies)
                {
                    if (ally.IsAlive)
                        await CreatureCmd.GainBlock(ally, 1m, ValueProp.Move, null);
                }

                // 被攻击的敌方没有易伤时，给予 1 层易伤
                if (target.GetPower<VulnerablePower>() == null)
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1m, Owner, null, false);
            }
            else
            {
                // 命中友方：本能力 +2 层
                await PowerCmd.ModifyAmount(choiceContext, this, 2m, Owner, null);

                // 广播「点火命中友方」（卡牌/能力可订阅，例如【原罪】）
                AllyHit?.Invoke(choiceContext, player);
            }
        }
    }
}
