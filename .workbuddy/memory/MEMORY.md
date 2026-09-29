# 项目长期记忆 — ManosabaLin

> **细节分层**：技能 `manosaba-lin-{localization,card-visuals,combat-tests,audio,mp-desync,mod-compat,build-audit}`；**要用就 Read** `.workbuddy/memory/ENGINE-API.md`（引擎 API/钩子/球位/悬浮提示）与 `PITFALLS.md`（发布验证/本地化格式表/能力美术/设计卡面/战后奖励/组件/火色）；`docs/`：`hextech-compat-ours.md`（海克斯施工单）、`hextech-compat-hextech.md`（Issue 草稿）、`originalsin-refactor-design.md`、`yalisalin-{firecolor-v2-design,card-tiers,card-before-after}.md`。
> ⚠️ **授权铁律**：点名改哪个机制就**只改那个**；`docs/*.md` 的待办不是当前范围；被打断 = 立刻停手并如实报出已改范围。品级/可用性/数值档位以用户裁决为准（机制存在 ≠ 该给谁）。
> ⚠️⭐ **别用能力（Power）搬运「持续生效」的效果**：球队列里的球、牌堆里的卡**本身就是 hook 监听者**（ENGINE-API.md）⇒ 持续生效 = 让球/卡留在被枚举的集合里。用户 09-28 因此否掉 7 个「情绪溢流」能力；**「卡牌挂在血条下面」=「充能球换个地方挂着」**。

## 坐标与工具
根 `D:\ManosabaLin`；代码 `ManosabaLinCode/`；本地化 `ManosabaLin/localization/{zhs,eng,jpn,kor,rus}/`；游戏目录 `D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2`（`local.props` 的 `Sts2Dir`，产物 `mods/ManosabaLin/`）。⚠️ `.local/`（反编译 `probe_engine/decomp/`、库源码 `external/`）被 gitignore ⇒ **查源码必须显式传 `path=`**。Bash + `dotnet`；**PowerShell stdout 不回传**；Python 用 `~\.workbuddy\binaries\python\envs\default\Scripts\python.exe`。

## 本地化（细则在技能 + PITFALLS）
- **zhs 是源语言**；键 `MANOSABA_LIN_{CARD,POWER,RELIC,CHARACTER,ORB}_<SNAKE>.{title,description,flavor,smartDescription}`、`MANOSABA_LIN_REWARD_<SNAKE>`、卡浮层 `<卡键>_EFFECT`。
- ⚠️ **BOM/缩进逐文件不同**（表见 PITFALLS）⇒ **绝不 `json.dump` 整体重写**；占位符原样保留、**没注入过变量的描述不能写占位符**（抛 `LocException`）；`N点⚡` 是有意约定。eng/kor `cards.json` 有大片中文（**别拿它们查术语**）。

## 铁律
- `PowerModel.Owner` 就是 `Creature`；`PowerCmd.Apply<T>` 返回 `T?` 必判空；限伤挂 `Hook.ModifyHpLost`（`SetCurrentHp`/`Kill` 绕过）。钩子 **Postfix 必须命名 `*Postfix`**；`CardModel.OnPlay` 是 virtual ⇒ 拦打出用 `OnPlayWrapper`；**没有 `OnOrbRemoved`/`BeforeOrbEvoked`**。多人：一切"随机"走 `Owner.RunState.Rng.*`，禁 `Random.Shared`/实例 id 顺序；本地 UI/输入直接改 run state 是 grep 不到的 desync 根因。
- 发布 `dotnet publish ManosabaLin.csproj`（别走 .sln）；⚠️ **新增美术/本地化键后必须重发布**（游戏只读游戏目录 PCK）；**别做无头测试**、publish 前查并发编辑者、还原一律 `cp -f`；判据/验产物手法/`CorFlags` 判平台无关 ⇒ 见 PITFALLS.md + 技能 `manosaba-lin-build-audit`。

## 设计范式（数值档位/判据全在 PITFALLS §设计卡面）
- **卡数 = 抓位数**、**加厚 ≠ 加分支**（禁嵌套条件/循环/概率链/隐藏状态/自动触发卡上的选择 UI）、**蓝比金好拿 ⇒ 蓝是构筑主力**、禁「抽 1 张/获得 X 格挡」当填料（数值档位与判据见 PITFALLS §设计卡面）。
- ⚠️ **改卡前先读原卡面（未升级 + 升级版）**，别为统一模板磨平（被打回实例见 `docs/yalisalin-card-before-after.md`）；卡构造 `ManosabaCardTemplate(费, CardType.X, CardRarity.Y, TargetType.Z)`；能力/球的额外悬浮提示只能覆写 `AdditionalHoverTips`（⇒ CS0239）。⚠️【魔女化】两义：`WithPower`（层数资源）vs `Witchification` **组件**（`ModifyCardPlayCount +1` + `WitchificationCount`；**刻意不加 `[ComponentState]`**，加了会破存档/联机）。

## 海克斯联动 6 遗物（`Characters/Common/LinRelics/`；池 `LinRelicPool.cs`；patch `Patches/HextechRelicPatches.cs`）
- **遗物 2**（09-29 定稿）：持续型 7 种**脱离球位、改挂血条下方**（`Channel` prefix 里 `queue.Remove` + `EvokeOrbAnim` + 登记；靠 **`ModHelper.SubscribeForCombatStateHooks`** 让那颗球继续收钩子 ⇒ **不造能力、不重写效果**，下回合开始 `Release`）、反伤型 2 种（所有正打算攻击的敌人各用自己的攻击**打你**一次 → 反伤 → 回等量血）、延迟型 5 种（立刻结算）。**遗物 3**：计数 +1 ⇒ +10 层【魔女化】，回合开始 >300 → 100 且削减层数 25:1 转计数（必须 `grantWitchification:false`）；**削减得到的计数是「拆分」**（合计 = count，逐点 `Rng.CombatCardSelection` 随机指派，不是每张各一份）；**计数来源 = 每回合被【保留】一次**。
- 其余 4 个 + 注册桥见 `docs/hextech-compat-ours.md §2.8/§2.9`；第二身份 `Compat/Hextech/`（零 patch 软依赖）；⚠️ 本机没装海克斯 ⇒ 未实测；判硬引用 = 对方程序集名在 `#Strings` 必须 0 次。
- ⭐ **离线时这 6 个遗物「局内拿不到」是刻意的**（`RelicGrabBag.Populate` 只取原版 `SharedRelicPool` + 当前角色池，且 `RemoveAll` 丢掉非 Common/Uncommon/Rare/Shop ⇒ Starter + `LinRelicPool` 双重排除）。**用户 09-29 裁定「跟随海克斯，不用你管」⇒ 别自建获取渠道，也别为了能抽到去改 `Rarity`**（详 `docs/hextech-compat-ours.md §2.8`）。本项目既有 Starter 遗物同理靠事件 / `AnanTestConsoleCommand` 的 `RelicCmd.Obtain<T>` 发放。
- ⭐ **两个稀有度别串**：**模型侧 `RelicRarity` 必须 Starter**（《符文类的要求》写死，且原版枚举里**没有 Prismatic**：`None/Starter/Common/Uncommon/Rare/Shop/Event/Ancient`）；**海克斯品级是另一维的字符串** —— 用户 09-29 裁定**六个一律 Prismatic**（常量 `HextechRuneCatalog.RuneRarity`）。

## 火色 v2 / 原罪诅咒（细则在 PITFALLS）
火色挂**敌人** 6 格、格位定色（1-2 浅橙/3-4 亮黄/5-6 赤红）、**给**由卡牌效果给（攻击不再自动给）、**消耗从最新格**、**同色**两段连续才有额外收益；⚠️ **代码方向是反的**（`YalisalinsHairpin.cs:1483` 取 `ordered[0]`）。
原罪：14 张卡共用 `Originalsin` 组件，回合结束**自动保留 → 立刻宽恕**、「自惩」只在**打出**时触发；**「下回合开始时 X」用专用隐藏能力延迟执行**。
