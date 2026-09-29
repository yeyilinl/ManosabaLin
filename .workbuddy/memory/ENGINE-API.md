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
- ⭐ **`Hook.BeforeSideTurnEnd` 内部其实是三个子阶段，各遍历一遍全部监听器**（`Hook.cs:1240-1268`）：
  `BeforeSideTurnEndVeryEarly` → `BeforeSideTurnEndEarly` → `BeforeSideTurnEnd`。
  同一个模型可以只实现其中任意一个（回弹器 `PaelsEye.cs:118` 就用 `Early`；`RegenPower` 也用 `Early`）。
  ⚠️ **别只看 `AbstractModel` 的注释就把 `VeryEarly` 当成禁区** —— 它的 "CAREFUL! 通常该用 Early/Before" 指的是「别乱抢跑」；
  当你**需要让自己的改动被更早阶段的别人读到**时（先例：联动遗物 6 要在 `RegenPower.BeforeSideTurnEndEarly` 结算回血**之前**补【再生】层数），
  `VeryEarly` 就是唯一确定性的落点。同阶段内的先后**不可控**（同一列表顺序遍历），跨阶段才可控。
- ⚠️ 由此推得：**`AfterSideTurnEnd` 里读到的 `JusticePower.Amount` 已被它自己的 `AfterSideTurnEnd` 递减过或没有 —— 顺序未定义**。
  需要「回合结束时的原始层数」就必须提到 `BeforeSideTurnEnd*` 阶段去读。
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

### ⭐ 可复用套路：**同步**新建一个 Power（Harmony prefix 里用不了异步 `PowerCmd.Apply`）

`PowerCmd.Apply<T>` 是 `async`，所以任何「必须在 prefix 里同步决定」的地方（例：联动遗物 5 要在
`CardModel.SpendResources` 的 prefix 里当场扣费）都**不能**用它。同步等价写法：

```csharp
var created = ModelDb.Power<MyPower>().ToMutable();          // 从 canonical 拿可变副本（AssertCanonical 需要它来自 ModelDb）
created.ApplyInternal(target, amount, silent: true);         // = PowerCmd.Apply 的同步内核
```

- `PowerModel.ToMutable(int initialAmount = 0)`（`PowerModel.cs:555`）= `MutableClone()` + 记 `CanonicalInstance` + 设 `Amount`。
- `PowerModel.ApplyInternal(Creature owner, decimal amount, bool silent)`（`:564`）= `Owner = owner` → `SetAmount((int)amount, silent)`
  → `Owner.ApplyPowerInternal(this)`。**最后一步才是「注册为战斗监听器」**，少了它 Power 不会生效。
- ⚠️ `ApplyInternal` **不触发** `Hook.BeforePowerAmountChanged` / `AfterApplied`，`SetAmount` 也**不触发** `AfterPowerAmountChanged`
  ⇒ 目标 Power 里挂在 `AfterApplied` / `AfterPowerAmountChanged` 上的自动结算**都不会跑**。
  这有时候正是你要的（例：不想让【伪证】「满 5 层自动转正义」在兑换时触发），但**必须是有意为之**。
  异步路径 `PowerCmd.Apply` 会跑这些钩子（`:124`/`:136`/`:159`）。
- 删掉同理用同步的 `PowerModel.RemoveInternal()`（已在本项目多处使用）。

### ⭐ 可复用套路：让敌人「立刻打一下」但**不改变它原本的意图**

需求场景：想借用敌人**当前意图的攻击数值**打一次，但不想消耗它的回合（它自己的回合照常打出这一下）。

**不要**用 `monster.PerformMove()` / `SetMoveImmediate(...)` —— 那会推进状态机、把意图换掉。

正确写法（先例 `AnanlinTakeTheHitForTeammate.cs:42-60`，联动遗物 2 复用）：

```csharp
var move = monster.NextMove;                       // 先判 NextMove 非空，再读 IntendsToAttack
var totalDamage = move.Intents
    .OfType<AttackIntent>()
    .Sum(intent => intent.GetTotalDamage([target], monster.Creature));   // 只要攻击意图的数值

if (totalDamage <= 0) return;

await DamageCmd.Attack(totalDamage)
    .FromMonster(monster)
    .Targeting(target)                              // 单目标 ⇒ 原动作是 AOE/多目标时也只打这一个
    .Execute(choiceContext);

monster.MoveStateMachine?.OnMovePerformed(move);    // 只把 _performedFirstMove 置 true，**不碰 NextMove**
CombatManager.Instance.History.MonsterPerformedMove(combatState, monster, move, [target]);
```

- `MonsterMoveStateMachine.OnMovePerformed(MoveState _)` **参数被忽略**，函数体只有 `_performedFirstMove = true`（`MonsterMoveStateMachine.cs:49`）⇒ 安全。
- `MoveState.Intents` 里 `AttackIntent` 之外的（Buff/Defend/…）不参与取值 ⇒ 纯增益意图的敌人 `totalDamage == 0`，本套路无伤害量可用。
- `MonsterModel.IntendsToAttack => NextMove.Intents.Any(...)` **会直接解引用 `NextMove`** ⇒ 判空顺序必须是
  `c is { Monster: { NextMove: not null } monster } && monster.IntendsToAttack`，反过来会 NRE。
- 想「不改意图」就**只**做上面两步；想真的替换意图才用 `SetMoveImmediate`。

### ⭐ 可复用套路：**被「挤出球位」的球**在 `AfterOrbEvoked` 里已经不在队列里了

`OrbCmd.Evoke`（`OrbCmd.cs:125`）的顺序是 **先 `orbQueue.Remove(evokedOrb)` → 再 `evokedOrb.Evoke(ctx)` → 再 `Hook.AfterOrbEvoked(...)` → 最后 `evokedOrb.RemoveInternal()`**。
而 `CombatState.IterateHookListeners()`（`CombatState.cs:411`）枚举球的方式是
`list.AddRange(player.PlayerCombatState.OrbQueue.Orbs)` ⇒ **已被挤出/消散的球不会再收到任何 hook**。

- ⇒ 想在「球刚被挤出去」时补结算它的反应型效果（如 `EmotionDisgustOrb` 的 `AfterDamageReceived` 反伤），
  **必须在遗物/能力里手工复刻那段逻辑**，不会重复触发。
- ⇒ 「被挤出」的判定：给 `OrbCmd.Channel` 打 Prefix（`queue.Orbs.Count >= queue.Capacity` 时记下 `queue.Orbs.First()`），
  再在 `AfterOrbEvoked` 里「取走即清空」地比对（先例 `HextechOrbOverflowPatch` + `HextechOrbEvokeRules`）。

### ⭐ 能力的「额外悬浮提示」只能覆写 `AdditionalHoverTips`

RitsuLib 把 `ModPowerTemplate.ExtraHoverTips` 写成 **`sealed override`**（`ModPowerTemplate.cs:42`），
它内部 = `IncludeEnergyHoverTip` + **`AdditionalHoverTips`** + `RegisteredKeywordIds.ToHoverTips()`。
⇒ 自己的能力里**只能**覆写 `protected override IEnumerable<IHoverTip> AdditionalHoverTips`（覆写 `ExtraHoverTips` 会报 **CS0239**）。

```csharp
protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<EmotionSadness>()];
// using MegaCrit.Sts2.Core.HoverTips;  // FromCard<T>(bool upgrade = false) / FromCardWithCardHoverTips<T>()
```

### ⭐⭐ 谁会被当成 hook 监听者 —— 「卡/球本身就能持续生效」

`CombatState.IterateHookListeners()`（`CombatState.cs:411`）按顺序收集：
`creature.Powers` → 玩家的 `Relics`（未熔毁的）→ `PotionSlots` → **`OrbQueue.Orbs`** →
**`PlayerCombatState.AllPiles` 里每一张卡**（连带的 `Affliction` / `Enchantment`）→ `Modifiers` → `Badges` …

⇒ **任何牌堆里的任何一张牌都天然是监听者**（本项目的能力/组件钩子就靠这条），
**球队列里的球也是**。因此「让某个效果持续存在」有两条零成本路子：

- **球留在队列里** ⇒ 球自己的钩子（`AfterCardPlayed` / `ModifyDamageMultiplicative` …）继续跑；
- **卡留在某个牌堆里** ⇒ 挂在卡上的组件钩子继续跑（这也是 `RetainCounterComponent` 只在 `PileType.Hand` 里计数成立的原因）。

⇒ 结论：**不需要**为了「换个地方继续生效」再造一个 Power 去搬运效果 —— 把载体（球/卡）留在被枚举的集合里即可。

### ⭐ 球位容量：不挤出 = 先给容量 +1

`OrbCmd.AddSlots(Player, int)`（`OrbCmd.cs:21`）：`amount = Math.Min(10 - Capacity, amount)` ⇒ **封顶 10 格**，
`OrbQueue.AddCapacity(n)` + `NOrbManager.AddSlotAnim`；返回的 `Task` 本身是**已完成**的（函数体无 await）。
`OrbCmd.RemoveSlots` ⇒ `RemoveCapacity(n)`（`Capacity = Max(0, Capacity-n)`，**并从队尾开始裁掉超出的球**）
+ `RemoveSlotAnim`。⇒ 想「不挤出队首那颗球」，在 `OrbCmd.Channel` 的 Prefix 里先 `AddSlots(player, 1)`，
让后面那句 `if (OrbQueue.Orbs.Count >= OrbQueue.Capacity) await EvokeNext(...)` 不成立即可；
收回容量前必须确认 `Orbs.Count <= Capacity - 1`，否则会连带裁掉别的球。

球位锚点：`NCreature.SetOrbManagerPosition()` 取 `Visuals.OrbPosition.Position`，
而 `NCreatureVisuals` 里 `OrbPosition = HasNode("%OrbPos") ? GetNode<Marker2D>("%OrbPos") : IntentPosition`
⇒ 我们的角色场景没有 `%OrbPos`，球位落在意图/血条附近（**「血条下面」**）—— 而 `NOrb` 上渲染的正是那张情绪卡
（`EmotionOrbVisualPatch` 往里塞 `NCard`）。

### ⭐ 球的悬浮提示：`ModOrbTemplate.AdditionalHoverTips`

`OrbModel.HoverTips` = `ExtraHoverTips` + `SmartDescription`（有就加，否则 `DumbHoverTip`（本球 `orbs` 表的描述））；
`NOrb` 悬停时 `NHoverTipSet.CreateAndShow(_bounds, Model.HoverTips, …)`。
`ModOrbTemplate` 侧 **`ExtraHoverTips` 也是 sealed**，可覆写的是 `protected virtual AdditionalHoverTips`。
⇒ 想让「挂在球位上的那张卡」鼠标可看：`AdditionalHoverTips => [HoverTipFactory.FromCard<T>()]`（先例 `EmotionOrb<T>`）。
球的 `title/description` 取自 `orbs` 表（`MANOSABA_LIN_ORB_<STEM>.title/.description`，本项目已有 14 条）。

### ⭐⭐ 让「不在球队列里的球」继续收钩子：`ModHelper.SubscribeForCombatStateHooks`

**这是「脱离球位 / 换个地方挂着 / 效果照旧」的正解**（2026-09-29 落地，替代 `AddSlots` 折中版）。

```csharp
// 一次即可（MainFile.Initialize() 里调用；id 重复会 Log.Error 并忽略）
ModHelper.SubscribeForCombatStateHooks("ManosabaLin.HangingEmotionOrbs", Iterate);

// public delegate IEnumerable<AbstractModel> CombatHookSubscriptionDelegate(CombatState combatState);
private static IEnumerable<AbstractModel> Iterate(CombatState combatState) { /* 必须立刻物化成列表！ */ }
```

- `CombatState.IterateHookListeners()` 末尾：`foreach (var m in ModHelper.IterateAllCombatStateSubscribers(this)) yield return m;`
  （**没有** `Contains(item)` 过滤，也没有 `HasBeenRemovedFromState` 过滤 ⇒ 要自己判）。
- `RunState.IterateHookListeners(combatState)` 最后一步是 `childCombatState.IterateHookListeners()`
  ⇒ **runState 系钩子（`ModifyDamageMultiplicative` / `TryModifyEnergyCostInCombat` / `BeforeSideTurnEnd` …）也覆盖得到**。
- 另有 `SubscribeForRunStateHooks(id, RunHookSubscriptionDelegate(RunState))`，供场外（非战斗）钩子。
- ⚠️ **返回的集合必须两端一致**（只依赖同步状态），否则联机 desync。
- ⚠️ **必须立刻物化**：钩子派发过程中若有人改底层字典，惰性迭代会抛 `InvalidOperationException`。

**摘球三连**（`OrbCmd.Channel` 的 Prefix 里，见 `HextechOrbOverflowPatch`）：
1. `queue.Remove(front)` —— 模型侧摘下 ⇒ 后面 `Orbs.Count >= Capacity` 不成立 ⇒ 不会 `EvokeNext`；
2. `NCombatRoom.Instance?.GetCreatureNode(p.Creature)?.OrbManager?.EvokeOrbAnim(front)` —— 视觉离场；
   ⚠️ **不能省**：省了的话后面 `AddOrbAnim()` 找不到空位，会去淡出**队首那颗（= 下一颗）球**，画面错位。
3. 登记进订阅集合。

`NOrbManager` 内部记账（`NOrbManager.cs`）：`_orbs` 是 `List<NOrb>`，`Model == null` 就是空位；
`EvokeOrbAnim` = 淡出该 NOrb + **在队尾补一个空位** + `TweenLayout()`；
`AddOrbAnim` = 取 `OrbQueue.Orbs.Last()`（新球永远在队尾）+ 填第一个空位/`_orbs` 里的空槽。
⇒ 「先 EvokeOrbAnim 再入队」是唯一能让 `_orbs` 与模型队列保持同序的做法。

### 血条下方的锚点

`NCreature._stateDisplay = GetNode<NCreatureStateDisplay>("%HealthBar")`；
`NCreatureStateDisplay._healthBar = GetNode<NHealthBar>("%HealthBar")`（**两层都叫 `HealthBar`，但各自是自己场景内的唯一名**）；
`NHealthBar.HpBarContainer` 是 **public Control**（= 血条本体）。
⇒ 从 `NCreature` 起 `GetNodeOrNull<NCreatureStateDisplay>("%HealthBar")?.GetNodeOrNull<NHealthBar>("%HealthBar")?.HpBarContainer`
就是血条；`NCard.defaultSize = new Vector2(300f, 422f)`（卡面尺寸，缩放后按它排版）。

### 自定义 UI 节点挂在角色身上（先例 + 新先例）

- 先例 `YalisalinFireColorCounter` / `...CounterPatch`：`[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]` postfix →
  `__instance.AddChild(node)`；`_Process` 里用 `creature.GetCreatureNode()?.Hitbox` 定位；
  ⚠️ `_ExitTree` 要 `NHoverTipSet.Remove(...)`。
- 新先例 `EmotionHangDisplay`（2026-09-29）：同上，但定位到血条正下方；
  悬浮 = 透明 `ColorRect`（`MouseFilter = Stop`）当靶子 + `NHoverTipSet.CreateAndShow(target, HoverTipFactory.FromCard(card), HoverTipAlignment.Right)`。
- `NHoverTipSet.CreateAndShow(Control owner, IHoverTip tip, HoverTipAlignment alignment = None)` / `NHoverTipSet.Remove(Control owner)`。
- `HoverTipFactory.FromCard(CardModel card, bool upgrade = false)` 有**非泛型**重载（泛型版 `FromCard<T>()` 只是包了一层 `ModelDb.Card<T>()`）。
