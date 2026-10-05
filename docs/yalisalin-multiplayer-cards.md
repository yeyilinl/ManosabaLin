# 亚里沙专属多人卡 ×5 + 能力 ×3（2026-09-30 定稿）

> 需求来源：用户 2026-09-30 口述（含 4 项裁决）。实现：`ManosabaLinCode/Characters/Yalisalin/Cards/YalisalinMultiplayerCards.cs`
> + `ManosabaLinCode/Characters/Yalisalin/Powers/YalisalinMultiplayerPowers.cs`。
> 卡名由 AI 拟定（用户要求「符合效果和角色人设」）。**zhs 为文案权威**，eng/jpn/kor/rus 本轮同步译好。

## 一、5 张卡

| 类名 / 本地化键 | 卡名 | 费·型·罕见度 | 未升级 → 升级 | 备注 |
| --- | --- | --- | --- | --- |
| `CombustionShared` | 共燃 | 1·攻击·罕见·`AnyEnemy` | 8 伤 → 11 伤 | **自带余火**；余火额外连接队友弃牌堆 1 张攻击牌，该牌被烧掉时**双方各自动打出它 1 次** |
| `StokeForYou` | 代你燎原 | 2·攻击·罕见·`AnyAlly` | 2 费 → 1 费 | 本回合他造成伤害时，按**命中段数**给该敌人等量火色 |
| `TorchPassing` | 薪火相传 | 3·能力·稀有·`AnyAlly` | 3 费 → 2 费 | 本场战斗他每次用**指向敌人的技能牌**就给该敌人 1 格火色（**予燎**） |
| `SharedGuilt` | 共罪 | 1·技能·罕见·`Self` | — → 「并移除其所有减力量」 | 获得 1 层【共罪】 |
| `EmberDividend` | 余烬分赠 | 1·攻击·稀有·`Self` | 回 6 血 → 9 血 | **自带余火**；被余火烧掉时：下回合回手 + 给每名队友 1 张其卡池的牌（0 费虚无、带余火） |

五张全部 `MultiplayerConstraint => MultiplayerOnly`，`[RegisterCard(typeof(YalisalinCardPool))]`。
两张「自带余火」= 在 `Capabilities/YalisalinFireComponentCapability.cs` 追加 `[RegisterDefaultModelCapability(typeof(...))]`（**不是**卡上写接口）。

### 用户 4 项裁决（原话存档）
1. 连接的牌被烧掉 → **双方各打一次** ⇒ 实现为对同一张牌连调两次 `CardCmd.AutoPlay`（`AutoPlay` 内部一律用 `card.Owner` 解析目标/费用，所以两次都记在该队友名下，这是引擎层面能做到的最接近形态）。
2. 「额外连接」由谁定 → **随机**，走 `Owner.RunState.Rng.CombatCardSelection.NextItem`（联机可用，禁用 `Random.Shared`）。
3. 「次数」= **本次伤害的命中段数** ⇒ `AttackHitContext.TotalHitCount`，在 `HitIndex == 0` 时结算一次。
4. 共罪阈值 → **按引擎实际阈值 12**（`SuspectPower.TokenThreshold = 12`），不是 13。

## 二、3 个能力

| 类名 | 挂谁 | 触发 | 要点 |
| --- | --- | --- | --- |
| `StokeForYouPower` | **队友** | `IAttackHitHookListener.AfterAttackHit`，仅 `HitIndex == 0` | 火色量表属于**施加者**，故额外存 `Caster`（= 发夹持有者）；`AfterSideTurnEnd` 按 `side == Owner.Side` 移除 |
| `TorchPassingPower` | **队友** | `AfterCardPlayed`：`CardType.Skill` + `Target is { IsEnemy: true }` | `GiveFireColor(..., overflowTriggersConsume: true)` = 予燎 |
| `SharedGuiltPower` | 自己（`StackType.Counter`） | `AfterPowerAmountChanged`：`power is SuspectPower` 且 `Amount >= 12` | 见下 |

### `SharedGuiltPower` 的「避免坏结局」两条腿
引擎里【嫌疑】满 12 就会发【坏结局】，而**先后顺序不保证**（本能力与原版 `SuspectPower` 自己的处理器谁先跑不定），所以两条都做：

1. **同钩子内先把嫌疑压回 < 12**（`PowerCmd.ModifyAmount(-taken)`）⇒ 后跑的人看到 < 12，坏结局根本不会发。
2. **兜底**：若它已经先跑、坏结局已经到手，用 `CardPileCmd.RemoveFromCombat` 把那张诅咒**移出战斗区**拿走。
   ⚠️ 只能这样 —— 那 4 张坏结局卡（`HiroBadEnding` / `EmaForgottenOne` / `AnanlinBadEnding` / `Sherrybadending`）被弃掉、被消耗**都会自己回手**。

随后：嫌疑减半对分（`total / 2` 留给他、`total - half` 归你）+ 双方各 1 层 `RitualCeremonyPower`【魔女仪式】+ 自身 `-1` 层（归零则 `PowerCmd.Remove`）。
升级版（`ClearsStrengthDown`，由 `SharedGuilt.OnPlay` 里 `power.ClearsStrengthDown = IsUpgraded` 传入）：额外清空他所有「减力量」= `TempStrengthDown` 全清 + 负向 `StrengthPower` 抹平。

## 三、「予燎」= 溢出结算（2026-10-01 按用户裁决改为「持续累计 + 连续」）

`YalisalinsHairpin.GiveFireColor(..., overflowTriggersConsume: true)` 在量表已满时对溢出的每一格补结算：

- **溢出不改变量表格数**（`ResolveConsumedColor` 不动 `gauge.Filled`）⇒ 量表**始终满格**，只要持续给予火色就持续溢出、持续结算。
- 溢出第 `n` 格取 `SlotColor(n % 6 + 1)`，**每 6 格一循环**：

| 溢出第 n 格 | 1 | 2 | 3 | 4 | 5 | 6 | 7… |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 触发颜色 | 浅橙 | 浅橙 | 亮黄 | 亮黄 | 赤红 | 赤红 | 回到浅橙 |

- 连续两次碰到同色 ⇒ 与普通消耗一样凑成一次「**连续**」。

### 本轮改动（`YalisalinsHairpin.cs`）

原实现每次 `GiveFireColor` 都 `new YalisalinFireColorChain()`、序号 `i` 每次从 0 起
⇒ **分多次溢出时永远不连续，颜色也每次都从浅橙重来**（只有"一次性溢出多格"才连续），与要求不符。

现把「溢出色序 + 连续链」记在**目标各自的量表** `YalisalinFireColorGauge` 上：

- `OverflowConsumedThisTurn`：本回合该目标累计溢出格数；`TakeNextOverflowSlot()` 返回 `n % 6 + 1` 并自增。
- `OverflowChain`：溢出专用连续链，与普通消耗的 `_chain` **分开**、互不干扰。
- `ResetOverflowTracking()`：在 `AfterPlayerTurnStart` 里随 `MarkTurnStart()` 一起重置（战斗开始/结束由 `_gauges.Clear()` 覆盖）。

⇒ 满格超 1 格 = 1 次浅橙；**仍满格**再超 1 格 = 1 次浅橙 **+** 1 次连续浅橙；再超 = 亮黄；满 6 格一循环。**与用户口述逐字一致。**

- 「留到明天的烫伤」`Tomorrowburn`、「魔女囚犯」`YalisalinWitchPrisoner`、薪火相传 `TorchPassingPower` 都走同一个
  `overflowTriggersConsume: true` ⇒ **一处改动全部生效**，无需各改各的。
- 遗物 `..._YALISALINS_HAIRPIN.fireColor.description` 的「予燎」一句已同步补上
  「只要持续超出就持续结算（每 6 格一循环），相邻同色仍可构成连续」（5 语言）。

## 四、⚠️ 类型改动带来的副作用（用户需确认）

「还给你的发带」`Returnedhairribbon`、「口袋里的火柴盒」`Pocketmatchbox` 按用户要求由**技能 → 攻击卡**。
发夹的自动消耗门槛是 `IsYalisalinAttackCardDamage()` 里的 `cardSource.Type == CardType.Attack`（`YalisalinsHairpin.cs:944`）
⇒ **这两张卡造成的伤害现在会额外「从最新格消耗 1 格火色」**（改之前是技能牌，不消耗）：

- `Returnedhairribbon`：5 格挡 + 5 伤 + 回收 1 张牌给余火。若目标身上有火色，**打完会少 1 格**（卡面没写这件事）。
- `Pocketmatchbox`：先给 1 格再打 8 伤 ⇒ 净结果变成 **给 1 格 - 消耗 1 格 = 0**（有火色时）。相应测试 `Pocketmatchbox_damages_when_entering_new_color_band` 的期望值已由 3 改成 2 并注明原因。

> 若这不是想要的：要么改回技能类型，要么在发夹的 `IsYalisalinAttackCardDamage` 里为这两张卡开例外 —— **本轮按「只改点名机制」未擅自加例外**。

## 五、待办 / 遗留（本轮**未**处理）

- ✅ **【2026-09-30 23:59 已修，原结论有误】** 上一版此条称 `ContrastWound` / `FullCourtVerdict` / `SinMarket` / `SinOffering` 五语言缺 `selectionScreenPrompt` 键 —— **是误报**，这 4 张的键 5 语言都齐全。
  穷尽复核（扫全库 113 处「选择提示键」引用后）**真正的崩溃点只有 2 处，均已修复并重发布**：
  1. 🔴 `relics.json` 五语把键写成 `MANOSABA_LIN_RELIC_WITHEMA.`**`S`**`electionScreenPrompt`（**大写 S**），而引擎读 `.selectionScreenPrompt`（小写）⇒ `Withema`（艾玛 Starter 遗物「身份象征之物」）在「疏远 == 7 且打出疏远牌」时必崩。**已改为小写**。
  2. 🔴 `kor/cards.json` 缺 `MANOSABA_LIN_CARD_SAME_PLACE_TRUTH.selectionScreenPrompt` ⇒ 韩语下「旧识疑影」必崩。**已补 `손패에 추가할 카드 선택`**。
  另把 `zhs` 缺的 `..._UNWANTEDKINDNESS.selectionScreenPrompt` 一并补齐（该项**当前无代码引用**，仅为 5 语言对齐，可随时撤）。
  根因要点：`LocString.Exists()` 是**精确匹配** `LocTable.HasEntry`，**无语言回退、无大小写容错**。
- ⚠️ **5 张卡未实测**（全部 `MultiplayerOnly`，单人测试框架打不出；本机没联机环境）⇒ 需多人环境手动验证。
- 卡面美术：`CombustionShared` 等 5 张没有专属卡图，走默认框。
