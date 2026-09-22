# 项目长期记忆 — ManosabaLin

## 本地化（localization）约定

- 目录：`ManosabaLin/localization/{zhs,eng,jpn,kor,rus}/{cards,powers,relics,enchantments,card_keywords,ancients,events,characters,monsters,encounters,afflictions,orbs,settings_ui,static_hover_tips}.json`
- **zhs 是源语言**：新增/修改文案先在 zhs 定稿，再同步其他语言。
- 键命名：
  - `MANOSABA_LIN_CARD_<SNAKE>` / `MANOSABA_LIN_POWER_<SNAKE>` / `MANOSABA_LIN_RELIC_<SNAKE>` / `MANOSABA_LIN_CHARACTER_<SNAKE>`
  - `.title` / `.description` / `.flavor` / `.smartDescription` / `.selectionScreenPrompt`
  - 模型能力：`MANOSABA_LIN_MODEL_CAPABILITY_<SNAKE>.hovertip.title|description`、`.afterBase`、`.afterBaseStrong`
  - C# 组件走 `LocString("cards", "<LocPrefix>.<suffix>")`，如 `ManosabaLin.YalisalinFireComponent.*`、`ManosabaLin.AnanlinReassuranceMarkCapability.*`
- 变量占位符必须原样保留：`{Damage}` `{Block}` `{Cards}` `{Energy}` `{Count}` `{Amount}` `{Slot}` `{Prompt}`、`{X:diff()}`、`{IfUpgraded:show:...|...}`、`{energyPrefix:energyIcons(n)}`
- 富文本标签保留并随语言本地化：`[color=#RRGGBB]` `[b]` `[gold]` `[purple]` `[green]` `[blue]`；各行断行用 `\n`
- 文件为 **CRLF + 部分带 UTF-8 BOM**；批量改值必须保留行尾与 BOM（用字节级读入/写出，不要 `ensure_ascii=True` 重写整文件）。

### 术语表（务必复用）

| 中文 | eng | jpn | kor | rus |
| --- | --- | --- | --- | --- |
| 余火 | Embers | 余燼 | 잔불 | угли |
| 添火 | kindle | 火を加える | 불을 더하다 | добавить огонь |
| 升温 / 强升温 | Heat Up / Strong Heat Up | 昇温 / 強昇温 | 승온 / 강한 승온 | Нагрев / Сильный нагрев |
| 魔女化 | Witchification | 魔女化 | 마녀화 | Оведьмление |
| 亚里沙的魔法 | Alisa's Magic | アリサの魔法 | 아리사의 마법 | магия Алисы |
| 火色（浅橙/亮黄/赤红/黑红） | Dark Orange / Dark Red / Crimson / Blackened Red Char | 暗橙 / 暗紅 / 真紅 / 黒紅 | 어두운 주황 / 어두운 빨강 / 새빨강 / 검붉은 탄화 | тёмно-оранжевый / тёмно-красный / алый / чёрно-красный |
| 嫌疑 | Suspicion | 容疑 | 의혹 | Подозрение |
| 缄默 | Silence | 沈黙 | 침묵 | Молчание |
| 封存火色 | Sealed Fire | 封存火色 | 봉인 화색 | запечатанный огонь |
| 魔女监狱 | Witch Prison | 魔女監獄 | 마녀 감옥 | Тюрьма ведьм |
| 魔女之力 | Witch's Power | 魔女の力 | 마녀의 힘 | Сила ведьмы |
| 魔女仪式 | Witch Ritual | 魔女儀式 | 마녀 의식 | Ритуал ведьмы |
| 无实体 / 虚空形态 / 死神形态 | Intangible / Void Form / Reaper Form | 無実体 / 虚空形態 / 死神形態 | 무형 / 공허 형태 / 사신 형태 | Бестелесность / Форма пустоты / Форма смерти |
| 怀旧 / 他人的情绪 | Nostalgia / Others' Emotions | 懐旧 / 他者の感情 | 향수 / 타인의 감정 | Ностальгия / Эмоции других |
| 书本中的真相 | Truth Within the Pages | 書物の中の真実 | 책 속의 진실 | Истина в книгах |
| 疑心暗起 / 誓言 | Suspicion Arises / Oath | 疑心暗鬼 / 誓い | 의심이 싹트다 / 맹세 | Зарождающееся подозрение / Клятва |
| 试镜 / 安心 | Audition / Peace of Mind | オーディション / 安心 | 오디션 / 안심 | Прослушивание / Спокойствие |
| 搬石 / 移除 | Stone Hauling / Remove | 石運び / 取り除き | 돌 나르기 / 제거 | Камень преткновения / Удалить |
| 泽度可可 / 黑部奈叶香 / 莲见蕾雅 | Sawado Koko / Kurobe Nayuka / Hasumi Reiya | 沢渡ココ / 黒部ナユカ / 蓮見レア | 사와도 코코 / 쿠로베 나유카 / 레아 | Коко Савадо / Наёка Куробэ / Лея Хасуми |
| 紫藤亚里沙 | Wisteria Arisa（卡面用 Alisa） | 紫藤アリサ | 위스테리아 아리사 | Ариса Фудзито |

## 复用工具（`.local/tools/`）

- `loc_gap_report.py <filefilter> <keyfilter>` — 5 语言键缺失/空值对比
- `cjk_leak_check.py` — 检出非中文语言里残留中文（jpn 会有大量误报，需人工判断）
- `code_loc_audit.py` — 代码引用的本地化键 vs JSON 实际键
- `power_key_check.py` — C# Power 类 → `MANOSABA_LIN_POWER_*` 键覆盖检查
- `fix_fire_loc.py` / `fix_ability_loc.py` — 按 key 精确替换值的模板（保 BOM/CRLF + `\n` 规范化）
- `untranslated_scan.py` / `untranslated_breakdown.py` — 全库未翻译条目扫描（「值==zhs 原文且含中文」）
- `ability_union_dump.py` — 能力键 5 语言并集对照，判断缺口用
- `verify_ability_loc.py` — 残留 / 占位符 / 改动范围 / BOM / 行尾 四项校验
- `check_eol.py` — 行尾风格回归检查

## 易踩的坑

- **未翻译判据要用「值 == zhs 原文 且含中文」**，不要用「含中文」——否则 eng 的正常译文会被大量误报。
- **jpn 的"无假名且含汉字"绝大多数是合法日文**（魔女監獄 / 疑心暗鬼 / 洗脳 / 絆 / 正義 / 真実 / 昇温…），
  必须逐条人工确认；`封存火色` 是 jpn 既有约定（全库 32 处），不要改成 封印。
- 能力键缺失要双向查：JSON 里没有该键 ≠ 代码引用了错的键；也要查「键在但值是中文」。
- `MANOSABA_LIN_CARD_*_EFFECT`、`MANOSABA_LIN_RELIC_*` 这类字符串在代码里也可能是**卡牌/遗物 ID**，不一定是本地化键。
- `afterBase` 是卡面追加文字（不是 hover），`hovertip.*` 才是悬浮提示。
- **`ManosabaLin.YalisalinFireComponent.enhancement.*` 是余火组件悬浮提示的追加条目**
  （由 `YalisalinFireComponentCapability.CreateHoverTip` 用 `"\n- "` 拼进 description，
  来源 `YalisalinsHairpin.GetFireComponentEnhancementDescriptions`），**不是能力本地化**、
  不需要 Power 级条目。其中 `dazzlingTolerance` / `burnedApology` / `warmthShouldNotStay`
  由 Power 驱动，对应的 `MANOSABA_LIN_POWER_*` title/description 已于 2026-09-20 删除（重复条目）；
  其余 4 条（`absentThirteenth` / `painKeeper.count` / `separatedEnds` / `unneededGoodChild`）与 Power 无关。
- 卡片 hover 能力说明由能力自身/loc 决定，**不要**为补本地化而往卡牌上加 hover 提示。
- **「回合开始时（在手牌中）」类效果用卡牌的 `AfterPlayerTurnStart(choiceContext, player, componentContext)` 钩子**
  （先例：`EmaForgottenOne`、`WitchBurn`、`EmptyHouse`）；该钩子对战斗中的卡牌模型触发，
  不保证只在手牌，需自己判定 `Pile?.Type == PileType.Hand`。
- **变身类效果优先用基底的 `CardCmd.Transform(original, replacement)`**（原牌位替换 + 变身表现，
  先例：`EmaEnding`、`Xueqinjincard1`、`Hiroshuyuanpower`、`AnanlinMiliaAssist`）；
  调用前必须守卫 `original.IsTransformable` 与 `CombatState != null`，否则抛 `InvalidOperationException`。
  可选选择用 `new CardSelectorPrefs(prompt, 0, 1)`（min 0 = 「可以」选择）。
- **复制一张战斗中的牌一律用 `source.CreateClone()`（`CardModel.CreateClone`），不要用
  `CombatState.CreateCard(CanonicalInstance, owner)` + 手动升级**——后者只带基础牌 + 升级等级，
  会丢附魔 / 幻附魔 / 关键词改动 / 费用改动 / MinionLib 组件。
  `CreateClone` = `MutableClone()`（`MemberwiseClone` + `DeepCloneFields` + `AfterCloned`），
  上述全部带齐并重置 `DeckVersion`/事件，设 `ExhaustOnNextPlay = false`、`CloneOf = 原牌`；
  原版 `DualWield` 用的就是这条路径。`IsClone` 语义是"继承原牌当前状态、跳过一次性初始化"，是有益的。
  注意 `AddGeneratedCardToCombat` 要求传入的牌 `Pile == null`。
- **游戏源码副本在 `.local/probe_engine/decomp/` 与 `.local/probe_engine/full/sts2.decompiled.cs`**
  （`AGENTS.local.md` 里的 `D:\33` 已失效，2026-09-20 确认不存在）。查基底 API 用这里。
- 构建时若 `SlayTheSpire2.exe` 正在运行，`CopyMod` 目标会因 DLL 被锁而报 `MSB3027/MSB3021`
  ——这是部署失败，不是编译失败；关游戏后重新 build 即可。
- 判断某 Power 是否会在战斗中出现：看它有没有覆写 `IsVisibleInternal`。
  `ManosabaPowerTemplate` 默认 `StackType != None` 即显示；`Originalsin*CounterPower` 等
  「隐藏」能力都显式写了 `=> false`。
- **写本地化 JSON 的两条硬规矩**（历史脚本踩过）：
  1. 行尾必须跟随文件既有风格 —— 本地化 JSON 主体是 **CRLF**，逐行替换时别只写 `\n`，
     否则产生混合换行（会让 git diff 变成整文件噪声）。
  2. 缩进按文件既有风格 —— `zhs/powers.json` 通体用 **制表符**，其余语言 powers.json 用 2 空格。
  处理方式：字节级读入 → 只改目标行 → 原样写出；改完用换行符计数核对（CRLF 数应等于 LF 数）。

## 本机环境 / 游戏存档（2026-09-21 定）

- **Bash 工具的 coreutils 全不可用**：`ls` / `grep` / `wc` / `head` / `date` 一律 `command not found`
  （`PortableGit\...\shim` 里 `dirname` 都缺失）。要跑 shell 逻辑只能用 PowerShell；
  文件查找/内容搜索改用 Glob / Grep 工具。
- **PowerShell 工具的 stdout 不回传**（返回只有 `exit code`）。
  需要看输出时一律 `... | Set-Content -Encoding UTF8 <临时文件>`，再用 Read 工具读。
- **游戏存档真实路径**：`%APPDATA%\SlayTheSpire2\steam\<steamid>\modded\profile1\saves\`
  （不是 `%APPDATA%\SlayTheSpire2\default\<n>\`！后者是旧档案残留）。
  确认方法：日志里搜 `Profile-scoped data path initialized:`。
- 存档 JSON 格式：**2 空格缩进 + 纯 CRLF + 无 BOM**。改档只能
  `ReadAllText` → 字符串替换 → `UTF8Encoding($false)` 写回；
  **禁止 `ConvertTo-Json` 往返**（会重排、重转义整个文件）。
- 游戏对自己的存档有 `*.FUT.corrupt` / `*.VAL.corrupt` 隔离命名；
  `current_run.save` 没被改名说明游戏自身校验是通过的。
- 读档失败定位法：看 `current_run.save` 的 mtime 有没有被改写
  （`SetUpSavedSingleplayer` → `IncrementNumReloads` 会写档）；没改写 ⇒ 炸在
  `RunState.FromSerializable`。其内部顺序是**先 players 后 acts**，由栈可直接区分。
- **`RoomSet.FromSave`（`MegaCrit.Sts2.Core.Rooms/RoomSet.cs:144`）对
  `EventIds` / `NormalEncounterIds` / `EliteEncounterIds` 裸调 `.Select()`**：
  任何 mod 的自定义 act 只要这三个列表为 null，读档就抛
  `ArgumentNullException (Parameter 'source')`，整个 run 再也读不回来。
  `HouseOfSpidersRyoshu` 的 `SpiderNestAct`（手写 3 节点图：ancient→shop→rest→boss）就是这种情况。
