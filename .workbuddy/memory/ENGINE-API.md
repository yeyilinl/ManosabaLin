# ManosabaLin 引擎 API 速查（从 MEMORY.md 拆出）

> 本文件**不会自动注入**，由 `MEMORY.md` 的「源码与引擎 API」一节指过来。
> 需要查具体签名/行为时 Read 本文件；更细的背景见 `.workbuddy/memory/<日期>.md`。
> 源码：本体反编译 `.local/probe_engine/decomp/`；MinionLib 等库真源码 `.local/external/`
> （`.tmp_minionlib/` 只是 dump）。⚠️ `.local/` 被 gitignore ⇒ **全仓检索会跳过，查源码必须显式传 `path=`**。

## 类型与判空

- `PowerModel.Owner` 就是 `Creature`（写 `Owner.Creature` 报 CS1061）。
- `NCard.Model` **是公开可空的 `CardModel?`**（`NCard.cs:642`）—— 之前记过「NCard 没有公开 Model」是**错的**，别再据此绕路。
- `PowerCmd.Apply<T>(PlayerChoiceContext, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)` 返回 `Task<T?>` **必判空**；另有 `IEnumerable<Creature>` 重载返回 `IReadOnlyList<T>`。
- `PowerModel.Amount` 是 **int**；`Creature.GetPower<T>()` 返回 `T?`（`Creature.cs:571`）。

## 时机 / 钩子

- 团队型效果按卡面保持团队范围，不收窄（审判开庭先例）。
- 可点击能力（`ActionModel`）要「无限次」就什么限次都别加：层数降 0 会被引擎移除 ⇒ 别用 `DecrementAfterAct`，施加 1 层即可（`IAmCorrectAction` 先例）。
- 「回合开始消失」只认**持有者自己的**回合开始（`player?.Creature != Owner`）。
- 「回合开始（在手牌中）」用卡牌 `AfterPlayerTurnStart(ctx, player, compCtx)` + 自判 `Pile?.Type == PileType.Hand`。
- 限伤/减伤都挂 `Hook.ModifyHpLost`（`Hook.cs:1725`，唯一调用点在 `CreatureCmd.Damage` 管线内）；`CreatureCmd.SetCurrentHp` / `Kill` 走 `SetCurrentHpInternal` / `LoseHpInternal` 直接写 `CurrentHp`，**绕过**全部限伤。
- `DamageResult.WasTargetKilled` = `CurrentHp > 0 && amount >= CurrentHp`（`DamageResult.cs:99`）⇒ 只表示「这一击把血打空」，**即使随后被复活也仍为 true**。要断言「限伤没能阻止即死」用它，别用 `IsAlive`。
- `Creature.IsAlive => CurrentHp > 0`；`SetMaxHpInternal` 会把 `CurrentHp` 夹到 `MaxHp`（压血量做测试的常用手法）。

## 回合结束顺序（踩坑高发区）

- **完整顺序**（`CombatManager.cs` + `PlayerTurnPhase.cs` 文档注释）：
  1. `Hook.BeforeSideTurnEnd`（`:1556` 玩家侧 / `:1691` 敌方侧）
  2. `DoTurnEnd`（`:1599`）：手牌里带 `HasTurnEndInHandEffect` 的牌结算 + `Ethereal`（虚化在这里被消耗）
  3. `Hook.BeforeFlush`（`:1584`）
  4. **清手牌** `FlushPlayerHand`（`:1760`，在 `EndPlayerTurnPhaseTwoInternal` 里）—— `ShouldRetainThisTurn` 在这一步才生效
  5. `Hook.AfterSideTurnEnd`（`:1771` 玩家侧 / `:1696` 敌方侧）
  ⇒ 想「回合结束自动保留后再触发效果」，且**不希望效果新塞进手牌的牌被同一回合清手牌冲掉**，就放 `AfterSideTurnEnd`；放 `BeforeSideTurnEnd` 会早于清手牌。
- ⚠️ `HasTurnEndInHandEffect` + `OnTurnEndInHand` **与「保留」互斥**：`ResolveTurnEndCardEffects`（`:1668`）结算完无条件把牌塞进**弃牌堆**（虚化则消耗）⇒ 想回合结束保留的卡**不能**用这套。MinionLib 组件侧对应 `HasTurnEndInHandEffect` / `OnTurnEndInHandPrefix|Postfix`（`CardComponent_Hooks.cs:47,52`）。
- 组件层钩子来源 MinionLib：`CardComponent_Hooks.cs` 提供 `BeforeSideTurnEndPostfix(:182)` / `AfterSideTurnEndPostfix(:192)` / `AfterPlayerTurnStartEarlyPostfix(:92)`（由 `ComponentsCardModel_Hooks.cs:537,1239,1317` 转发）。
- 原版「回合结束在手牌里」的标准写法看 `Regret.cs`：`BeforeSideTurnEnd` 先快照、`OnTurnEndInHand` 里结算。
- MinionLib 组件状态：`[ComponentState]` 只能序列化值类型 ⇒ 要记「是谁」就存值标识（如 `Player.NetId`，`Player.cs:48`）。

## 卡牌显示（NCard）

- 能量/星费的「×」：`_unplayableEnergyIcon` / `_unplayableStarIcon`（`NCard.cs:550/556`，私有）。
- 引擎**只在 `pileType == PileType.Hand && !Model.CanPlay(...)` 时**点亮，且 `Visible = !reason.HasResourceCostReason()`
  ⇒ 能量不够的那种「打不出」**不显示 ×**（数字变红），只有非资源类原因才画 ×。
- 选卡界面用 `PileType.None` 展示卡牌 ⇒ 一律不点亮 ×。想自己点亮就给 `UpdateEnergyCostVisuals`（`:972`）/ `UpdateStarCostVisuals`（`:1044`）加 Harmony 后置；二者都由 `UpdateVisuals`（`:861 → 875/876`）调用，**没有重载**。

## 构建 / 发布

- 新增 `.cs` 后缺 `.cs.uid` **不用手搓**：`dotnet publish`（Godot 导入 + savepack）会自动补齐。
- `-t:Compile` 会把游戏目录 DLL 换成 Debug 版（`CopyMod` 复制 `$(TargetPath)`）⇒ 收尾用
  `cp -f .godot/mono/temp/bin/ExportRelease/win-x64/ManosabaLin.dll "<Sts2Dir>/mods/ManosabaLin/"` 再核对 md5。

## 目标与阵营

- `TargetType.AnyAlly` = 「除自己以外」，`OnPlay` 仍守 `target.IsAlive && target.Side == Owner.Creature.Side`。
- 「友方」先例 `FakeDeath.cs`：能量 → `CombatState.Players`；回血/能力 → `Allies`。

## 选牌 / 造牌

- 别混用：某玩家自己的战斗牌堆 → `CardSelectCmd.FromCombatPile(ctx, pile, 玩家, prefs, filter)`；任意卡集合 → `FromSimpleGrid`；牌组升级 → `FromDeckForUpgrade(player, prefs)`；手牌 → `FromHand`。
- 复制战斗中的牌用 `source.CreateClone()`（`CardCmd.CreateCard` 会丢附魔/组件）。
- 变身用 `CardCmd.Transform(o, r)`，先守 `IsTransformable`。
- **变形/生成没有前置替换钩子** ⇒ 自写 Harmony 前缀（`ref Task __result` + `return false`，重入用静态标志）；`CardCmd.Transform` 没有 choiceContext，只能在 `CardModel.OnPlayWrapper` 上挂前缀拿到。

## 牌堆

- 组件方法在 `ComponentsCardModel` 上（**不在** `CardModel`）。
- 「打出后移除」→ `CardPileCmd.RemoveFromCombat(this)`。
- `CardPile` index 0 = 堆顶；`CardPileCmd.Add` 默认 Bottom，插堆顶必须传 `CardPilePosition.Top`。

## 伤害 / 攻击

- `DamageCmd.Attack(decimal damagePerHit)` 的 `AttackCommand.DamageProps` **默认就是 `ValueProp.Move`**（`AttackCommand.cs:127`）⇒ 普通攻击不必再显式传 props。
- `WithHitCount(n)` 只把**同一个** `_damagePerHit` 重复 n 次 ⇒ 「一段基础值 + 另一种数值的额外段」**必须写两条 `DamageCmd.Attack(...).Execute()`**。
- `AttackCommand.Execute` 内部：`validTargets = GetPossibleTargets().Where(c => c.IsAlive)`；`Count == 0 && combatState.IsLiveCombat()` 才 `break` ⇒ **战斗已经结束（非 live）时不会 break**，所以自己补的第二次攻击前应显式判 `target.IsAlive`。
- `VulnerablePower.ModifyDamageMultiplicative` = `DamageIncrease`（1.5m），且**只对 `props.IsPoweredAttack()` 生效**（= 有 `Move` 且无 `Unpowered`）。
- 多段攻击要用一个 `AttackCommand` 统一给钩子看时，用
  `await using var ctx = await AttackCommand.CreateContextAsync(CombatState, choiceContext, cardPlay)`
  → `CreatureCmd.Damage(...)` → `ctx.AddHit(results)`（`AttackContext` 内部对钩子只做 BeforeAttack/AfterAttack，实际目标由 `CreatureCmd.Damage` 决定；先例 `Omnislice.cs`）。
