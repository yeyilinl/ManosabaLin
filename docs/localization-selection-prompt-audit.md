# 本地化「选择提示键」审计与修复（2026-09-30 23:59）

## 一、问题本质

卡牌 / 遗物 / 能力**自己弹选牌界面**时，代码里会写 `SelectionScreenPrompt`（或拼 `$"{Id.Entry}.selectionScreenPrompt"`）。
引擎实现（`.local/probe_engine/decomp/MegaCrit.Sts2.Core.Models/CardModel.cs:116`，`RelicModel.cs:69`、`PowerModel.cs:89` 同构）：

```csharp
protected LocString SelectionScreenPrompt {
    get {
        LocString locString = new LocString("cards", base.Id.Entry + ".selectionScreenPrompt");
        if (!locString.Exists())
            throw new InvalidOperationException($"No selection screen prompt for {base.Id}.");
        ...
    }
}
```

`Exists()` 最终是 `LocManager.Instance.GetTable(table).HasEntry(key)` —— **精确匹配**：

- **没有语言回退**（韩语表缺键 ⇒ 韩语崩，只有韩语崩）；
- **没有大小写容错**（`.SelectionScreenPrompt` 与 `.selectionScreenPrompt` 是两个不同的键）。

> ⚠️ 反例澄清：**余火选择界面不读卡自己的键**。`YalisalinFireComponentContext.Text()` 用的 `LocPrefix` 是**常量** `"ManosabaLin.YalisalinFireComponent"`，
> 即全余火卡共用 `ManosabaLin.YalisalinFireComponent.selectionScreenPrompt`（五语言都有）。所以余火卡**不需要**各自的 `selectionScreenPrompt`；
> 且 `new LocString(...)` 直接构造时缺键只会**显示原始键名**，不会抛。

## 二、修掉的 2 处真实崩溃

| # | 键 | 语言 | 现象 | 修法 |
| --- | --- | --- | --- | --- |
| 1 | `MANOSABA_LIN_RELIC_WITHEMA.`**`S`**`electionScreenPrompt` | 五语全崩 | `Withema`（艾玛 Starter 遗物「身份象征之物」，`Withema.cs:220`，在「疏远 == 7 且打出疏远牌」时触发）抛 `InvalidOperationException` | **键名首字母改小写**（值不动） |
| 2 | `MANOSABA_LIN_CARD_SAME_PLACE_TRUTH.selectionScreenPrompt` | 仅 kor | 韩语下「旧识疑影」打出即崩 | 补 `손패에 추가할 카드 선택` |

顺带对齐（**无代码引用，可随时撤**）：`zhs` 补 `MANOSABA_LIN_CARD_UNWANTEDKINDNESS.selectionScreenPrompt` = `选择1张卡牌作为[color=#ff0000]余火[/color]`（eng/jpn/kor/rus 本就有）。

脚本（幂等）：`.local/fix_selection_prompt.py`；备份：`.local/loc_backup_selprompt/`。
改后已 `dotnet publish`（EXIT=0）；PCK 核验：小写键 5 命中 / 大写键 0 命中（zhs 侧）。

## 三、审计脚本的三个坑（都踩过，别重复）

1. `grep 'SelectionScreenPrompt'` **大小写敏感** ⇒ 漏掉拼接式 `$"{Id.Entry}.selectionScreenPrompt"` 和已成小写的键。**改大小写不敏感**。
2. 归一化比对时**忘了剥前缀** `MANOSABA_LIN_(CARD|RELIC|POWER)_` ⇒ 拿 `AnansSketchbook` 去比 `MANOSABA_LIN_RELIC_ANANS_SKETCHBOOK`，**92 个类全判「缺键」**。
3. 判定写成 `if t not in present.get((l, t), {})`（拿**表名**当键去查字典）⇒ 同样全判缺。**要拿归一化后的 entry 名去查。**

结论必须基于 **entry 精确拼写 + 精确大小写**。核对命令（最小版）：

```bash
# 某表某语言缺不缺这个键
python -c "import json;d=json.load(open('ManosabaLin/localization/kor/cards.json',encoding='utf-8-sig'));print('MANOSABA_LIN_CARD_SAME_PLACE_TRUTH.selectionScreenPrompt' in d)"
# 全库找「后缀首字母大写」的可疑键（引擎只认小写）
python -c "
import json,glob
for p in sorted(glob.glob('ManosabaLin/localization/*/*.json')):
    d=json.load(open(p,encoding='utf-8-sig'))
    for k in d:
        if k.startswith('MANOSABA_LIN_') and '.'.join(k.split('.')[1:])[:1].isupper(): print(p,k)"
```

## 四、遗留观察（未处理）

- 仓库根目录有 **46 个 `map_*.json`**（08-14 生成的本地化分块，`.gitignore:428` 的 `/map_*.json`），会被 `dotnet publish` **一并打进 PCK**。
  其中 `map_eng_relics2.json` / `map_jpn_relics_events2.json` / `map_kor_relics_2.json` / `map_rus_relics_2.json` **仍带旧的大写键**。
  判定为**惰性残留**：任何代码都没引用 `map_` 前缀（RitsuLib 与游戏反编译均无匹配），且它们完全不含 08-14 之后新增的内容（`COMBUSTION_SHARED` = 0 命中）。
  ⇒ 不影响本次修复，但建议后续清理或在导出时排除，以免误判。

## 五、第二次复核：用户点名的 4 张卡（2026-10-01 01:00）

用户点名「`ContrastWound` / `FullCourtVerdict` / `SinMarket` / `SinOffering` 用了 `SelectionScreenPrompt` 而 5 语言都缺键」——
**结论：`.selectionScreenPrompt` 五语言全部齐全，不缺、不崩**（这条是我 09-30 的误报，见第一版脚本坑）。逐项证据：

| 检查项 | 结论 |
| --- | --- |
| 键存在性 | 4 张卡 × 5 语言 `.selectionScreenPrompt` **全部存在且非空** |
| PCK 字节计数 | `MANOSABA_LIN_CARD_CONTRAST` = 20 = 5 语 × 4 键（title/description/selectionScreenPrompt/yesNoPrompt）、`..._FULL_COURT` = 15 = 5×3、`..._SIN_MARKET` = 20、`..._SIN_OFFERING` = 15 —— **与"键齐全"精确吻合** |
| entry 拼写 | 无下划线的 `FULLCOURT` / `SINMARKET` / `SINOFFERING` 在 PCK 中命中 **0 次** ⇒ 带下划线才是 `Id.Entry` 的正确形式，本地化键拼写没错 |
| 占位符安全 | 20 条 prompt 值**均不含 `{...}`** ⇒ 不会触发 `LocException` |

**顺带查出的真缺口**（与 `selectionScreenPrompt` 无关）：`FULL_COURT_VERDICT` 与 `SIN_OFFERING` 的 **`.yesNoPrompt` 五语言都缺**。
代码里目前**只有** `ContrastWound.cs:67` 与 `SinMarket.cs:44` 走 `new LocString("cards", $"{Id.Entry}.yesNoPrompt") + YesNoChoiceScreen.Pick(...)`
（这两张的键都在），所以**不是崩溃**；已为对齐补上 2 键 × 5 语言（`.local/fix_prompt_and_stoke.py`，BOM/LF/末尾换行逐文件保持）。

> 复用的判据：`CardModel.SelectionScreenPrompt`（`_cardmodel_decomp/...CardModel.decompiled.cs:116`）
> = `new LocString("cards", base.Id.Entry + ".selectionScreenPrompt")` + `if (!Exists()) throw`
> ⇒ **键名（含大小写、下划线位置）就是契约**。
