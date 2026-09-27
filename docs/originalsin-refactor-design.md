# 原罪诅咒卡（Originalsin）重构 · 设计思路

> 状态：**只写设计，不含代码**。结论全部来自 `ManosabaLinCode/` 实读 + 项目现有写法。
> 一句话病根：**`Originalsin.cs` 把 14 张卡的行为全吞进自己肚子里的 5 个 `switch`，
> 而真正的卡基类 `LinAncientCurseCard` 是个空壳。**

---

## 一、现状：一张图看清结构

```
Characters/Common/
├── Components/Originalsin.cs          ← 944 行：时点 + 5 个 switch + 28 个效果方法
└── AncientCurses/
    ├── LinAncientCurseCard.cs         ← 13 行：只管卡框/稀有度，行为 0
    ├── AncientSinCardCatalog.cs       ← 13 张卡的随机目录（缺 WitchificationCurse）
    ├── {13 张卡}.cs                   ← 每张只写数值，效果不在自己身上
    └── Powers/                        ← 13 个专用能力
Characters/Hiro/
├── Cards/Hiroparanoid.cs              ← 第 14 张，物理位置在希罗池
└── Powers/WithPower.cs                ← 【魔女化】，被 Common 的 WitchificationCurse 依赖
```

`Originalsin.cs` 里 5 个 `switch (Card)`：`PrefixLocString` / `HoverTips` / `Forgive` / `Punish`
（× 2 个方向）= **加一张新诅咒卡要动 6 处、写 3 个 switch 分支**。

---

## 二、六个问题（按严重性）

### P0-1 · 同一张卡有两种形态，一张是废牌

`AncientSinCardCatalog` 的注释自己写了：

> 默认不挂载 `Originalsin` 组件——只有亚里沙的卡在获得原罪诅咒时主动挂载组件；
> 其他人正常获得该诅咒卡不带组件（纯诅咒）。

实测：只有 `WitchificationCurse` 自己挂了组件，**其余 13 张全部靠亚里沙那 9 张卡
（`ContrastWound` / `Indictment` / `JiHenFanShi` / `RedemptionCorridor` / `SinGift` /
`SinMarket` / `SinOffering` / `YalisalinWitchTrial` / `ZhiMingHuanYa`）在造牌时手动挂**。

⇒ 后果：**同一张 `MargeCharm`，从亚里沙手里出来是有宽恕/自惩的机制牌，从别处出来是
一张 2 费 `CardType.Curse`、打出后什么都不发生的纯废牌。**
而且从卡面**看不出来**是哪一种（带不带组件决定有没有 hovertip）。

### P0-2 · 组件是 944 行的 switch 分发器，基类是空壳

`Originalsin.cs` 有 `Forgive*` / `Punish*` 共 **28 个私有方法**，全部硬编码。
`LinAncientCurseCard` 只有 `MaxUpgradeLevel => 0` —— 一个行为都没有。

这是典型违反开闭原则：机制行为应该长在卡上，组件只该管**时点**。

### P1-1 · 静态事件 + `async void`，6 个订阅者全在裸奔

```csharp
// Originalsin.cs
public static event Action<PlayerChoiceContext, CardModel>? PunishTriggered;
public static event Action<PlayerChoiceContext, CardModel>? ForgiveTriggered;
```

6 个订阅者（`YalisalinsHairpin` + `KuanShuYinJiPower` / `NiNiZhiZhengPower` /
`ShuangGuiRiPower` / `XingJiaJiaShenPower` / `XunHuanFaDianPower`）**全部是 `async void`**，
每个都自己包了一层 `try { } catch (Exception ex) { Log.Error(...) }`。

三个后果：
1. `Forgive()` 里 `ForgiveTriggered?.Invoke(...)` 是**同步调用**，`async void` 在第一个
   `await` 就返回 ⇒ **调用方不等监听器跑完**就继续走清手牌 / 下一张牌结算 ⇒ 竞态。
2. `async void` 的异常无法被上层捕获，只能靠每个订阅者内部 try/catch —— 那是症状不是解法。
3. 静态事件**不过滤 owner** ⇒ 联机下每个订阅者都会收到**所有玩家**的宽恕/自惩，
   每个订阅者都得自己写一遍归属判断。

⇒ 这是整个体系里 **desync 风险最高**的一处（记忆里 `manosaba-lin-mp-desync` 的根因
第一类就是"本地回调里改 run state"，这里是同类问题的另一面：改了但没人等它改完）。

### P1-2 · 宽恕/自惩的计数是两套并行的

| 套 | 载体 | 谁在用 | 维度 |
|---|---|---|---|
| 通用 | `OriginalsinForgivenessCounterPower` / `OriginalsinResolveCounterPower`（隐藏 Power） | 组件自己 | **只有本场总量** |
| 亚里沙专用 | `YalisalinsHairpin` 的 6 个 `[SavedProperty]`：`SinForgiveThisTurn/ThisCombat/LastTurn` + `SinPunish*`（靠订阅静态事件自增） | 3 张亚里沙卡 `CrimeAndPunishment` / `JiHenFanShi` / `WarmthOfForgiveness` | 本回合 / 本场 / 上回合 |

⇒ "本回合宽恕了几次" 这种**通用**需求，实现写在**亚里沙的遗物**上。
别的角色玩原罪卡取不到这个数；发夹也因此多背 6 个序列化字段。

### P2-1 · 宽恕之后，卡去哪儿没有统一规则

| 卡 | 宽恕后 |
|---|---|
| 绝大多数 | 留在手牌 |
| `MiliaLost` | **移出战斗**（`RemoveFromCombat`），下回合由 `OriginalsinMiliaReturnPower` 送回 |
| `AnanlinVanity` | 进抽牌堆 |
| `Cocoworry` | 进抽牌堆，并换一张上手 |
| `NoahEnsnare` | 50% 进弃牌堆 / 50% 消耗 |

⇒ 玩家没法从"宽恕"这个词预判卡还在不在手里。

### P2-2 · 「宽恕」的语义正反不一，而且存储和兑现是断的

- 多数卡：宽恕 = **纯增益**（下回合 +1 能量、`Chance` +5、各种数值成长）。
- `WitchificationCurse`：宽恕 = **代价**（登记"下回合开始时失去 10 魔女化"）。
- `NoahEnsnare`：宽恕 = **负收益**（卡可能直接没了）。

更严重的是 `WitchificationCurse` 的**断裂**：

- `ForgiveWitchificationCurse` 存的是 → 「下回合失去 10 魔女化」
- `PunishWitchificationCurse` 算的是 → 「**当前**魔女化 / 50，每段给随机友方 −20 魔女化 + 抽 1」

**宽恕存进去的东西，自惩完全不看。** 两条路径各自独立对着 `WithPower` 读写，
玩家以为在"积累"，实际积累的和兑现的不是一个数。

### P3 · 目录 / 位置 / 依赖三处不一致

1. `AncientSinCardCatalog` 只有 **13** 张，实际 **14** 张（缺 `WitchificationCurse`）。
2. `Hiroparanoid` 物理位置在 `Hiro/Cards/`，却被当作通用诅咒放进目录
   ⇒ `Originalsin.cs` 和 `AncientSinCardCatalog.cs` 两个 Common 文件都得 `using ...Hiro.Cards`。
3. `WitchificationCurse`（Common）依赖 `WithPower`（在 `Hiro/Powers/`）—— 又一个反向依赖。
4. 39 个本地化键（14 × prefix / hovertip.description + 卡面）靠两个 28 分支 switch 关联，
   但键名本身就是 `ManosabaLin.Originalsin.<卡名>.prefix` 的**约定** ⇒ switch 完全可以推导掉。

---

## 三、重构主张：**行为回到卡上，组件只管时点**

### 分层目标

| 层 | 现在 | 目标 |
|---|---|---|
| `LinAncientCurseCard`（基类） | 13 行空壳 | 持有 `virtual OnForgive` / `virtual OnPunish` + 自带组件 |
| `Originalsin`（组件） | 944 行（时点 + switch + 28 方法） | **~120 行**：只有时点调度 + 计数 + 广播 |
| 14 张卡 | 只有数值 | 各自实现自己的宽恕/自惩 |

### 关键接口形态（示意，不是最终签名）

```csharp
// LinAncientCurseCard.cs
public virtual Task OnForgive(PlayerChoiceContext ctx) => Task.CompletedTask;
public virtual Task OnPunish(PlayerChoiceContext ctx)  => Task.CompletedTask;

// Originalsin.cs —— 只剩时点
public override async Task AfterSideTurnEndPostfix(...)
{
    if (!_retainedForTurnEnd) return;          // 保留判定（已实现，不动）
    _retainedForTurnEnd = false;
    if (Card?.Pile?.Type != PileType.Hand) return;
    await ForgiveCore(choiceContext);          // 计数 → ((LinAncientCurseCard)Card).OnForgive → 广播
}

public override async Task OnPlayPostfix(...)
    => await PunishCore(choiceContext);        // 计数 → OnPunish → 广播
```

**新增一张原罪卡 = 写一个卡类 + 实现两个方法 + 3 个本地化键。组件零改动。**

---

## 四、六项改动（按依赖顺序）

### ① 【P3·低风险·纯删代码】键名推导化

`PrefixLocString` / `HoverTips` 两个 28 分支 switch 全删，改成按卡类型名拼键：

```
ManosabaLin.Originalsin.{TipStem}.prefix
ManosabaLin.Originalsin.{TipStem}.hovertip.description
```

**本地化文件一行都不用改** —— 现有键名已经是这个约定。纯删 ~60 行 switch。
放在第一阶段做，因为它零风险且立刻让组件短一截。

### ② 【P0-2】行为多态化

把 `Forgive*` / `Punish*` 28 个方法搬进各自卡类，`Forgive` / `Punish` 两个 switch 删除。

收益：`Originalsin.cs` **944 → ~120 行**；`using ManosabaLin.Characters.Hiro.Cards` 可删；
加卡从"动 6 处"变成"动 1 处"。

### ③ 【P1-1】静态事件改成可 await 的广播

- 签名 `Action<PlayerChoiceContext, CardModel>` → `Func<PlayerChoiceContext, CardModel, Task>`；
- 组件里 `await` 所有监听器，而不是 `Invoke` 完就走；
- **由组件统一过滤归属**（只通知同一玩家/同一侧的监听者），删掉订阅者里各自重复的判断；
- 6 个 `async void` 改回 `async Task`，6 处内部 try/catch 上移成一处。

（更彻底的做法是注册成引擎 Hook 让订阅走 `IterateHookListeners`，但那要求 hook 能被
`CombatState` 遍历到；项目里 `Originalsin` 走的是 MinionLib 的组件钩子转发，
要先确认能不能挂自定义 Hook —— 不确定就先做"可 await 的监听器集合"这一档。）

### ④ 【P1-2】计数统一到 Power

- 给现有两个计数 Power 补上 `ThisTurn` / `LastTurn` 维度（在 `AfterPlayerTurnStart` 里滚动），
  或拆成"总量 + 本回合"两个能力；
- 发夹的 6 个 `SinForgive*` / `SinPunish*` 字段删除；
- `CrimeAndPunishment` / `JiHenFanShi` / `WarmthOfForgiveness` 改读 Power。

收益：非亚里沙角色也有计数可用；发夹少 6 个 `[SavedProperty]`（它已经背了 38 个）。

### ⑤ 【P0-1】组件改为基类自带，消灭"废牌形态"

`LinAncientCurseCard` 里静态挂组件（项目已有先例：`SilverBlazeToken` / `EmotionMimic` /
`RetainGrant` 都是 `CanonicalComponents => [new XxxComponent()]`）：

```csharp
public override IEnumerable<CardComponent> CanonicalComponents => [new Originalsin()];
```

然后删掉 9 张亚里沙卡里的手动挂载。

⇒ 所有角色拿到的原罪诅咒都一致地有宽恕/自惩。
⚠️ **这条会改平衡**（别的角色拿到诅咒不再是无效果废牌，反而是负债变资源），所以放最后做，
并且要单独评估：如果担心过强，可以让 `OnForgive`/`OnPunish` 对非亚里沙只走**弱化版**，
但**不要**退回"带不带组件看谁给的"——那个从卡面看不出来，是最糟的方案。

### ⑥ 【P2】规则收口 + 目录/依赖归位

**定死语义**：**宽恕 = 卡留在手上 + 数值成长；自惩 = 一次性兑现 + 卡离场。**
- 4 张宽恕后离场的卡（`MiliaLost` / `AnanlinVanity` / `Cocoworry` / `NoahEnsnare`）：
  要么改成留手，要么把"宽恕时改为…"明确写进卡面（例外必须可见，不能藏在代码里）。
- `WitchificationCurse`：把"宽恕存的"和"自惩兑现的"接上 ——
  宽恕存 N 魔女化进这张卡，自惩按**存进去的量**兑现，而不是按"当前魔女化 / 50"另算一套。
  代价感可以保留（存储期间不能自由使用），但不再是"存了白存"。

**归位**：
- `Hiroparanoid` 迁到 `Common/AncientCurses/`（消除两个 Common 文件对 `Hiro.Cards` 的 using）；
- `AncientSinCardCatalog` 补 `WitchificationCurse`，或明确标注它是亚里沙专属并拆出目录；
- `WithPower` 从 `Hiro/Powers/` 迁到 `Common/Powers/`（**单独评估**：涉及希罗本体，
  建议只在确认 Common 有多处依赖后再动）。

---

## 五、落地顺序

| 阶段 | 内容 | 风险 | 验收 |
|---|---|---|---|
| **1** | ① 键名推导化 | 零（本地化不动） | 组件 −60 行，hovertip/prefix 表现不变 |
| **2** | ② 行为多态化 | 中（动 14 个卡类） | `Originalsin.cs` → ~120 行；5 个 switch 全无 |
| **3** | ③ 事件 await 化 | 中 | 无 `async void`；监听器异常能被上层捕获；联机无竞态 |
| **4** | ④ 计数统一到 Power | 低 | 发夹 −6 字段；3 张亚里沙卡改读 Power 后表现不变 |
| **5** | ⑤ 组件基类自带 | **高（改平衡）** | 任何角色拿到诅咒都有宽恕/自惩 |
| **6** | ⑥ 规则收口 + 归位 | 中（改数值） | 宽恕后卡去向可预期；`WitchificationCurse` 存兑一致 |

阶段 1–4 是**纯结构重构，行为不改变**，可以连续做完只靠 `-t:Compile` 0 错误验收。
阶段 5–6 才改玩法，需要单独跑一遍数值验证。

---

## 六、需要你知道的两个取舍

1. **⑤ 会让诅咒卡对所有角色都"活"起来。** 现在别的角色拿到 = 废牌（这本身是个 bug 级体验），
   修完之后变成负债但有收益。如果你就是想让诅咒"只有亚里沙能玩"，那正确做法不是
   "看谁给的才挂组件"，而是**把它放进亚里沙专属卡池、从 `AncientSinCardCatalog` 里摘出来**，
   让别人根本抽不到 —— 可见的排除，好过不可见的随机。
2. **`WithPower` 在 Hiro 目录下但被 Common 依赖。** 这次没动它。如果后续 Common 里
   还有第二处依赖，就该迁到 `Common/Powers/`，否则先记着不动。

---

## 附：本次结论的代码依据

- `ManosabaLinCode/Characters/Common/Components/Originalsin.cs` — 944 行，5 个 `switch (Card)`，
  28 个 `Forgive*`/`Punish*` 私有方法，2 个静态事件。
- `ManosabaLinCode/Characters/Common/AncientCurses/LinAncientCurseCard.cs` — 13 行，无行为。
- `ManosabaLinCode/Characters/Common/AncientCurses/AncientSinCardCatalog.cs` — 13 张目录，
  注释明说"默认不挂组件，由亚里沙调用方按需挂载"。
- 实际挂 `Originalsin()` 的位置：`WitchificationCurse` 自身 + 9 张亚里沙卡。
- `Characters/Yalisalin/Relics/YalisalinsHairpin.cs:492-501` — 订阅静态事件；
  `:65-70` — 6 个 `Sin*ThisTurn/ThisCombat/LastTurn` 字段；全文 38 个 `[SavedProperty]`、74 个 public 成员。
- 依赖倒置：`Originalsin.cs:5` 与 `AncientSinCardCatalog.cs:1` 均 `using ManosabaLin.Characters.Hiro.Cards`；
  `WitchificationCurse`（Common）用 `WithPower`（`Characters/Hiro/Powers/WithPower.cs`）。
- 亚里沙池挂原罪相关：Cards 12 张 + Powers 5 个；`Hiro/Cards/JusticeEnforcer.cs` 1 张。
