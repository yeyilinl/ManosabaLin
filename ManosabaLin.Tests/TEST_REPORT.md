# 亚里沙（Yalisalin）卡牌测试报告

**日期**: 2026-09-09 ｜ **最终状态**: 76 个测试全绿（`SUMMARY total=76 passed=76 failed=0 skipped=0`）

## 覆盖率

| 统计项 | 数量 |
|---|---|
| 全部卡牌 | 75 |
| 已测试 | 72 |
| 不可测（多人专用） | 1（YalisalinMlym） |
| 已核查但发现 bug 移除测试 | 2（Temperatureproof、Pocketmatchbox） |

## 测试文件（9 个，`ManosabaLin.Tests\Cases\`）

| 文件 | 测试数 | 覆盖内容 |
|---|---|---|
| YalisalinSmokeTests | 2 | 基础攻击/防御 |
| YalisalinCoreCardTests | 8 | SuspectSpread、SinGift、Nyxm、MentalTrauma、DualTrackDay、Two、JiHenFanShi、Kkm |
| YalisalinPowerCardTests | 10 | Aam、Nym、KuanShuYinJi、NiNiZhiZheng、GuiltyChain、XingJiaJiaShen、XunHuanFaDian、Mgm、Mllm、Lym |
| YalisalinSkillCardTests | 11 | One、Eight、WitchTrial、WitchPrisoner、Twelve、ZuiYeShengDun、RedemptionCorridor、CardTen、Three、Tower、ZuiZhaiDiYa |
| YalisalinMiscCardTests | 5 | Seven、Amm（含加成）、Powertwotwo、Powerfourfour |
| YalisalinSinChoiceTests | 7 | Indictment、SinOffering、ContrastWound、FullCourtVerdict、SinMarket、Friend、ZhiMingHuanYa |
| YalisalinFireColorTests | 8 | 火色系（带/不带发夹两套） |
| YalisalinFireColorMoreTests | 9 | Unusedconclusion、Ashinpages、Burntthermometerpaper、Dontcooldown、Grazingcritical、Deadlinehandoff、Ticketonwindow、Tomorrowburn、Thirteenthlistener |
| YalisalinFireComponentTests | 16 | Glasshug、Unwantedkindness、Beforeforgiven、Burnedecho、Fifthselfproof、Holdmypain、Dazzlingtolerance、Burnedapology、Unneededgoodchild、Twoseparatedends、Warmthshouldnotstay、Returnedhairribbon、Dontlookatme、Brokentrust、Stayingstillhurts、Dontbringmehome |

## 已知问题

### 🔴 问题 A（真实 bug，未修）：Temperatureproof / Pocketmatchbox 打出必崩
- **现象**：打出时抛 `InvalidOperationException: No selection screen prompt for CARD.MANOSABA_LIN_CARD_XXX`。
- **根因**：两卡的 OnPlay 主路径调用 `SelectionScreenPrompt`（CardModel 属性），它读取本地化键 `cards.<ID>.selectionScreenPrompt`；**5 种语言（zhs/eng/jpn/kor/rus）的 cards.json 均缺这两个键**。
- **影响**：玩家实际游玩打出这两张卡同样会崩。这是卡牌本地化不完整导致的运行时崩溃。
- **修复方向**：为 `MANOSABA_LIN_CARD_TEMPERATUREPROOF` 和 `MANOSABA_LIN_CARD_POCKETMATCHBOX` 在 5 语 cards.json 补 `selectionScreenPrompt` 键（如"选择1格火色进行封存"）。
- **测试陷阱**：Temperatureproof 整批跑时"假通过"——先执行了 7 伤（GameAction 计数满足）后异常被吞，单独 filter 跑才暴露 TERMINATE。

### 🟡 问题 B（潜在崩溃点）：Beforeforgiven / Glasshug / Unwantedkindness 也缺键
- 三卡代码引用 `SelectionScreenPrompt`（仅条件分支路径），5 语同样缺 `selectionScreenPrompt` 键。当前测试路径未触发，但特定条件（如 fire component 联动选择）触发时会崩。建议一并补键。

### ⚪ 不可测：YalisalinMlym（罪债抵押多人版）
- `MultiplayerConstraint.MultiplayerOnly`，单人测试框架无法打出。需多人环境手动验证。

> ⚠️ **2026-09-30 复核更正（问题 A / 问题 B 的结论已过期）**
> 上面 A/B 两条是按**当时的代码**写的。之后 `Temperatureproof` / `Pocketmatchbox` 已改为读消耗记录（`ConsumptionLog` 差量），
> **不再引用 `SelectionScreenPrompt`** ⇒ 「打出必崩」不再成立（因此这两张至今没有、也不再需要该键）；
> `Glasshug`（`GLASSHUG`）/ `Beforeforgiven`（实际键名 `BEFOREFORGIVEN`）**5 语言都已有键**。
> 全库穷尽复核（113 处「选择提示键」引用）后**真实且仍存在的崩溃点只有 2 处**，均已于 2026-09-30 23:59 修复：
> ① 五语 `relics.json` 的 `MANOSABA_LIN_RELIC_WITHEMA.SelectionScreenPrompt` **大小写写错**（应为小写 s）⇒ `Withema` 在「疏远==7 且打出疏远牌」时崩；
> ② `kor/cards.json` 缺 `MANOSABA_LIN_CARD_SAME_PLACE_TRUTH.selectionScreenPrompt` ⇒ 韩语下「旧识疑影」崩。
> 判据：`LocString.Exists()` = `LocTable.HasEntry(精确键名)`，**无语言回退、无大小写容错**；余火选择界面用的是**共享键** `ManosabaLin.YalisalinFireComponent.selectionScreenPrompt`，故余火卡不需要各自的键。

### ✅ 已确认正确的行为（此前轮次核实）
- ContrastWound 手牌无原罪时弹窗询问获得原罪+宽恕+伤害减半——设计正确（本地化一致）。
- ZuiZhaiDiYa 无原罪时直接返回只给格挡——正确。
- Afterschooltestburn 无发夹也打 6 伤（消耗火色是额外效果）——正确。
- Unseenkindling 逻辑与本地化一致（无火色给2格/有火色给1格+抽1/升级+4格挡）。

## 基础设施（已修复并稳定）

- **headless 弹窗崩溃**（原问题1）：测试 Entry 安装 `AutoFirstCardSelector`（`CardSelectCmd.UseSelector(localOnly)`），所有 `CardSelectCmd` 选择自动完成，不再弹 UI 崩溃；崩溃类测试不再 TERMINATE 中断整批（原问题5）。
- **运行命令**：`cd D:\ManosabaLin; dotnet msbuild ManosabaLin.Tests/ManosabaLin.Tests.csproj -t:RunSts2Tests -v:minimal`
- **单测**：加 `-p:Sts2TestArgs=--sts2-test-filter=<测试名>`
- **日志**：`D:\tmp\sts2-combat-tests\ManosabaLin_tests\logs\run.log`
