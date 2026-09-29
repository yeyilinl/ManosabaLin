# 通用海克斯符文设计（一角色一个 · 纯原版机制）

> **配对文件**：`docs/hextech-compat-ours.md`（施工单，注册桥与 6 个联动遗物在此）｜`docs/hextech-compat-hextech.md`（给作者的 Issue 草稿）。
> **本轮状态**：**只出设计，未落地任何代码/本地化**（用户 2026-09-28 裁决「先只出设计文档」）。
> **平衡依据**：本机全量反编译 `.local/hex_src/HextechRunes/`（371 个玩家符文 + Forge 系统）+ 引擎源码 `.local/probe_engine/decomp/`。

---

## 0. 用户裁决记录（2026-09-28）

| # | 裁决 | 落点 |
| --- | --- | --- |
| 1 | **可用性 = 真·通用**：任何角色（含原版 5 角色）都能抽到 | `HextechRuneSpec.Availability = null`（不做任何 `player.Character` 收窄） |
| 2 | **效果 = 纯原版通用机制**：不引用【轮回】【情绪球】【伪证】【审判附魔】等本模组招牌关键字 | 4 个符文全部只用原版机制表达 |
| 3 | **「在原版的基础上，只允许最多加额外能力」** | 见 §1 红线 |
| 4 | ⭐ **指定 4 个能力**：希罗=**再生**、雪莉=**临时能量**、艾玛=**随机附魔**、安安=**活力** | 见 §3 |
| 5 | ⭐ **保证不要和海克斯现有的重复** | 见 §4 查重表（逐条实测过） |

---

## 1. 设计红线

> **纯增量：只允许在原版行为之上【添加】效果；至多新增 1 个自定义能力（`PowerModel`）；
> 不得修改 / 替换 / 拦截原版的卡牌、能力、意图或任何原版系统。**

推论：

1. ✅ 允许：**给玩家加一个原版能力**（【再生】/【活力】/ 下回合能量）—— 本组 4 条的主手段。
2. ✅ 允许：**给一张牌附加原版附魔**（艾玛那条）—— 属「在卡上加东西」，不是改原版卡。
   ⚠️ 注：附魔不是「能力」；但因用户点名指定（§0-4），视为授权范围内的**原版机制增量**。
3. ❌ 禁止：**改敌人意图**、**改原版卡的数值/文本**、**给原版打 patch**、**自造新机制**。

**目标实现：零 Harmony patch、零原版修改、零新增自定义能力（4 条全部用原版能力/原版附魔）。**

---

## 2. 平衡基线（本机海克斯 371 个玩家符文实测）

构成：`Silver 107 / Gold 146 / Prismatic 118`。

| 档 | 代表符文 | 实测数值 |
| --- | --- | --- |
| **Silver** | `NimbleRune` / `BigStrengthRune` / `AdamantRune` / `SlapRune` / `BlackCandleRune` | 每回合 +1 抽牌｜伤害 ×1.2｜上 debuff ⇒ 5 格挡｜上 debuff ⇒ +1 力量｜获得时删光诅咒 |
| **Gold** | `JudicatorRune` / `AncientWineRune` / `TankEngineRune` | +1 能量 **且** 伤害 ×1.25｜打出技能回 2% 最大生命｜击杀永久 +6% 最大生命 |
| **Prismatic** | `UnmovableMountainRune` / `GoliathRune` / `GlassCannonRune` | 常驻【壁垒】+【残影】｜最大生命 ×1.35｜伤害 ×1.5 |

**本组用到的原版能力（均已确认存在）**：

- `RegenPower`（【再生】）：**回合结束回复 `Amount` 点生命，然后层数 −1**（`RegenPower.cs:20-28`）。
  ⇒ `N` 层再生的总回复量 = `N+(N−1)+…+1 = N(N+1)/2`，**跨 N 回合**。
- `EnergyNextTurnPower`（下回合能量）：下回合开始时折算成能量。
- `VigorPower`（【活力】）：下一次攻击额外 +`Amount` 伤害，攻击后消耗。
- 原版附魔（`Models/Enchantments/`）：`Adroit / Nimble / Sharp / Steady / Vigorous / Momentum / Swift / PerfectFit / Sown / Imbued / Instinct / Goopy / Slither / Glam / Clone / Corrupted / SlumberingEssence` …（另有角色专属：`Inky`/`SoulsPower`/`RoyallyApproved` 等，**不纳入随机池**）。

---

## 3. 四个符文（一角色一个）

### 3.1 二阶堂希罗 —— 海克斯·轮回新生 `HEXTECH_REBIRTH` —— **再生**

| 项 | 内容 |
| --- | --- |
| **品级** | **Gold** |
| **效果** | **每场战斗开始时，获得 5 层【再生】。**（= 回合结束回 5/4/3/2/1，共 **15 点生命**，跨 5 回合） |
| **代表希罗的理由** | 希罗身上有「**死亡回归 / 重生**」系设计（`DeathRewindPower`、`HiroMagicRevivePower`）——【再生】= 倒下也能重新站起来。再生**总回复量随回合三角递减**，越往后越慢，正好是「**轮回**」的节奏：同样的力气，循环回来时已经少了一分。 |
| **实现落点** | `BeforeCombatStart` → `PowerCmd.Apply<RegenPower>(Owner.Creature, 5)`。**零新增能力**。 |
| **档位依据** | 与唯一的既有再生符文 `DawnbringersResolveRune`（**Gold**）同档：后者是「一场一次、生命 <50% 触发、最大生命 15%」（≈11–15 HP），本项是「开场固定、无触发条件、共 15 HP」 ⇒ 同量级、不同形态。可下调为 3 层（=6 HP）并降到 Silver。 |

### 3.2 橘雪莉 —— 海克斯·情绪共鸣 `HEXTECH_EMOTION_RESONANCE` —— **临时能量**

| 项 | 内容 |
| --- | --- |
| **品级** | **Silver** |
| **效果** | **每当你获得一张【状态】或【诅咒】牌时，获得 1 点下回合能量。**（每回合至多 2 次） |
| **代表雪莉的理由** | 雪莉琳把**负面情绪**一口口咽进情绪球 —— 情绪不**当场**爆发，而是**憋到下一刻**再翻涌出来。这正是 `EnergyNextTurnPower` 的节拍：把此刻的难受，变成下一回合的劲头。 |
| **实现落点** | 钩子「获得牌」—— 首选 `AfterCardEnteredCombat` / `AfterCardGeneratedForCombat`，判定 `card.Type is CardType.Status or CardType.Curse`；触发 `PowerCmd.Apply<EnergyNextTurnPower>(1)`。**零新增能力**。回合上限用一个 `[SavedProperty] int` 计数（`AfterPlayerTurnStart` 清零）。 |
| **档位依据** | Silver：条件触发 + 回合上限 + 发放的是**延迟**资源（要等到下回合才变能量）。对照 `AdamantRune`/`SlapRune`（条件触发 ⇒ 小收益、无上限）与 `NimbleRune`（每回合 +1 抽牌）。 |

### 3.3 夏目安安 —— 海克斯·安然手记 `HEXTECH_COMPOSURE` —— **活力**

| 项 | 内容 |
| --- | --- |
| **品级** | **Silver** |
| **效果** | **每回合结束时，若你本回合未受到任何伤害，获得 2 层【活力】。** |
| **代表安安的理由** | 安安的招牌是「**安心**」——而她是那个**替队友挨打**的人。所以她的节奏是：「只要这一回合轮到我身上没有人受伤，我就悄悄攒下一分力气」，攒着的力气在下一击里一次放出去（【活力】打完即消耗）。 |
| **实现落点** | `AfterDamageReceived`（owner 受伤 ⇒ 置标记）+ `AfterSideTurnEnd`（未受伤 ⇒ `PowerCmd.Apply<VigorPower>(2)`）。**零新增能力**。 |
| **档位依据** | Silver：条件触发、每回合 2 层、且【活力】是**一次性**消耗品（不是永久力量）。对照 `SlapRune`（+1 力量、无上限）。 |

### 3.4 樱羽艾玛 —— 海克斯·三审宣判 `HEXTECH_VERDICT` —— **随机附魔**

| 项 | 内容 |
| --- | --- |
| **品级** | **Gold** |
| **效果** | **每场战斗开始时，随机为 1 张手牌附上一种随机【附魔】。** |
| **代表艾玛的理由** | 艾玛的【审判】= 给「证据」（牌）**盖章归类**（同意 / 反驳 / 怀疑）。这里把「盖章」保留下来，只是随机盖哪一枚章 —— 因为要**真·通用**，不能用我们自己的三种审判附魔，于是退成原版附魔系统。 |
| **实现落点** | `BeforeCombatStart` → 手牌随机挑 1 张（`RunState.Rng.CombatCardSelection`）→ 从**附魔白名单**随机挑 1 种（`RunState.Rng.CombatCardGeneration`）→ `CardCmd.Enchant(ModelDb.Enchantment<T>().ToMutable(), card, 1m)`。**零新增能力**。 |
| **档位依据** | Gold：附魔是强收益（正面附魔直接改牌的性能），且随机上限不可控 ⇒ 取 Gold（Prismatic `ThoughtOverwriteRune` 是「获得时**自选**多张牌加关键字」，本项是「每战随机 1 张」，比它弱一档）。 |
| **⚠️ 实现注意** | ① 附魔**白名单**要在实现时按 `ModelDb` 实际可无条件作用的项最终定稿，**排除角色专属**（`Inky`=Silent、`SoulsPower`=Necrobinder、`RoyallyApproved`=Regent）与废弃项（`DeprecatedEnchantment`/`MockFreeEnchantment`）；② 随机必须走 `Owner.RunState.Rng`（联机确定性）；③ 手牌为空时不触发（开场一般不为空）。 |

---

## 4. 与海克斯现有内容的查重（逐条实测）

| 我们的效果 | 海克斯现有 | 是否重复 | 依据 |
| --- | --- | --- | --- |
| 希罗：战斗开始固定层数【再生】 | `DawnbringersResolveRune`（**Gold**） | ❌ **不重复** | 它是「**一场一次**、生命 <50% 触发、最大生命 15%」（`AfterDamageReceived` + `_triggeredThisCombat`）；我们是「开场固定层数、无触发」。`NineDragonPowerRune`（Prismatic）主体是最大生命成长，再生只是附带。 |
| 雪莉：负面牌 ⇒ **下回合**能量 | `SpinToWinRune`（**Gold**） | ❌ **不重复** | `EnergyNextTurnPower` 全海克斯**只有它用**，且它是「**转换**已有的下回合资源（下回合抽牌/能量/召唤/星）为**即时**收益」——**不发放**新资源。另有 `EnergyForge`（Forge）是 `AfterEnergyResetLate` ⇒ **即时** +1 能量。 |
| 安安：未受伤 ⇒ 【活力】 | `ChargeUpRune`（**Gold**，Regent 专属）｜`VigorForge`（Forge） | ❌ **不重复** | `ChargeUpRune` = 「**消耗星**时 +等量活力」；`VigorForge` = 「**每回合开始无条件** +2 活力」。我们是「**未受伤** ⇒ +2」，触发条件与两者都不同。 |
| 艾玛：**随机附魔** | `InkshadowRune`（Gold，Silent 专属）｜`EnchantmentForgeBase`｜`HextechInkshadowHooks` | ❌ **不重复** | 全海克斯 `CardCmd.Enchant` 只出现这 3 处，且都是「给**固定**附魔／给**特定卡**附魔」。**没有任何符文做「随机附魔」**。 |

> 另外**我们自己**的遗物 6 `HextechJusticeRegen` 也涉及【再生】，但它是「回合结束把再生补到【正义】层数」——与「战斗开场固定层数」不冲突。

---

## 5. 落地清单（**待用户批准后才执行**）

1. 新建 4 个 `ManosabaRelicTemplate` 子类（目录 `ManosabaLinCode/Characters/Common/LinRelics/`）：
   `HextechRebirth` / `HextechEmotionResonance` / `HextechComposure` / `HextechVerdict`；
   `Rarity => RelicRarity.Starter`，`[RegisterRelic(typeof(LinRelicPool))]`。
2. `Compat/Hextech/HextechRuneCatalog.cs` 追加 4 条 `HextechRuneSpec`：`Availability = null`（真·通用）。
3. 本地化：5 语言 `relics.json` 各追加 4 组 `title/description/flavor`（12 键）——用 `.local/tools/add_hextech_loc.py` 追加、`.local/tools/set_loc.py` 改值。
4. 发布：`dotnet publish ManosabaLin.csproj` → 复核 md5 == `ExportRelease/win-x64` → `ilspycmd` 确认 4 个类型进产物 → 确认 `HextechRunes` 在 `#Strings` 仍 = 0 → 新增键后**必须重导 PCK**。

---

## 6. 未决 / 待确认

1. **这 4 个要不要也进我方 `LinRelicPool`**（= 不装海克斯时也能拿到）？
2. **雪莉那条的触发钩子**：战斗中获得状态/诅咒牌走 `AfterCardEnteredCombat` 还是 `AfterCardGeneratedForCombat`，需实测。
3. **艾玛的附魔白名单**需按 `ModelDb` 实测后定稿（§3.4 注意①）。
4. **数值档位**（再生层数 / 下回合能量点数 / 活力层数）可上下调。
5. **是否为本组画专属图标**（海克斯要求 `PackedIconPath` 由我方提供，否则显示 `missing_power.png`）。
