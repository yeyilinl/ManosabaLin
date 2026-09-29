# 构建 / 验证 / 时点 深度细节（不自动注入，需要时 Read）

> `MEMORY.md` 的体积上限装不下这些，故拆到此文件。主文件里保留结论与指针。

## 发布与验证

- 发布 `dotnet publish ManosabaLin.csproj`（走 `.sln` 报 MSB4126）；重导 PCK 先 `unset STS2_SKIP_PCK_EXPORT`；只验编译用 `-t:Compile`。
- **MSB3027/3021 = 游戏没关、DLL 被锁**；导出偶发 `[DONE] savepack` 后退出码 `-1`，紧跟重发即 0；MSB3231 有破坏性副作用（RitsuLib `compat`/`shared` 被删）⇒ 重跑 `dotnet build` 恢复。
- ⚠️ **publish 后游戏目录 DLL 可能是 Debug 版**（`CopyMod` 复制 `$(TargetPath)`，未带 `-c Release`）。**判据 = md5 等于 `.godot/mono/temp/bin/ExportRelease/win-x64/ManosabaLin.dll`（约 2.6MB）**，不等就 `cp -f` 覆盖复核。⚠️ `-t:Compile` 与跑测试都会把它换回 Debug 版。
- **新增/替换美术资源或本地化键后必须重发布**：测试与游戏都读游戏目录 `ManosabaLin.pck`（工作区 JSON 只在导出时进 PCK）⇒ 不重发则 `ResourceLoader.Exists` 为 false、卡框静默不挂，或报 `Missing localization key '<键>' in table '<表>'`。省一次 publish：publish → 跑全套测试 → `cp -f` ExportRelease 的 DLL 回游戏目录（PCK 没被动过）。
- **PCK 键计数** `grep -a -o "<KEY>" ManosabaLin.pck | wc -l` = **5（每语言 1 份）＋根目录 `map_*.json` 重复次数**；只比**改动前后是否 +N**。
  - ⚠️ 探测串含 `[color=…]`/`[b]` 时**必须 `grep -a -F`** —— BRE 里 `[b]` 是字符集，`apply 2 [b]Vulnerable` 会永远 0 命中，看着像「文案没进 PCK」。
  - ⚠️ 根目录 46 个 `map_*.json`（约 717KB 草稿）会被打进 PCK（**未清理，待确认**）。
- ⚠️ **用户不要把时间花在无头测试上**（2026-09-26 原话「别测试了你这个没用的」）：实现以**原版 STS2 反编译源码**（`.local/probe_engine/decomp/`）+ **本项目现有写法**为准，写完保证 `-t:Compile` 0 错误、`dotnet publish` 通过即可。测试文件可以留着，但**不要**为了让它跑绿反复迭代（尤其别再改 ASSERT 去迁就 harness）。真要验证就 `-t:Compile` + 极少一次回归。
- 无头测试 `dotnet build ManosabaLin.Tests/ManosabaLin.Tests.csproj -t:RunSts2Tests [-p:Sts2TestArgs=--sts2-test-filter=<类名>]`，日志 `D:\tmp\sts2-combat-tests\ManosabaLin_tests\logs\run.log`（每次覆盖）；**判据只看 `START total=N` 与 `SUMMARY … passed=N` 两个 N 是否相等**，不看 MSBuild 退出码。⚠️ **harness 三铁律**（详见技能）：① 每用例 ≥1 个战斗 `GameAction`，否则 `TERMINATE` 打断**整轮** runner（真错因在 `Original error before cleanup:` 后）；② `Play(card,target)` 只对 `AnyEnemy`/`AnyAlly` 合法，其余必须 `Play(card)`；③ 比对文案先剥 `[color=…]`/`[b]`。
- **确认改动真进了产物 DLL**：`grep -a` 在 .NET DLL 上**永远 0 命中**（字符串是 UTF-16）⇒ 用 Python `b.count(k.encode('utf-16-le'))`，或 `ilspycmd -t <类型> -o <目录> <dll>` 看方法体。**确定性构建下 md5 不变 = 内容真没变**（先核源码 mtime 与 dll mtime 先后）。
  ⚠️ **但「DLL 大小没变」不能推出「没重新编译」**：极小 IL 改动（改一个委托/常量）前后大小可能**完全一致**（2026-09-28 实测都是 2,678,272 B）⇒ **验「改动进没进产物」唯一可靠办法是 `ilspycmd` 反编译目标类型**，别只看大小或 md5。
- ⚠️ **改「今天新加、还没进 git HEAD」的键之前先备份**：`set_loc.py` 不留备份、`git show HEAD:…` 也取不到 ⇒ 旧文案会消失（靠 `.local/backup/` 快照救回）。**还原文件一律 `cp -f`，绝不 `rm -rf` 目录**；含糊需求先看 `git diff`。
- ⚠️ **这个工作区会有并发编辑者**（另一个会话/人也在改同一棵树；2026-09-26 14:48–16:36 实测被人插入了 `IgnitePower`/`YalisalinWitchFactor`/`FireComponentUnplayable*`/`FriendshipSeeking`/`Originalsin` 等一批改动）。⇒ **publish 前先 `find ManosabaLinCode ManosabaLin.Tests ManosabaLin/localization -newermt "<你上次构建的时间>" -type f \( -name '*.cs' -o -name '*.json' \)`**；有别人的 WIP 就别发布（`dotnet publish` 会把整棵树的半成品打进 mod），改用 `ilspycmd` 只验自己那几个类型。跑测试时 `total` 突然变大也是信号（新用例是别人加的）。⇒ 也因此 **「md5 等于我那次构建的」不再是充分判据**（别人会在你之后发布），要改用 `ilspycmd` / `grep -a -F` 直接验「产物里有没有我的逻辑/文案」；而 `-t:Compile` 与跑测试都会把游戏目录 DLL 换回 Debug 版 ⇒ 别人部署过后**先备份、跑完再 `cp -f` 还原**。

## 本机环境

- **看日志先确认是哪份**：实时 = `%APPDATA%\SlayTheSpire2\logs\godot.log`（桌面那份常是快照），启动时轮转旧日志为 `godot<时间戳>.log`；**`at HH:MM:SS` 是 UTC（本地 = UTC+8）**。判「真卡死」三连：日志在涨 + `Responding` 为 True + CPU>0；都正常即**假性卡死**（`MuteInBackground` 失焦降频，查 `FocusIn` 的 `msSinceFocusOut=`）。⚠️ 下结论前核**游戏目录 DLL 是哪次构建**：**别人可能在你之后又发布过**；**PE 时间戳对 .NET 确定性构建无效**。
- **Bash coreutils / `dotnet`（10.0.201）可用**；**PowerShell stdout 不回传**（只有 exit code）⇒ 输出写文件再 Read，或改用 Bash。
- **Python 用 venv** `~\.workbuddy\binaries\python\envs\default\Scripts\python.exe`（Pillow 装这里）；直接调 `versions\3.13.12\python.exe` 会 `No module named 'PIL'`。
- **游戏目录** `D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2`（`local.props` 的 `Sts2Dir`；mod 产物在 `mods/ManosabaLin/`）。

## 原罪诅咒 / 时点（深度）

- 【原罪诅咒】回合结束**自动保留**（`BeforeSideTurnEndPostfix` 里 `GiveSingleTurnRetain`），**保留后立刻**触发「宽恕」（`AfterSideTurnEndPostfix`）；「自惩」只在**打出**时触发。旧的「下回合开始才宽恕」已删除（会和新的重复）。14 张卡共用一份 `Originalsin` 组件。
- 引擎回合结束顺序、`HasTurnEndInHandEffect` 与「保留」互斥等坑见 `ENGINE-API.md` 的「回合结束顺序」一节。
- **原罪诅咒的「下回合开始时 X」必须延迟执行**（用户 2026-09-26 明确）：触发者（宽恕/自惩）**当场只登记**，真正效果在**下一个玩家回合开始时**执行 ⇒ 写一个专用**隐藏**能力（`StackType.Counter` + `IsVisibleInternal => false` + `AfterPlayerTurnStart`），多次触发会让层数叠加。先例 `OriginalsinWitchificationLossPower`（下回合开始失去 N 魔女化）、`OriginalsinMiliaReturnPower`（下回合重新加入抽牌堆），两者都在 `Characters/Common/AncientCurses/Powers/`。
  区分三类写法，别混：①「下回合开始时 X」→ 上述专用能力；②「下回合获得能量」→ 直接用原版 `EnergyNextTurnPower`；③「下回合**当你 XXX 时**」（Emaregret/ArisaGuilt/SherryVoid 那种条件型）→ 条件钩子 + 「限本回合/已用」标记，不是回合开始时。
- **⚠️ 结构债（改动前必读）**：`Characters/Common/Components/Originalsin.cs` 是 **944 行的 switch 分发器**——`switch (Card)` × 5 个（Prefix/HoverTips/Forgive/Punish×2）+ 28 个 `Forgive*`/`Punish*` 私有方法；而基类 `Characters/Common/AncientCurses/LinAncientCurseCard.cs` 是 **13 行空壳**（只有 `MaxUpgradeLevel=>0`）。⇒ **加一张新诅咒卡要动 6 处**。正确方向是行为多态化（虚方法放回卡类），组件只留时点。完整诊断与六步方案见 `docs/originalsin-refactor-design.md`。
- **⚠️ 同一张诅咒卡有两种形态**：组件默认不挂，只有 `WitchificationCurse` 自己挂，其余 13 张靠 9 张亚里沙卡在造牌时手动挂（`AncientSinCardCatalog` 注释自己写了这件事）⇒ 从别处得到 = 2 费 Curse、打出什么都不发生的废牌，且卡面看不出区别。别再往"看谁给的才挂组件"这条路上加代码。
- **⚠️ `Originalsin.ForgiveTriggered`/`PunishTriggered` 是静态事件 + 6 个 `async void` 订阅者**（发夹 + `KuanShuYinJi`/`NiNiZhiZheng`/`ShuangGuiRi`/`XingJiaJiaShen`/`XunHuanFaDian` 五个 Power）；`Invoke` 同步调用、`async void` 第一个 await 就返回 ⇒ 调用方不等监听器跑完，且**不过滤 owner** ⇒ 联机 desync 高风险。新监听器别再写成 `async void`。
- 宽恕/自惩计数**两套并行**：通用只有本场总量（`OriginalsinForgivenessCounterPower` / `OriginalsinResolveCounterPower`），「本回合/上回合」那套写在**亚里沙发夹**的 6 个 `[SavedProperty]` `SinForgive*`/`SinPunish*` 上 ⇒ 非亚里沙取不到。需要计数时先想清楚读哪一套。

## 设计卡面 · 完整判据（长文）

- **卡面长度/动作数基准**：本项目 573 条 `.description`（含富文本）**p25=75 / 中位=107 / p75=169**。参照样本：基础「造成 N 点伤害」=43（1 动作）；艾玛「获得 N 层【魔女化】」=80；**艾玛「造成 N 伤害 + 获得 N 层【嫌疑】+ 给予目标 N 层【魔法】」=136（3 动作）← 这才是"正常一张卡"**。⇒ 档位：**基础 1 动作 ~45 / 白 2 动作 80~110 / 蓝 3 动作 110~160 / 金 3~4 动作或大终端+附加 150~210**。
  ⚠️ **别把"字数少"当成"简单"**：该禁的是**嵌套条件 / 循环（每 X 就 Y）/ 概率链 / 隐藏状态（本回合上次…、是否曾满格）/ 自动触发卡上的选择 UI** —— 这些才是复杂度来源。加厚卡面的正确方式是**加动作数 + 加修饰语**（本回合 / 若… / 数值联动），不是加分支。
  ⇒ **普通卡按上面档位加厚，但诅咒/自动触发类卡仍要短**（宽恕 40~70、自惩 50~90，原版 `Decay` 只有 1 行）。
- **抓位**：玩家是**缺什么抓什么**，所以每张卡先回答"我什么时候需要你"。抓位维度：填充 / 引爆 / 够深层 / 强制连续 / 分色收益 / 过牌能量 / 防御续航 / 解场 / 定向获取 / 提前触发 / 资源转化 / 终结 / 引擎。
  **白卡只给基础动作（够用但不好用）；蓝卡必须做白卡做不到的事（功能差异，不是数值差异）**；金卡 = 终结位 + 改规则。⚠️ **蓝比金好拿 ⇒ 蓝才是构筑的真正主力**，功能多样性要压在蓝卡上。判据：**一张蓝卡若写成"某张白卡但数字更大"，就是废设计**。
  ⚠️ **贴抓位标签 ≠ 有抓位差异**（我犯过）：给卡表加"抓位"列、但卡面仍是「X + 抽牌/格挡」的排列组合，等于没改。
  **硬约束**：① 每张卡有且只有一个抓位理由、**不得与任何其他卡重复**（卡数 = 抓位数）；② **禁止用「抽 1 张 / 获得 X 格挡」当填料凑字数**——它们是资源不是功能，只有该卡抓位**本身就是**"过牌"/"防御"时才允许；③ 判据：**把两张卡的抓位理由互换后若没区别 ⇒ 其中有一张是废设计**。
  ⚠️ **别为了"抓位唯一"把卡削成 1 个动作**（我也犯过）：正确做法是**抓位唯一 + 每张 2~3 动作**，加厚的部分必须是**该卡核心功能的延伸 / 放大 / 回报**，不是通用资源。判据：**删掉第二效果后，若卡从"有性格"变"白板"⇒ 是填料；若变成"有前提没回报"⇒ 是延伸**。
  ⇒ 我在这事上摇摆过三次：字数太少 → 同质化填料 → 削太简单。三者都要同时满足才算对。
  ⚠️ **改任何卡之前，先读它的原卡面**，回答"它原来解决什么问题"。原设计若已解决独特问题就保留其独特性、只简化表达，别为了统一模板磨平它（详见 `docs/yalisalin-card-before-after.md` 的三条反思）。

## 可读性坑（原罪/火色共用）

- `cards.json` 是 **UTF-8 BOM**，`json.load` 必须 `encoding='utf-8-sig'`。
- 卡构造是 `ManosabaCardTemplate(费, CardType.X, CardRarity.Y, TargetType.Z)`，**不是** `base(...)` ⇒ grep 稀有度必须匹配前者，否则漏掉几乎全部卡。
- 亚里沙卡池类型失衡实测：攻击 24（26%）/ 技能 45（47%）/ 能力 27（28%）vs 原版 36%/43%/20% ⇒ "攻击引爆火色"的角色攻击牌偏少、能力牌偏多（目标能力 ~18）。
- 火色单格价值：赤红引爆 5 伤、浅橙 4 格挡、亮黄下张 −1 费；连续第 2 段再加一次 ⇒「引爆 1 格」约值**半个 1 费动作**。

## 组件 / 范式（深度）

- **艾玛「审判」体系**：【审判】= `Agreement` / `Rebuttal` / `Doubt` 三种附魔的统称；亲近 / 疏远 = `BondPower.Affinity` / `.Estrangement`。计数统一走 `EmalinCombatHelper`：
  数「**种**」→ `GetDistinctEnchantmentTypesThisTurn`（**上限 3**）；数「**张**」→ `GetTotalEnchantmentPlaysThisTurn` / `Get{Agreement,Rebuttal,Doubt}PlaysThisTurn`。
  ⚠️ **卡面写「N 种」就必须用 distinct 那个**（写「N 张」用 total），两者不可混。
  免费打出：`card.SetToFreeThisTurn()`；读当前费用：`card.EnergyCost.GetResolved()`。
- **组件悬浮提示**：`KeywordLikeComponent` 子类的组件自带 `HoverTips`，键 `ManosabaLin.{ComponentId}.hovertip.title` / `.description`。卡片侧暴露 `public static IHoverTip[] Tip => GetHoverTip<T>();` + 卡里 `AdditionalHoverTips => XxxComponent.Tip;`（先例 `RetainCounterComponent.Tip`）。
  ⚠️ **`RemoveOnPlayComponent.Tip` 是例外 —— `static readonly` 字段**，不是属性。
  挂组件：静态 `CanonicalComponents => [new XxxComponent()]`；运行时 `card.TryAddComponent(...)`。先例：`SilverBlazeToken` / `EmotionMimic` / `RetainGrant`（静态）、`PerpetualFrenzy` / `RetainAmplify`（运行时）。
- **「卡面只留风味文本」范式**（少数特殊牌用）：① 把卡加进 `LyXlTypePlaquePatch.RemovesTypePlaque` 白名单；② `AncientTextBgPath` 指向一张 8×8 全透明图；③ `.description` 只写风味文本，效果全部搬到 `<卡键>_EFFECT` 悬浮提示里。⚠️ **`CardRarity.Ancient` 不会自动去掉类型牌匾**，必须走白名单。完整步骤见技能 `manosaba-lin-card-visuals` §7。

## 火色 v2 定稿细则

> 主入口见 `docs/yalisalin-issue-firecomponent-firecolor.md` §五/§六 + `docs/yalisalin-firecolor-v2-design.md`。

- **定稿口径**：挂**敌人**身上 **6 格**；**格位定色** 1-2 浅橙 / 3-4 亮黄 / 5-6 赤红；**给**由**卡牌效果**给（**攻击不再自动给**）从第 1 格往上填；**消耗**从**最新格（已填最高格）**开始；**同色**两段连续才有额外收益；三色数值照抄 `zhs/relics.json` 的 `fireColor.*.description`。
- ⭐ **格位定色 ⟺ 从最新/最高格消耗**（填 1→6 + 消耗 6→1 ⇒ 量表恒为 `{1..n}` 连续前缀、无空洞）。若改 FIFO 从第 1 格消耗，要么留前洞让新火色插队，要么左对齐压缩使"第 3 格亮黄移到第 2 格(浅橙)"与格位定色冲突 ⇒ **"要不要压缩"不是待裁项，是方向决定的推论**。
- ⚠️ **代码方向是反的**：`YalisalinFireColorGauge.Consume`（`YalisalinsHairpin.cs:1483`）取 `ordered[0]`（`Order = ++conversionSequence` ⇒ 最早格）。本地化「造成伤害会消耗最新火色」**是对的**，**要改代码**。
- **21 张卡面已定稿**（用户直给，issue §六）：封存 / 升温（强升温）机制**全删但卡全保留**（`firecolor-v2-design.md` §3.3 的"删 8 张"作废）；"连续"统一成**同色**（原 `Unusedconclusion`/`Samewrongproblem`/`BoundPrometheus` 的"不同色"全改）。
- **「予燎」= 给予火色**；给予超出 6 格 ⇒ **一次性**按消耗顺序补结算（超出 N 格结算 N 次）。⚠️ `Tomorrowburn` 例子写"超出一格触发一次**浅橙**"与"从最新格(=赤红)"矛盾，**未裁**。
- ⚠️ 基础牌 `Attack`/`Defend` 卡面**没变** + "火色只由卡牌效果给" ⇒ **起手没有任何给火色的途径**（未裁：是否给 `Defend` 加"给予 1 格"）。

## 战后奖励 / 自定义 Reward（深度）

- **唯一追加 API**：`CombatRoom.AddExtraReward(Player, Reward)`；`OfferRoomEndRewards()` **逐玩家**读 `ExtraRewards[player]` ⇒ 「每个玩家都要」必须**每人各加一条**。
- **排序**：按 `Reward.RewardsSetIndex` 升序（Gold=1 / Potion=2 / Relic=3 / SpecialCard=4 / Card=5 / RemoveCard=7）⇒ 自定义用 `0` 排最前。
- **RitsuLib 注册**：继承 `ModCustomReward`（基类 `RewardsSetIndex => 9`，**必须覆盖**），`ModRewardRegistry.For(ModId).RegisterOwned(stem, (save, player, json) => new X(player))`；id = `MANOSABA_LIN_REWARD_<NORMALIZE(stem)>`。**必须在 `MainFile.Initialize()` 注册**（读档靠前缀 `RewardFromSerializableExtPatch` → `TryCreate`）。先例 `Hiro/Rewards/GuardOneBossUpgradeReward.cs`。
- **`OnSelect()` 在每台机器都跑** ⇒ 副作用必须**确定性**：只用 `CardSelectCmd.*`（内含 `PlayerChoiceSynchronizer`），别自弹 UI、别用本地随机。
- ⚠️ **`NDeckUpgradeSelectScreen` 会卡死**：它的确认要求 `已选数 >= prefs.MaxSelect` ⇒ **MaxSelect 必须先按可升级张数收窄**（`Math.Min(2, count)`），且 `count == 0` 直接 `return true`。
- ⚠️ **拿不到已死 BOSS**：`Hook.AfterCombatEnd` / `IterateHookListeners` 只遍历还在 `_allies`/`_enemies` 里的生物 ⇒ 「击杀 BOSS」用 **`MonsterModel.AfterDeath`**（`wasRemovalPrevented` 会先以 `true` 来一次）。
- ⚠️ **`AfterDeath` 里别做「奖励屏 / 玩家选择」这类重流程**：引擎防死分支是**先 `Hook.AfterDeath(true)`、后 `Hook.AfterPreventingDeath`** ⇒ 里面 `RewardsCmd.OfferCustom` 会把「防死 / 诈尸」整段吃掉（症状：诈尸音效都响了却原地暴毙）。覆写 `AfterDeath` 必须先判 `wasRemovalPrevented`（先例 `GuardTwoBossMonster`）。诈尸护盾别写成「某能力层数 × N」（层数可能还没建立 ⇒ 0 盾被 `Block <= 0` 的「破盾即杀」立刻打死），要有下限。

## 能力（Power）图标尺寸 / 美术

- 原版 `NPower` 的图标是 `%Icon` 这个 **`TextureRect`**（`NPower.cs:139/184/211`），
  `expand_mode` / 拉伸模式写在**场景里**、我们改不到。
- 本项目 `images/powers/*.png`（小图标）**实测全部 ≈64×64**（64×63 ~ 64×66）；
  `images/powers/big/*.png`（触发闪光 `_powerFlash`）实测跨度 **157×157 ~ 770×770**。
  ⇒ **小图标槽只敢放 64×64 档的图**；大图档是宽尺寸安全的。
- ⚠️ 别把**卡牌大图**（`images/cards/big/*.png` = **606×808 竖构图**）塞进 64×64 的小图标槽：
  万一场景是 `KEEP_SIZE`，`TextureRect` 的最小尺寸会变成 606×808、把整排能力图标撑爆。
  只用它当 `BigIconPath`（闪光），小图标用现成的 64×64 同主题图
  （也可以几个同主题能力共用一张，如 `images/powers/emotionpower.png`）。
- 想给某个能力单独配图标：把 `{typename小写}.png`（64×64）丢进 `ManosabaLin/images/powers/`，
  `ManosabaPowerTemplate.AssetProfile` 会自动解析；缺失回落 `power.png`。
  ⚠️ 新增美术要**重发布**（PCK 才会带进去）。

## 本地化 JSON 的 BOM / 行尾（2026-09-29 实测）

| 文件 | BOM | 缩进 | 备注 |
| --- | --- | --- | --- |
| `zhs/powers.json` | **无** | **TAB** | |
| `zhs/orbs.json` | **无** | 2 空格 | |
| `zhs/relics.json`、`zhs/cards.json` | **有** | 2 / 4 空格 | ⚠️ 曾误记为「zhs 全无 BOM」 |
| `eng/jpn/kor/rus` 全部 json | **有** | 2 / 4 空格 | `powers.json` 末尾无换行 |

- ⇒ 批量脚本必须 `open(path, 'rb')` → `decode('utf-8-sig')`，写回时**原样补回 BOM**，
  否则要么读崩（`json.JSONDecodeError: Unexpected UTF-8 BOM`）、要么把 BOM 弄丢。
- 校验一条命令：`json.loads(utf-8-sig)` + `CR 数 == LF 数` + `head -c3 | od -An -tx1` 看 BOM。
- 行尾是 **CRLF**，且**最后一行通常没有换行** ⇒ 写回时用 `newline=''` + 自己 `\n → \r\n`，别让它变成 LF。
- 可重复执行的追加脚本先例：`.local/add_overflow_loc.py`（按 `if key in text: skip` 幂等）。
