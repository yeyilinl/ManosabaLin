# 项目长期记忆 — ManosabaLin

> **细节优先看技能**（每个都比本文件详细）：本地化 `manosaba-lin-localization`｜卡框/材质 `manosaba-lin-card-visuals`｜战斗测试 `manosaba-lin-combat-tests`｜语音音效 `manosaba-lin-audio`（FMOD 在仓库根 `FMOD/`）｜联机 desync `manosaba-lin-mp-desync`。
> **另两个不自动注入的文件（要用就 Read）**：`.workbuddy/memory/ENGINE-API.md`（引擎 API + 回合结束顺序）｜`.workbuddy/memory/PITFALLS.md`（发布/验证/本机环境/原罪时点/设计卡面长文）。
> 设计文档在 `docs/`：`originalsin-refactor-design.md`｜`yalisalin-firecolor-v2-design.md`｜`yalisalin-card-tiers.md`（卡表 v6）｜`yalisalin-card-before-after.md`（新旧对比+反思）。

## 项目坐标

- 项目根 `D:\ManosabaLin`；代码 `ManosabaLinCode/`；本地化 `ManosabaLin/localization/{zhs,eng,jpn,kor,rus}/`。
- 游戏目录 `D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2`（`local.props` 的 `Sts2Dir`；mod 产物 `mods/ManosabaLin/`）。
- 原版反编译 `.local/probe_engine/decomp/`；库真源码 `.local/external/`。⚠️ `.local/` 被 gitignore ⇒ **全仓检索会跳过，查源码必须显式传 `path=`**。
- 工具：Bash coreutils + `dotnet` 可用；**PowerShell stdout 不回传**（只有 exit code）。Python 用 venv `~\.workbuddy\binaries\python\envs\default\Scripts\python.exe`。

## 本地化

- **zhs 是源语言**，先定稿 zhs 再同步其余 4（改了不同步 = 静默不一致，没人会报错）。
- 键 `MANOSABA_LIN_{CARD,POWER,RELIC,CHARACTER}_<SNAKE>` + `.title/.description/.flavor/.smartDescription`；`<SNAKE>`=`NormalizePublicStem`（**数字不拆词**）；奖励 `MANOSABA_LIN_REWARD_<SNAKE>`；卡效果悬浮提示键自定 `<卡键>_EFFECT`（放 `cards.json`）。
- 占位符原样保留 `{Damage}`/`{Amount}`/`{Stacks}`/`{X:diff()}`/`{IfUpgraded:…}`；富文本 `[color=#RRGGBB]`/`[b]`。卡牌层数 `{Stacks}`、能力层数 `{Amount}`。⚠️ **`{Var}` 取 `BaseValue`、`{Var:diff()}` 取 `PreviewValue`，别把 `:diff()` 简化掉**。改完校验 `json.loads` + CRLF 数 == LF 数。
- **同一效果常有「能力 + 卡牌」两份逐字相同的文案** ⇒ 改机制措辞两份都改。
- **表名必须是原版已有的表**：奖励文案 `gameplay_ui`、悬浮提示 `static_hover_tips`；未支持语言走 eng 回退链；无需 `.import`。
- **能量**：动态 `{Energy:energyIcons()}`／能力层数 `{Amount:energyIcons()}`／固定 `{energyPrefix:energyIcons(N)}`（分支表见技能 §4b）。铁律：**没注入过变量的描述不能写占位符**（抛 `LocException`）。既有 **`N点⚡`**（约 15 处）是**有意约定，别统一**。
- **未同步缺口**：`…CARD_ASHINPAGES.description` 的 eng/jpn/kor/rus 仍是旧中文；⚠️ **eng/kor 的 `cards.json` 有大片条目至今仍是中文原文**（`GUARDIAN_OATH`/`COCO_ESTRANGEMENT` 等）⇒ 查术语译法别拿这两个文件当权威。
- **卡面「升级后」怎么还原**（改任何卡前要看得见新旧两版）：
  `{X:diff()}` 显示**升级后数值**（只靠颜色标绿，不写「7→10」）；`{IfUpgraded:show:A|B}` **未升级取 B、升级后取 A**（A 是升级后才有的内容）；
  `EnergyCost.UpgradeBy(-1)` 只降费、文本不变；`AddKeyword(Innate)` 只加固有、文本不变。
  ⇒ 升级后卡面 = `zhs/cards.json` 模板 + 该卡 `.cs` 的 `CanonicalVars` / `OnUpgrade` / `IsUpgraded`。
  批量脚本 `.local/dump_card_digest.py`（产物 `.local/card_upgrade_digest.txt`）。
  ⚠️ 卡类 `CanonicalVars` 用 **`[...]` 集合表达式**（不是 `{}`）⇒ 正则抓取必须做 `{}`/`[]`/`()` 三种括号平衡，否则抓到的是 `OnPlay` 方法体。

## 源码与引擎 API（要点）

- 完整速查见 `ENGINE-API.md`。三条最容易踩：
  - `PowerModel.Owner` 就是 `Creature`（写 `Owner.Creature` 报 CS1061）；
  - `PowerCmd.Apply<T>` 返回 `T?` **必判空**；
  - 限伤挂 `Hook.ModifyHpLost`，而 `SetCurrentHp`/`Kill` **绕过**限伤。

## 战后奖励（自定义 Reward）

- 追加唯一 API `CombatRoom.AddExtraReward(Player, Reward)`；`OfferRoomEndRewards()` **逐玩家**读 `ExtraRewards[player]` ⇒ 「每个玩家都要」必须每人各加一条。
- 排序按 `Reward.RewardsSetIndex` 升序（Gold=1/Potion=2/Relic=3/SpecialCard=4/Card=5/RemoveCard=7）⇒ **自定义用 `0` 排最前**。
- RitsuLib：继承 `ModCustomReward`（基类 `RewardsSetIndex=>9`，**必须覆盖**），`ModRewardRegistry.For(ModId).RegisterOwned(stem,(save,player,json)=>new X(player))`；id=`MANOSABA_LIN_REWARD_<NORMALIZE(stem)>`。**必须在 `MainFile.Initialize()` 注册**（读档靠前缀 `RewardFromSerializableExtPatch`→`TryCreate`）。先例 `Hiro/Rewards/GuardOneBossUpgradeReward.cs`。
- `OnSelect()` 在**每台机器**都跑 ⇒ **副作用必须确定性**，只用 `CardSelectCmd.*`（内含 `PlayerChoiceSynchronizer`），别自弹 UI、别用本地随机。
- ⚠️ **`NDeckUpgradeSelectScreen` 会卡死**：确认要求 `已选数 >= prefs.MaxSelect` ⇒ **MaxSelect 先按可升级张数收窄**（`Math.Min(2,count)`），`count==0` 直接 `return true`。
- **拿不到已死 BOSS**：`Hook.AfterCombatEnd`/`IterateHookListeners` 只遍历还在 `_allies`/`_enemies` 里的生物 ⇒ 「击杀 BOSS」用 **`MonsterModel.AfterDeath`**（`wasRemovalPrevented` 会先以 true 来一次）。
- ⚠️ **`AfterDeath` 里别做「奖励屏 / 玩家选择」这类重流程**：引擎防死分支是**先 `Hook.AfterDeath(true)`、后 `Hook.AfterPreventingDeath`** ⇒ 里面 `RewardsCmd.OfferCustom` 会把「防死/诈尸」整段吃掉，症状是「诈尸音效都响了却原地暴毙」。覆写 `AfterDeath` 必须先判 `wasRemovalPrevented`（先例 `GuardTwoBossMonster`）。诈尸护盾别写成 `某能力层数 × N`（层数可能还没建立 ⇒ 0 盾被 `Block <= 0` 的「破盾即杀」立刻打死），要有下限。

## 发布与验证（结论，细节见 PITFALLS.md）

- 发布 `dotnet publish ManosabaLin.csproj`（走 `.sln` 报 MSB4126）；重导 PCK 先 `unset STS2_SKIP_PCK_EXPORT`；只验编译 `-t:Compile`。
- ⚠️ 产物判据：md5 等于 `.godot/mono/temp/bin/ExportRelease/win-x64/ManosabaLin.dll`（~2.6MB）才是 Release 版；`-t:Compile`/跑测试会把它换回 Debug。
- ⚠️ **新增/替换美术资源或本地化键后必须重发布**（测试与游戏都读游戏目录 PCK）⇒ 否则 `ResourceLoader.Exists` false（卡框静默不挂）或 `Missing localization key`。
- ⚠️ **用户不要把时间花在无头测试上**：以原版反编译源码 + 本项目现有写法为准，保证 `-t:Compile` 0 错误 + `publish` 通过即可；别改 ASSERT 迁就 harness。
- ⚠️ **工作区会有并发编辑者**：publish 前 `find ManosabaLinCode ManosabaLin.Tests ManosabaLin/localization -newermt "<上次构建时间>" -type f \( -name '*.cs' -o -name '*.json' \)`；有别人 WIP 就别发布，改用 `ilspycmd` 只验自己的类型。
- 验产物有没有自己的字符串：`grep -a` 对 .NET DLL **永远 0 命中**（UTF-16）⇒ 用 Python `b.count(k.encode('utf-16-le'))` 或 `ilspycmd -t <类型>`。
- ⚠️ **改「今天新加、还没进 git HEAD」的键前先备份**；**还原文件一律 `cp -f`，绝不 `rm -rf` 目录**。

## 联机 desync

- 详见 `manosaba-lin-mp-desync`。两条最贵的教训：① 「本地 UI/输入直接改 run state」是 grep 不到的根因；② **多人下一切"随机"必须走 `Owner.RunState.Rng.*`**，禁 `Random.Shared`/`new Random()`/依赖实例 id 与隐式顺序。
- 判据：**动作序列一致 + 差异只在牌堆/数值 ⇒ 本地随机；序列不一致 ⇒ 只在单侧触发的 hook**。

## 常用设计范式（要点，长文见 PITFALLS.md）

- **卡面基准**：本项目 `.description`（含富文本）**p25=75 / 中位=107 / p75=169**。档位：基础 1 动作 ~45 / 白 2 动作 80~110 / 蓝 3 动作 110~160 / 金 3~4 动作 150~210。诅咒/自动触发类仍要短（40~90）。
- **加厚 ≠ 加分支**：该禁的是嵌套条件 / 循环 / 概率链 / 隐藏状态 / 自动触发卡上的选择 UI；加厚靠**加动作数 + 加修饰语**。
- **抓位**：玩家缺什么抓什么，每张卡必须有且只有一个抓位理由、不与任何卡重复；**蓝比金好拿 ⇒ 蓝才是构筑主力**，蓝卡必须做白卡做不到的事（功能差异而非数值差异）。
- **硬约束**：① 卡数 = 抓位数；② 禁「抽 1 张 / 获得 X 格挡」当填料（除非该卡抓位本身就是过牌/防御）；③ 每张 2~3 动作，加厚部分必须是核心功能的**延伸/放大/回报**——删掉第二效果后由"有性格"变"白板"即填料。
- ⚠️ **改任何卡之前先读它的原卡面（未升级版 + 升级版）**，回答"它原来解决什么问题"；原设计已解决独特问题就保留独特性、只简化表达。别为了统一模板磨平它（三条实例见 `docs/yalisalin-card-before-after.md`）。⭐ **实例教训**：`KuanShuYinJi`/`NiNiZhiZheng`/`XingJiaJiaShen`/`ZhiMingHuanYa` 四张我套模板磨平了原设计，被对比表打回。
- **艾玛「审判」体系**：【审判】= `Agreement`/`Rebuttal`/`Doubt` 三种附魔的统称；亲近/疏远 = `BondPower.Affinity/.Estrangement`。附魔计数用 `EmalinCombatHelper`：数「种」→ `GetDistinctEnchantmentTypesThisTurn`（≤3），数「张」→ `GetTotalEnchantmentPlaysThisTurn` / `Get{Agreement,Rebuttal,Doubt}PlaysThisTurn`；**卡面写「N种」就必须用 distinct**。免费打出用 `card.SetToFreeThisTurn()`；读费用 `card.EnergyCost.GetResolved()`。
- **组件悬浮提示**：`KeywordLikeComponent` 子类自带 `HoverTips`，键 `ManosabaLin.{ComponentId}.hovertip.title/.description`。卡面显示：`public static IHoverTip[] Tip => GetHoverTip<T>();` + 卡里 `AdditionalHoverTips => XxxComponent.Tip;`（先例 `RetainCounterComponent.Tip`）。⚠️ `RemoveOnPlayComponent.Tip` 例外，是 `static readonly` **字段**。
- **「卡面只留风味文本」范式**：卡加进 `LyXlTypePlaquePatch.RemovesTypePlaque` 白名单 + `AncientTextBgPath` 用 8×8 全透明图 + `.description` 只写风味、效果搬去 `_EFFECT` 悬浮提示。⚠️ **`CardRarity.Ancient` 不会自动去掉类型牌匾**。步骤见 `manosaba-lin-card-visuals` §7。
- **静态挂组件**：`CanonicalComponents => [new XxxComponent()]`；运行时 `card.TryAddComponent(...)`。先例：`SilverBlazeToken`/`EmotionMimic`/`RetainGrant`（静态）、`PerpetualFrenzy`/`RetainAmplify`（运行时）。
- ⚠️ **【魔女化】是两个东西**：`WithPower`（层数资源）vs `Witchification` **组件**（`ModifyCardPlayCount +1`，仅 `StabbingBlade` 挂）。改措辞前先确认指哪个。

## 火色 v2（**已定稿**，细节见 `docs/yalisalin-issue-firecomponent-firecolor.md` §五/§六 + `docs/yalisalin-firecolor-v2-design.md`）

- **定稿口径**：挂在**敌人**身上 **6 格**；**格位定色** 1-2 浅橙 / 3-4 亮黄 / 5-6 赤红；**给**由**卡牌效果**给（**攻击不再自动给**）从第 1 格往上填；**消耗**从**最新格（已填最高格）**开始；**同色**两段连续才有额外收益；三色数值**照抄当前 `zhs/relics.json` 的 `fireColor.*.description`**。
- ⭐ **格位定色 ⟺ 从最新/最高格消耗**（填 1→6 + 消耗 6→1 ⇒ 量表恒为 `{1..n}` 连续前缀、无空洞）。若改成 FIFO 从第 1 格消耗，要么留前洞让新火色插队，要么左对齐压缩使"第 3 格亮黄移到第 2 格(浅橙)"与格位定色冲突。**"要不要压缩"因此不是待裁项，是方向决定的推论。**
- ⚠️ **代码方向是反的**：`YalisalinFireColorGauge.Consume`（`YalisalinsHairpin.cs:1483`）取 `ordered[0]`（`Order = ++conversionSequence` ⇒ 最早格）。本地化「造成伤害会消耗最新火色」**是对的**，**要改代码**。
- **21 张卡面已定稿**（用户直给，`issue §六`）：封存 / 升温（强升温）机制**全删但卡全保留**（`firecolor-v2-design.md` §3.3 的"删 8 张"作废）；"连续"统一成**同色**（原 `Unusedconclusion`/`Samewrongproblem`/`BoundPrometheus` 的"不同色"全改）。
- **「予燎」= 给予火色**；给予超出 6 格 ⇒ **一次性**按消耗顺序补结算（超出 N 格结算 N 次）。⚠️ `Tomorrowburn` 例子写"超出一格触发一次**浅橙**"与"从最新格(=赤红)"矛盾，**未裁**。
- ⚠️ 基础牌 `Attack`/`Defend` 卡面**没变** + "火色只由卡牌效果给" ⇒ **起手没有任何给火色的途径**（未裁：是否给 `Defend` 加"给予 1 格"）。

## 原罪诅咒 / 时点（要点，深度见 PITFALLS.md）

- 【原罪诅咒】回合结束**自动保留**（`BeforeSideTurnEndPostfix` → `GiveSingleTurnRetain`），**保留后立刻**触发「宽恕」（`AfterSideTurnEndPostfix`）；「自惩」只在**打出**时触发。14 张卡共用一份 `Originalsin` 组件。
- **「下回合开始时 X」必须延迟执行**：触发者**当场只登记**，效果在下一个玩家回合开始时跑 ⇒ 专用**隐藏**能力（`StackType.Counter` + `IsVisibleInternal => false` + `AfterPlayerTurnStart`）。先例 `OriginalsinWitchificationLossPower` / `OriginalsinMiliaReturnPower`（`Common/AncientCurses/Powers/`）。三类别混：①下回合开始做 X → 专用能力；②下回合获得能量 → 原版 `EnergyNextTurnPower`；③下回合**当你 XXX 时** → 条件钩子 + 已用标记。
- **四条结构债**（详见 `docs/originalsin-refactor-design.md`）：① `Originalsin.cs` 944 行 switch 分发器 + 基类 13 行空壳 ⇒ 加一张卡动 6 处；② 同一张诅咒两种形态（组件默认不挂，靠 9 张亚里沙卡手动挂 ⇒ 从别处得到 = 废牌）；③ `ForgiveTriggered`/`PunishTriggered` 是**静态事件 + 6 个 `async void` 订阅者**且不过滤 owner ⇒ desync 高风险，新监听器别写 `async void`；④ 宽恕/自惩计数两套并行（通用 Power 只有本场总量；「本回合/上回合」写在**亚里沙发夹**的 6 个 `[SavedProperty]` 上 ⇒ 非亚里沙取不到）。
