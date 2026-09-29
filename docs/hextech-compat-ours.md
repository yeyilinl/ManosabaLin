# 与海克斯符文联动 · 本项目施工单

> **配对文件**：`docs/hextech-compat-hextech.md`（给海克斯作者的需求，可直接当 Issue 发）。
> **另一份施工单**：`docs/hextech-universal-runes-design.md`（**通用符文**：一角色一个，真·通用 / 纯原版机制；**只出设计、未落地**）。
> **通用参考**：`docs/hextech-compat-design.md`（跨 mod 联动的五层套路 L0~L4 + `[ModInterop]` / `[AssemblyInterop]` 硬规则）。
> **权威依据**：**海克斯作者发布的《外部模组对接指南》**（软依赖 `HextechRunesInterop` / 硬依赖 `HextechRunesApi`）。
> 本文档凡是与它冲突的，一律以它为准。
> **对方基准**：HextechRunes `0.9.6` 之后（本机 `lib/0.111.0/HextechRunes.dll`，全量反编译 `.local/hex_src/`）。
>
> **路线（已按官方指南改定）**：**软依赖 `HextechRunesInterop`；零 Harmony patch；不硬引用 dll；不建子程序集。**
> 我方符文 = 我们自己的 `RelicModel` 子类，图标/文案/稀有度全在 ManosabaLin 自己的 PCK 里；
> 海克斯只负责**识别 + 载入**（品级分层、图鉴分组、配置菜单、抽取池）。

---

## 0. 官方对接指南的硬性约束（红线，改代码前先读这节）

| # | 约束 | 依据 |
| --- | --- | --- |
| 1 | **`HextechCatalog` 等 `internal` 类不属于对外契约，随时可能重构，禁止对它们打 Harmony 补丁。**缺能力请开 issue | 指南「软依赖接入」开头 |
| 2 | manifest 里**不要**写 `dependencies: ["HextechRunes"]`（否则没装的玩家加载不了我们的 mod） | 指南「注册时机」 |
| 3 | 按**程序集名** `HextechRunes` 检测，**不要按 manifest id**（本体按游戏版本加载不同变体 DLL；二创版也可能用同一程序集名） | 指南「注册时机」 |
| 4 | 模组之间初始化顺序不固定 ⇒ **先查一次已加载程序集，没找到就订阅 `AppDomain.CurrentDomain.AssemblyLoad`**，载入后立即注册 | 指南「注册时机」 |
| 5 | 注册必须在**模组初始化阶段**完成：模型池要在**共享遗物池首次枚举前**登记；带 `[SavedProperty]` 的符文还要赶在**存档序列化缓存初始化之前**。窗口关闭后再调用抛 `InvalidOperationException`，且**不留半登记状态** | 指南「注册时机」 |
| 6 | 符文必须是**具体、非泛型**的 `RelicModel` 子类；`Rarity` **必须** `RelicRarity.Starter` | 指南「符文类的要求」 |
| 7 | 图标**由我们自己提供**（`PackedIconPath` / `PackedIconOutlinePath` / `BigIconPath`），不依赖海克斯的资源路径 | 指南「符文类的要求」 |
| 8 | `HextechRelicBase` 提供的扩展（阵营回合钩子、`*Compat` 伤害修正、生成卡牌辅助）**不对外部符文开放** ⇒ 外部符文只用**原版 `RelicModel` 的钩子** | 指南「符文类的要求」 |
| 9 | `isAvailableForPlayer` 委托**联机两端都会执行，必须得出相同结果**；只能读同步状态（`Player` 的角色/遗物/牌组/`RunState`），**不要读本地设置、UI 或随机数** | 指南「isAvailableForPlayer」 |
| 10 | 参数无效抛 `ArgumentException`（反射调用时包在 `TargetInvocationException` 里）；**所有校验都在产生任何副作用之前完成** | 指南「RegisterPlayerRune」参数表 |
| 11 | 同一个类型重复注册 ⇒ **以第一次登记的元数据、资源归属和过滤委托为准**，之后只在日志里警告 | 指南「RegisterPlayerRune」 |

**术语校准（原先我们搞错的地方）**：

- 软依赖路线下，**符文根本不需要继承 `HextechRelicBase`** —— 任意 `RelicModel` 子类即可。
  `HextechRelicBase` 只在**硬依赖**（`HextechRunesApi`）路线才强制。
- 「符文的资源归属」由**我们自己的** `PackedIconPath` 决定，不是海克斯的
  `res://{assetModId}/images/relics/{小驼峰}.png` 命名链（那条链只在硬依赖路线走）。
- 来源标签 / 分组标题的本地化表是 **`relic_collection`**（文件 `relic_collection.json`），
  不是 `relics`；符文**自身**的名称/描述才走 `relics` 表。
- 「第三方符文要不要自己进遗物池」有了官方答案：**海克斯会把它加进原版 `SharedRelicPool`**
  （为了拿到模型身份 + 图鉴显示），再靠「只有 `Starter` 能保证原版别的抽取路径抽不到」来兜底。
  所以我们**不需要**为联动去动池注册；我们自己那份 `[RegisterRelic(typeof(LinRelicPool))]` 只服务于
  「没装海克斯时也能玩」。

---

## 1. 结论

1. **零 patch。** 官方 `HextechRunesInterop` 覆盖了我们全部诉求（注册符文 / 品级 / 标志 / 角色池 / 标签 /
   可用性过滤 / 来源标签 / 配置菜单分组标题 / 额外阶段提示）⇒ 上一版文档里那 18 个 `HextechCatalog`
   postfix **全部作废**（并且现在是被明令禁止的做法，见 §0-1）。
2. **不硬引用、不建子程序集。** 用**反射**调 `HextechRunesInterop` 的 public static 成员
   （或 RitsuLib `[ModInterop]`，但对海克斯**不推荐**，理由见 §2.4）。产物 DLL 里**不得**出现对
   `HextechRunes` 的任何编译期引用。
3. **新增一个「注册桥」**：`ManosabaLinCode/Compat/Hextech/HextechRuneRegistrar.cs`（+ 门控
   `HextechCompat.cs` + 元数据表 `HextechRuneCatalog.cs`）。这是本次联动**唯一**要写的新代码。
4. **我们的符文本来就是合规的**：`ManosabaRelicTemplate : ModRelicTemplate : RelicModel`，
   `Rarity` 自己覆写 ⇒ 只需保证**每个海克斯符文都声明 `RelicRarity.Starter`**（§0-6）。
5. **硬依赖路线（`HextechRunesApi`）我们不走** —— 我们不是「与海克斯一起发布」的拓展包，
   必须做到「对方没装也能跑」。
6. **注册桥已落地**（2026-09-28）：`Compat/Hextech/` 三个文件 + `MainFile.Initialize()` 接线，见 §2.9；
   6 个联动遗物本身（离线那条线）见 §2.8。两条线现在都在了。

---

## 2. 施工单

### 2.1 我方符文自己登记（不做任何海克斯相关继承）

- 符文 = 现有 `ManosabaRelicTemplate` 子类，按现有方式 `[RegisterRelic(typeof(LinRelicPool))]`
  （`LinRelicPool` 用 `[RegisterSharedRelicPool]`，见 §2.7）。
- 图标走 `ManosabaRelicTemplate.AssetProfile`（`res://ManosabaLin/images/relics/<类名小写>.png`，
  再问一次 `ResourceLoader.Exists` 回退 `relic.png`）；文案走原版 `relics` 表
  （`Id.Entry + ".title"/".description"/".flavor"`）；稀有度 `override RelicRarity => RelicRarity.Starter`
  ⇒ **全部不需要海克斯参与**。
- ⚠️ **不要**调 `ModHelper.AddModelToPool(typeof(SharedRelicPool), 我方符文)`：
  海克斯自己会把它加进 `SharedRelicPool`，我们重复加只会让自己更早被原版别的抽取路径看见。
- 本组 6 个联动遗物同时是**海克斯符文**（`Starter`），所以它们**两份身份共存**：
  ① 我方 `LinRelicPool` 里的普通遗物（离线可玩）；② 海克斯注册表里的符文（装了才生效）。

### 2.2 门控 `Compat/Hextech/HextechCompat.cs`

```csharp
internal static class HextechCompat
{
    public const string AssemblyName = "HextechRunes";       // ⚠️ 按程序集名，不按 manifest id
    public const string InteropTypeName = "HextechRunes.HextechRunesInterop";
    public const int MinimumApiVersion = 1;

    public static Assembly? Assembly { get; private set; }
    public static Type? InteropType { get; private set; }
    public static int ApiVersion { get; private set; }        // 0 = 不可用

    /// <summary>在 MainFile.Initialize() 里调用一次；未找到程序集时订阅 AssemblyLoad 等它。</summary>
    public static void Initialize() { /* 见 §2.4 骨架 */ }

    public static bool IsReady => Assembly != null && InteropType != null && ApiVersion >= MinimumApiVersion;
}
```

- **能力探测优先于版本号**：`ApiVersion` 只用来做「太旧就整体跳过」的门槛；具体能力用
  「目标方法/属性解析得到没有」判断（`GetMethod(name, flags, [参数类型...])` 返回 null ⇒ 该能力不可用，
  单独降级，不要整体放弃）。
- 复用现有基建：`Compat/Core/OptionalModCompatRegistry.IsModLoaded(string modId)` 是**按 manifest id**
  判的（`OptionalModCompatRegistry.cs:17`）⇒ **不能直接拿来做这次的检测**，需要按程序集名另写一个
  `IsAssemblyLoaded(string assemblyName)`。两者可以并存。
- `ApiVersion` 反射取不到 ⇒ 对方版本 < 0.9.6 ⇒ 打一条 Info 后**整体跳过**（指南「版本检查」）。

### 2.3 我方侧元数据表 `Compat/Hextech/HextechRuneCatalog.cs`

一张纯数据的表，**直接就是 `RegisterPlayerRune` 的入参**（不需要再转换对方类型，因为软依赖签名收的是
**字符串**枚举名）：

```csharp
internal sealed record HextechRuneSpec(
    Type RuneType,
    string Rarity,                   // "Silver" / "Gold" / "Prismatic"（不区分大小写；⚠️ 不接受数字）
    string? Flags,                   // 逗号分隔：Disabled / FirstActExcluded / ThirdActExcluded
                                     //           / SelectionExcluded / AttributeConversionExclusive
    string? CharacterPool,           // Ironclad/Silent/Regent/Defect/Necrobinder；通用 = null（见 §5-1）
    int CharacterOrder,
    string TagKey,                   // "MANOSABA_LIN"（自造，见 §2.5）
    string AssetModId,               // "ManosabaLin"
    Func<Player, bool>? Availability // 见 §0-9 确定性要求
);

internal static class HextechRuneCatalog
{
    public static IReadOnlyList<HextechRuneSpec> All { get; }
    public static bool Contains(Type t);
    public static bool TryGet(Type t, out HextechRuneSpec spec);
}
```

- 规则（品级 / 幕数 / 可用角色）**全记在我们自己这边**，注册时一次交出去；后续不读对方任何状态。
- `Availability` 必须是**确定性纯函数**（只看 `player.Character` / `player.Relics` / `RunState`）。

### 2.4 注册桥 `Compat/Hextech/HextechRuneRegistrar.cs` ★核心

**用反射还是 `[ModInterop]`？**

- **本项目选纯反射。** 理由：① 指南要求「按程序集名检测」，而 RitsuLib 的
  `[ModInterop(modId, type)]` 是**按 manifest id** 解析目标程序集的（`ModInteropAttributes.cs:18`）
  —— 二创版 manifest id 不同就会失效；② 我们要先读 `ApiVersion` 再决定要不要注册，纯反射更直接；
  ③ 指南给的就是反射示例，照抄即可。
- 若将来想用编译期校验，可另加 `[AssemblyInterop("HextechRunes.HextechRunesInterop, HextechRunes")]`
  空壳（assembly-qualified 形式与「按程序集名」的要求一致），但**代理方法参数列表必须与指南签名逐字一致**，
  且它不解决 `ApiVersion` 门槛，收益有限。

**注册时机（照抄指南，`MainFile.Initialize()` 里调 `HextechCompat.Initialize()`）**：

```csharp
public static void Initialize()
{
    var loaded = AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name == AssemblyName);
    if (loaded != null) { TryRegister(loaded); return; }
    AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;   // 载入后自解绑再注册
}
```

**一次注册要做的 4 件事**（顺序：先 `RegisterPlayerRune`，再两个界面文字）：

```csharp
var register = interop.GetMethod("RegisterPlayerRune",
    BindingFlags.Public | BindingFlags.Static, null,
    [typeof(Type), typeof(string), typeof(string), typeof(string), typeof(int),
     typeof(string), typeof(string), typeof(Func<Player, bool>)], null);
// ↑ 参数列表必须与指南完全相同；返回 null ⇒ 对方版本太旧，整体跳过
```

| 步骤 | 调用 | 说明 |
| --- | --- | --- |
| 1 | `RegisterPlayerRune(type, rarity, flags, characterPool, characterOrder, tagKey, assetModId, availability)` | 每个符文一次；参数无效抛 `ArgumentException`（反射时是 `TargetInvocationException.InnerException`）⇒ **捕获并 Warn，不要让它打断整个 mod 初始化** |
| 2 | `SetPlayerRunePoolLabel(type, "MANOSABA_LIN")` | 三选一界面「来源标签」；文字取 `relic_collection` 表的 `HEXTECH_POOL.MANOSABA_LIN` |
| 3 | `RegisterConfigSectionTitle("ManosabaLin", "MANOSABA_LIN_HEXTECH_SECTION")` | 配置菜单里我们这组内容的小标题；`titleKey` 是 `relic_collection` 表的**完整键** |
| 4 | 打一条汇总日志 | 注册了几个、`ApiVersion` 多少、失败的逐个列出 |

- ⚠️ 串行化保护：`Initialize()` / `OnAssemblyLoad` / 注册体都要能被重复调用而**不重复注册**
  （对方以第一次为准，但我们会白刷警告）。
- ⚠️ **不要**在注册体里做任何需要「游戏已初始化」的事；也不要写 `[SavedProperty]` 载体登记
  （那是硬依赖 `RegisterSavedPropertyCarrier` 的活；软依赖下我们自己不给符文加 `[SavedProperty]`
  ⇒ 符文必须**无持久字段**，需要跨战斗状态就往 `Power` / `RelicModel` 原有机制上放）。

### 2.5 界面文字与本地化

| 界面位置 | 我方设置 | 本地化键 | 未设置时 |
| --- | --- | --- | --- |
| 来源标签 | `SetPlayerRunePoolLabel(type, "MANOSABA_LIN")` | `relic_collection` 表的 `HEXTECH_POOL.MANOSABA_LIN` | 通用显示「通用」 |
| 海克斯标签 | `RegisterPlayerRune(..., tagKey: "MANOSABA_LIN")` | `relic_collection` 表的 `HEXTECH_TAG.MANOSABA_LIN` | `COMPREHENSIVE`（综合） |
| 配置菜单小标题 | `RegisterConfigSectionTitle("ManosabaLin", "MANOSABA_LIN_HEXTECH_SECTION")` | `MANOSABA_LIN_HEXTECH_SECTION` 本身 | 「外部模组：」 |

- **可以直接用海克斯已有键**，省得自己写：来源标签 `GENERIC / IRONCLAD / SILENT / REGENT / DEFECT / NECROBINDER`；
  海克斯标签 `COMPREHENSIVE OUTPUT SURVIVAL RESOURCE ORB SUMMON RANDOM SWORDCRAFT DOOM POISON ECONOMY
  STATUS STARLIGHT STACKING MULTIPLAYER BLOODLETTING SHIV EXHAUST VOID TRICK DRAW COLORLESS`。
  ⚠️ **海克斯标签还决定三选一加权**：玩家已拥有的同标签海克斯越多，同标签候选权重越高
  ⇒ 自造 `MANOSABA_LIN` 标签会让我们的符文**只在自己人之间互相加权**，这正是我们要的。
- **自造键的落地位置**：`ManosabaLin/localization/{zhs,eng,jpn,kor,rus}/relic_collection.json`。
  ⚠️ 游戏**只合并与原版同名的本地化表** ⇒ **文件名必须是 `relic_collection.json`**；语言目录用游戏语言
  代码；其余语言缺失时**回退 `eng`**。键不存在**不报错**，界面直接显示键名（看到键名 = 本地化没生效）。
- 新表按 `manosaba-lin-localization` 技能：zhs 先定稿 → 同步 4 语言 → `json.loads` 校验 + CRLF 计数；
  新表**无 BOM、LF、2 空格缩进、末尾换行**（对齐 `static_hover_tips.json`）。
- 符文**自身**的名称/描述/风味仍走 `relics` 表（`MANOSABA_LIN_RELIC_<SNAKE>.{title,description,flavor}`）
  —— 与海克斯无关。

### 2.6 排障与联机

- 日志前缀固定为 **`[HextechRunes][ExternalContent]`**（对方侧）；我们自己的日志带
  `[ManosabaLin][Hextech]`，两边都要能一眼对上。
- 符文没出现时的自查顺序：① 配置菜单里是否已开启；② 品级 / 标志是否把它排除在当前幕之外；
  ③ `Availability` 是否返回了 `true`；④ 我们的注册是否真的发生在**注册窗口关闭之前**。
- **联机**：两端模组列表必须一致，**包括海克斯的版本**；`isAvailableForPlayer` 与
  `RegisterExtraActProvider` 的委托只允许依赖同步状态。
- ⚠️ 新增/改名/删除 `[SavedProperty]` 会改变存档同步的字段布局 ⇒ 新旧版本之间**无法联机**。
  我们软依赖路线下不给符文加持久字段，天然规避。

### 2.7 参数约定（我方取值）

| 参数 | 取值 | 理由 |
| --- | --- | --- |
| 符文基类 | `ManosabaRelicTemplate`（已 `: RelicModel`） | 指南只要求 `RelicModel` 子类 |
| `Rarity` | `RelicRarity.Starter`（**强制**） | 只有 Starter 能保证原版其他抽取路径抽不到外部符文 |
| `rarity` 品级 | Silver / Gold / Prismatic | 照海克斯三档；**不接受数字** |
| `flags` | 语义上**不要**用 `Disabled`。要「图鉴可见但不进随机选择」→ `SelectionExcluded`；「第一幕不出」→ `FirstActExcluded`；「第三幕不出」→ `ThirdActExcluded` | 对应 `PlayerRuneMetadataCatalog` 的 `IsVisible / IsSelectable / IsConfigurable` |
| `characterPool` | **`null`（通用）** | 对方枚举只有原版 5 个角色，**没有 mod 角色**（见 §5-1） |
| `tagKey` | `"MANOSABA_LIN"`（自造） | 默认 `COMPREHENSIVE` 会和对方「综合」符文挤同一个加权/计数桶 |
| `assetModId` | `"ManosabaLin"` | 配置菜单「来源」显示；**建议填** |
| 遗物池（我方） | `LinRelicPool`（`[RegisterSharedRelicPool]`） | 服务「没装海克斯时也能玩」那条线 |

---

## 3. 验证清单

1. **编译**：`dotnet -t:Compile` 0 错误；⚠️ 产物 DLL **不得**出现对 `HextechRunes` 的编译期引用
   （反射 + `AppDomain` 都是运行时的；这本身就是「没硬引用」的验收）。
2. **未装分支**（无头 harness 天然没有海克斯）：锁死「`HextechCompat.IsReady == false` ⇒
   注册整体跳过、无异常、我方符文的**离线行为**（`LinRelicPool` 那条线）完全不变」。
3. **装了分支**（本机已装）：
   ① 图鉴里出现「海克斯」分类且我方符文在列；
   ② 符文三选一 / 发放流程能抽到我方符文，卡片下方三枚小标签（品级 / 来源 / 标签）正确；
   ③ 配置菜单里能看到我们的分组小标题，且单独开关生效；
   ④ 图标是我方图，**不是** `missing_power.png`；
   ⑤ **原版遗物奖励不出现我方符文**（靠 `Starter` 兜底，必须实测）；
   ⑥ 海克斯「复视」不复制我方符文（指南已声明，回归确认即可）。
4. **启动自检**：把「程序集检测到没有 / `ApiVersion` 多少 / 每个方法的解析结果 / 每个符文的注册结果」
   全部打日志；⚠️ **静默跳过是最贵的坑**，任何一步失败都必须显式报出来。
5. **时序**：故意把注册挪到「晚于游戏初始化」的位置跑一次，确认会抛
   `InvalidOperationException` 而不是**半登记**（这验证我们真的卡在窗口内）。
6. 产物核验：`ilspycmd -t` 看注册桥类型在导出 DLL 里；⚠️ `grep -a` 对 .NET DLL **永远 0 命中**（UTF-16）。
7. ⚠️ 本机游戏目录 DLL 可能被并发会话覆盖 ⇒ 跑测试/发布前先 `cp` 备份、跑完 `cp -f` 还原。

### 2.8 6 个联动遗物：已落地（2026-09-28，`-t:Compile` 0 错误）

> ⚠️ **本节记录的是「离线那条线」** —— `LinRelicPool` 里的普通 Starter 遗物。
> §2.2 / §2.3 / §2.4 的**海克斯注册桥仍未开工**，所以现在不论装不装海克斯，这 6 个遗物都只有
> 我方普通遗物这一份身份，还没有「海克斯符文」的第二身份。

**共享遗物池**：`ManosabaLinCode/Characters/Common/LinRelicPool.cs`
（`[RegisterSharedRelicPool]` + `TypeListRelicPoolModel`，同构先例 `LinCardPool`）
⇒ 进 `ModelDb.AllSharedRelicPools`，成为 5 个角色池之外的「Lin 池」。
⚠️ 只被 `[RegisterRelic]` 注入的池**不会**自动进 `ModelDb.AllRelicPools`，所以必须用
`[RegisterSharedRelicPool]` 而不是裸 `TypeListRelicPoolModel`。

**6 个遗物**（全部 `ManosabaRelicTemplate` + `[RegisterRelic(typeof(LinRelicPool))]` + `Rarity => RelicRarity.Starter`；
目录 `ManosabaLinCode/Characters/Common/LinRelics/`）：

| # | 类 | 效果 | 关键落点 |
| --- | --- | --- | --- |
| 1 | `HextechDeckCleanser` | 每回合开始可把四堆里任意张牌扔进弃牌堆 | `AfterPlayerTurnStart` + `CardSelectCmd.FromSimpleGrid`（自带同步） |
| 2 | `HextechEmotionOverflow` | 球位满时按三类分派：**持续型脱离球位、改挂血条下方**（还是那颗球、效果照旧）、反伤型/延迟型**被挤出时立刻激发**（**2026-09-29 定稿，见下行**） | `OrbCmd.Channel` prefix + `AfterOrbEvoked` + `IEmotionOrb` + 挤出标记 + `HangingEmotionOrbs`（`ModHelper.SubscribeForCombatStateHooks`） |
| 3 | `HextechWitchificationCore` | 【魔女化计数】每 +1 就 +10 层【魔女化】；回合开始 >300 降到 100，削减层数 25:1 转计数；**计数还由「保留」自动累积**（见下） | `Witchification.AddWitchificationCount` 回调 + `AfterPlayerTurnStart` + `Witchification` 的保留三件套 |
| 4 | `HextechTransmigrationEcho` | 打出【轮回】牌改为自动打抽牌堆任意 2 张【轮回】 | `TransmigrationSingleton.AfterCardPlayed` prefix 接管 |
| 5 | `HextechPerjuryPayment` | 能量不足时用 3×缺口【伪证】顶替；1 层【正义】→4 层【伪证】 | `HasEnoughResourcesFor` postfix + `SpendResources` prefix |
| 6 | `HextechJusticeRegen` | 回合结束【正义】>【再生】则补齐【再生】；【正义】不再回血 | `AfterSideTurnEnd` + 拦 `JusticePower.AfterSideTurnEnd` 的 Heal |

> ⚠️ **离线时这 6 个遗物「局内拿不到」是刻意的，不是 bug（用户 2026-09-29 裁定「跟随海克斯，不用你管」）**
> —— **不要**为了让它们能被抽到去改 `Rarity`。完整链路（`RelicGrabBag.Populate`）：
> ```csharp
> list = ModelDb.RelicPool<SharedRelicPool>().GetUnlockedRelics(...);   // 只认**原版** SharedRelicPool
> list.AddRange(player.Character.RelicPool.GetUnlockedRelics(...));     // + 当前角色的池（LinRelicPool 不在其中）
> list.RemoveAll(r => !_rarities.Contains(r.Rarity));                   // _rarities = Common/Uncommon/Rare/Shop
> ```
> 两道闸：`LinRelicPool` 不在抓取袋的两个来源里；且 `Rarity` 必须 Starter ⇒ 必然被 `RemoveAll` 丢掉
> （`RelicFactory.RollRarity` 也只掷 Common/Uncommon/Rare）。
> ⇒ **获取渠道由海克斯符文系统负责**（装了才发），我方这条离线身份只保证「拆了海克斯也不报错」。

**⚠️ 持续型情绪「脱离球位、挂到血条下方」的实现（2026-09-29 定稿；推翻 09-28 晚的「能力接管」版，
也推翻 09-29 早些时候「不挤出、球留在球位」的折中版）**：

用户两轮裁定原话：「原本…效果实现**就不需要能力**…**卡牌挂在血条下面不就等于充能球换个地方挂着**」
→「**脱离默认球位**，改为挂在血条下面」。
⇒ **绝不允许**再造能力（Power）来接管效果，也**不允许**把效果逻辑搬到别处重写一遍。
正确做法 = **让那颗球真的离开球位，但仍然由它自己继续生效**：

- ⭐ **关键引擎出口**：`ModHelper.SubscribeForCombatStateHooks(id, CombatHookSubscriptionDelegate)`。
  `CombatState.IterateHookListeners()` 最后会 `foreach (var m in ModHelper.IterateAllCombatStateSubscribers(this)) yield return m;`
  ⇒ 模组可以把任意 `AbstractModel` 注册成该战斗的钩子监听者。
  **所以「换个地方挂着的还是那颗球、效果一点没变」，不需要 Power、不需要把逻辑搬到卡组件上。**
- 落地（`HextechOrbOverflowPatch`，`OrbCmd.Channel` Prefix，球位已满且队首是持续型情绪球且该玩家有遗物 2）：
  1. `queue.Remove(front)` —— 从模型队列摘下 ⇒ 后面 `Orbs.Count >= Capacity` 不成立 ⇒ **不会 EvokeNext**；
  2. `OrbManager.EvokeOrbAnim(front)` —— 视觉上离场（顺带把空位补回队尾，保证接下来 `AddOrbAnim` 能找到空位）；
  3. `HangingEmotionOrbs.Hang(player, front)` —— 登记成战斗钩子监听者 + 通知 UI。
- **下个玩家回合开始**：`HextechEmotionOverflow.AfterPlayerTurnStart` ⇒ `HangingEmotionOrbs.Release(player)`
  （逐个 `orb.RemoveInternal()`）—— 与球体自身「到下回合开始消散」对齐。
- 显示：`EmotionHangDisplay : Control`，由 `EmotionHangDisplayPatch`（`NCreature._Ready` postfix）挂到每个玩家生物上，
  每帧贴在 `%HealthBar`（`NCreatureStateDisplay`）→ `%HealthBar`（`NHealthBar`）→ `HpBarContainer` 正下方。
  卡面渲染照抄 `EmotionOrbVisualPatch`（`NCard.Create` + `UpdateVisuals(PileType.None, CardPreviewMode.Normal)` + scale 0.5 + 去掉文字层），
  悬浮照抄 `YalisalinFireColorCounter`（透明 `ColorRect` 当靶子 + `NHoverTipSet.CreateAndShow(HoverTipFactory.FromCard(card))`）。
- ⚠️ `HangingEmotionOrbs.Iterate` **必须立刻物化成列表**：钩子派发过程中可能有人 `Release` 改字典，
  惰性迭代字典会抛 `InvalidOperationException`。
- ⚠️ 旧版「`OrbCmd.AddSlots(+1)` 多开一格 + 之后归还」那套记账（`NoteExtraSlot` / `TryReturnExtraSlot` / `ExtraSlots`）**已全部删除**
  —— 用户要的是脱离球位，不是占着球位不让出去。
- ⚠️ `EmotionElationOrb` 本体没有钩子（效果在卡 `OnPlay` 与 `Loseengry` 里），球只是「还有雀跃」的标记；
  它离开球位后 `Loseengry.RemoveAndTrigger` 的 `hasOrb` 判定必须**把挂着的雀跃球也算上** ⇒
  改成 `orbs.Any(...) || HangingEmotionOrbs.HasHanging(Owner.Player, o => o is EmotionElationOrb)`。

**遗物 3 的「计数来源」**：`Witchification` 组件照搬 `RetainCounterComponent` 范式
（`OnAttach` / `BeforeSideTurnEndPostfix` 续期 `GiveSingleTurnRetain()`，`AfterPlayerTurnStartEarlyPostfix`
在 `PileType.Hand` 里 `AddWitchificationCount(1)`）⇒ 每回合保留一次就 +1 计数。
⚠️ `AddWitchificationCount(amount, grantWitchification)` 的 `false` 分支**只累加、不回调**，
遗物 3 削层后必须传 `false`（否则 400→100 会立刻被回调涨回 220）。

**Patch 文件**：`ManosabaLinCode/Patches/HextechRelicPatches.cs`，5 个 patch 类。
⚠️ **全部打的是引擎方法或我方自己的类型**（`OrbCmd.Channel` / `PlayerCombatState.HasEnoughResourcesFor` /
`CardModel.SpendResources` / `TransmigrationSingleton`（我方）/ `JusticePower`（我方）），
**没有一处碰海克斯的 `internal` 类**（§0-1）。

**本地化**：5 语言 `relics.json` 各追加 **19** 键（6 组 `title`/`description`/`flavor` = 18，
遗物 1 另需 `selectionScreenPrompt`）；`powers.json` **无新增**
（09-28 晚为「持续型能力」加的 21 键已于 09-29 全数撤回 —— 持续型不需要能力）。

**⚠️ 与原始描述的偏差**（用户 2026-09-28 已裁定 ①②⑤，其余待定）：

1. ✅ **遗物 2「厌恶 / 骇厌立刻执行一次」的伤害量来源（已裁定，2026-09-28 晚修正）**：
   原描述没给伤害量从哪来。用户裁定 = **直接用敌人当前意图里的攻击总伤害**。
   ⚠️ **目标不是「敌人打自己」，而是「敌人打激发者（你）一次」**（用户 2026-09-28 晚明确纠正，我原先按「打它自己」实现是错的）。
   流程 = 遍历**所有正打算攻击**的敌人 → 各用自己的攻击**打你**一次 → 随即结算该情绪的反伤
   （厌恶：等量打回该敌人；骇厌：敌方全体吃等量伤害 + 抽 1）→ 最后**回复等量生命**（等于你这次总共挨的伤害）。
   落点照搬我方既有卡 `AnanlinTakeTheHitForTeammate`：`move.Intents` 里 `AttackIntent.GetTotalDamage(...)` 取值
   → `DamageCmd.Attack(...).FromMonster(monster).Targeting(Owner.Creature)` → 手动补 `MoveStateMachine.OnMovePerformed`
   + `History.MonsterPerformedMove`，**全程不调** `PerformMove`/`SetMoveImmediate` ⇒ **敌人原本意图不变**。
   ⚠️ 没有任何敌人「正打算攻击」时无伤害量可取 ⇒ 本项不生效。
   ⚠️ 这是**真实承伤**（受格挡/能力影响）⇒ 低血时主动挤球可能致死；好处是触发时机完全由玩家掌握（只有主动 Channel 满位球才走到这里）。
   ⚠️ 球在 `Hook.AfterOrbEvoked` 之前就已经 `OrbQueue.Remove`（`OrbCmd.Evoke` 先 Remove 后播 hook），
   而 `CombatState.IterateHookListeners` 是从 `player.PlayerCombatState.OrbQueue.Orbs` 枚举球的
   ⇒ 球自己的 `AfterDamageReceived` **不会再跑**，不会重复结算反伤。
2. ✅ **遗物 3「10 倍」（已裁定）** = **【魔女化计数】每 +1 ⇒ +10 层【魔女化】**。
   **不是**「挂上组件那一刻按当前【魔女化】层数 × 10 放大一次」（旧的 `Witchification.OnAttach` 回调已删）。
   现由 `Witchification.AddWitchificationCount` 回调 `HextechWitchificationCore.OnWitchificationCountGained`。
3. 遗物 3 的计数**刻意不加 `[ComponentState]`**（不持久化）—— `Witchification` 是既有序列化类型，
   加字段会改 `[SavedProperty]` 布局、破坏联机与旧存档。
4. ✅ **遗物 5「完全没有【伪证】时不生效」（已裁定）** = **只要有【正义】就照样生效**；
   兑换后**多出来的层数原样留成【伪证】**。
   做法：不再要求 `PerjuryPower` 已存在 —— 玩家一层【伪证】都没有时，用同步 API
   `ModelDb.Power<PerjuryPower>().ToMutable()` + `ApplyInternal(creature, remaining, silent: true)` 现场建一个
   （等价于 `PowerCmd.Apply` 的同步内核：`SetAmount` + `Owner.ApplyPowerInternal`；后者才是「注册为战斗监听器」的那一步）。
   ⚠️ 不能在 prefix 里 `await PowerCmd.Apply`（同步扣费前缀等不了异步），所以必须走这条同步路径。
   `CanCoverEnergyDeficit` 早就已经把【正义】折算进来了，所以 `HasEnoughResourcesFor` 那侧无需改动 —— 之前只是**消费侧**走不到。
   顺带：`ApplyInternal` 与 `SetAmount` 都**不触发** `AfterPowerAmountChanged` / `AfterApplied`
   ⇒ 兑换出的多余【伪证】**不会被自动转回【正义】**，正好符合「多余的正常保存为伪证」。
5. ✅ **遗物 6 时序（已裁定）** = 补出的【再生】**本回合就回血**。
   做法：把补层从 `AfterSideTurnEnd` 提到 **`BeforeSideTurnEndVeryEarly`** ——
   它是 `Hook.BeforeSideTurnEnd` 内部的第一个子阶段（`Hook.cs`：`VeryEarly` → `Early` → `BeforeSideTurnEnd` 各遍历一遍监听器），
   因而排在 `RegenPower.BeforeSideTurnEndEarly`（结算回血 + 递减）**之前**。
   附带好处：此刻 `JusticePower.Amount` 还没被它自己的 `AfterSideTurnEnd` 递减 ⇒ 读到的是「回合结束时的当前层数」，
   与监听器顺序无关（旧实现挂在 `AfterSideTurnEnd`，与 `JusticePower` 同阶段、顺序未定义，可能读到已递减的值）。
6. ✅ **遗物 2 整套重做（用户 2026-09-28 晚「现在就整套重做」+ 09-29「不要能力 / 脱离默认球位」三次裁定）** —— 14 颗情绪球按三类重排：
   - **持续型 7 种**（悲伤 / 愤怒 / 快乐 / 怅然 / 雀跃 / 好奇 / 友谊）⇒ **脱离默认球位**：球从 `OrbQueue` 里被摘下、
     改**挂在血条下方**（`EmotionHangDisplay`），由**球自身钩子**继续生效，到下个玩家回合开始释放。
     ⚠️ **不是**「效果搬进血条下方的能力」（09-28 晚那版 7 个 Power 已全部删除）；
     ⚠️ **也不是**「球留在球位不让出去」（09-29 早些时候那版 `OrbCmd.AddSlots(+1)` 已删除）；
     挂着的**就是同一颗球**，靠 `ModHelper.SubscribeForCombatStateHooks` 继续收钩子。
   - **反伤型 2 种**（厌恶 / 骇厌）⇒ 见上方第 1 条（两者一律「所有正打算攻击的敌人各打你一次」，不再区分单/全体触发者）。
   - **延迟型 5 种**（恐惧 / 惊讶 / 恼惧 / 凄惶 / 无助）⇒ 「回合结束 / 下回合开始」才结算的收益**立刻结算一次**。
     旧实现只覆盖恐惧 + 惊讶，现补上恼惧（先掉 3 血、自己和队友拿到剩余等量格挡）、凄惶（先回半格挡血、再拿等量格挡）、
     无助（把负敏捷换成等量荆棘 + 给随机敌人等量负力量）。
   - 骇厌额外把「下回合开始随机造成 = 手牌数的 1 点伤害」也**立刻结算一次**。
7. ✅ **遗物 2 触发条件（用户明确）**：只有玩家**主动获得充能球、且把球挤出去**才触发 ⇒ 靠 `OrbCmd.Channel` 的
   Prefix 打标记（`HextechOrbOverflowPatch`）+ `AfterOrbEvoked` 取走标记，正常消散（`EvokeNext`）不触发。
   ⚠️ 持续型是唯一的例外：它**不会被挤掉**（在 `OrbCmd.Channel` 的 Prefix 里就被摘下来挂着了），
   因此 `AfterOrbEvoked` 里对持续型一律不结算（走到那里只可能是它自己在回合开始消散）。
8. ✅ **雀跃的循环接续**：雀跃球本体没有钩子，它的效果写在卡 `OnPlay`（获得 = 能量上限的能量 + 挂 `Loseengry`）和
   `Loseengry` 内部；球只是「还有雀跃」的**标记**。⚠️「挂到血条下方」方案下球已**不在** `OrbQueue` ⇒
   `Loseengry.RemoveAndTrigger` 的 `hasOrb` 必须**额外**查一份：
   `orbs.Any(o => o is EmotionElationOrb) || HangingEmotionOrbs.HasHanging(Owner.Player, o => o is EmotionElationOrb)`
   （09-28 晚为「纯标记能力」加的 `hasElation` 分支与 `using` 已回退，这次只加这一行）。

**愤怒的后半段（2026-09-29 补齐，用户「愤怒把效果改成本地化这样」）**：
`EmotionAngerOrb` 原本只有 `ModifyDamageMultiplicative => 2m`，**漏了**本地化里明写的后半句
（`MANOSABA_LIN_CARD_EMOTION_ANGER.description` / `MANOSABA_LIN_ORB_EMOTION_ANGER_ORB.description`）：
「造成双倍伤害，**每造成20伤害对随机友方造成1点伤害**」。
现补在 `AfterDamageGiven`：只统计**打敌人**的伤害、取 `DamageResult.TotalDamage`（= 挡掉的 + 实打实的，
就是玩家看到的被翻倍后的那串数字），每满 20 点用 `Owner.RunState.Rng.CombatTargets` 选一个存活友方打 1 点
（`ValueProp.Unpowered`）；`_resolvingBacklash` 重入保护保证反噬自己造成的伤害不再被统计。

**遗物 3「计数是拆分」不是「每张各一份」（用户 2026-09-29 裁定）**：
`HextechWitchificationCore.AfterPlayerTurnStart` 里那 `count` 点计数是**在带组件的卡之间瓜分**（合计恰为 `count`），
方式按用户 2026-09-28 的原话「将 12 点计数**随机分给**所有牌中魔女化组件卡」⇒ 逐点用
`Owner.RunState.Rng.CombatCardSelection` 随机指派（只有 1 张卡时直接全给它）。

### 2.9 注册桥：已落地（2026-09-28，`-t:Compile` 0 错误）

**新增 3 个文件**（`ManosabaLinCode/Compat/Hextech/`）：

| 文件 | 职责 |
| --- | --- |
| `HextechCompat.cs` | 门控。按**程序集名**找 `HextechRunes` → 解析 `HextechRunes.HextechRunesInterop` → 读 `ApiVersion`；没找到就订阅 `AssemblyLoad`（载入后自解绑再注册）；`ResolveInteropMethod(name, paramTypes)` 供**逐成员**能力探测 |
| `HextechRuneCatalog.cs` | 纯数据。6 条 `HextechRuneSpec`（字段就是 `RegisterPlayerRune` 入参）+ `Validate()` 自检（具体类型 / `RelicModel` 子类 / 品级名合法） |
| `HextechRuneRegistrar.cs` | 反射桥。`RegisterPlayerRune` × 6 → `SetPlayerRunePoolLabel` × 6 → `RegisterConfigSectionTitle` × 1；幂等，**逐个 try/catch**（`TargetInvocationException.InnerException` 解包），绝不因单个符文打断 mod 初始化 |

**接线**：`MainFile.cs` 在 `GuardOneRewardRegistrar.Register()` 之后调 `HextechCompat.Initialize()`（紧邻 `harmony.PatchAll()`），
注释里点明「必须在模组初始化阶段调用，要赶在共享遗物池首次枚举之前」。

**与 §2.2 草图的差异**：门控的程序集属性改名 `Assembly` → **`TargetAssembly`**
（`Assembly` 会与 `System.Reflection.Assembly` 类型名在同一作用域冲突）。

**数据表（2026-09-28 用户裁定可用性：3 张仅雪莉琳 + 3 张仅希罗，无「通用」；2026-09-29 用户裁定品级：全部棱彩）**

| # | 遗物 | 海克斯**品级** | `isAvailableForPlayer` | 依据 |
| --- | --- | --- | --- | --- |
| 1 | `HextechDeckCleanser` | Prismatic | **仅雪莉琳** | 用户裁定（我原按「不依赖角色专属机制」定通用，被否） |
| 2 | `HextechEmotionOverflow` | Prismatic | **仅雪莉琳** | 情绪球只存在于雪莉琳（其余角色 0 处引用） |
| 3 | `HextechWitchificationCore` | Prismatic | **仅雪莉琳** | 用户裁定（【魔女化】虽 5 个角色都有，但本遗物绑定雪莉琳那套组件链） |
| 4 | `HextechTransmigrationEcho` | Prismatic | **仅希罗** | 【轮回】只在希罗卡池（25 处引用，其余角色 0） |
| 5 | `HextechPerjuryPayment` | Prismatic | **仅希罗** | 【伪证】/【正义】只在希罗卡池（27 处） |
| 6 | `HextechJusticeRegen` | Prismatic | **仅希罗** | 同上 |

- ✅ **品级：6 条一律 `"Prismatic"`（用户 2026-09-29「这六个遗物都是棱彩品质」）**，
  落地为常量 `HextechRuneCatalog.RuneRarity`（原先 1/4/5 = Gold、6 = Silver）。
  ⚠️ **这个「棱彩」是海克斯自己的三档品级（字符串），不是原版稀有度** ——
  原版 `RelicRarity` 枚举里**没有 Prismatic**（`None/Starter/Common/Uncommon/Rare/Shop/Event/Ancient`），
  而 `Starter` 是海克斯《符文类的要求》写死的（保证原版别的抽取路径抽不到）⇒ **两个维度都别串**。
- ⚠️ **#1 / #3 定成「仅雪莉琳」是用户点名裁定的**，不要再按「机制通用性」自行放宽成 `null`。
  这一条也是**授权范围铁律**的实例：数据表这类「我给建议、用户裁决」的字段，以用户裁决为准。
- 品级只是掉率档位，**随时可改**（现在 6 条共用常量 `HextechRuneCatalog.RuneRarity`，改一处即可）。
- **用 `isAvailableForPlayer` 按角色收窄，而不是 `characterPool`** —— 对方枚举里只有原版 5 个角色、没有本 mod 角色（§5-1）。
  副作用是**把给作者的 R1 请求降到了最低优先级**：我们已经有可行的绕法，只是界面分组体现不出来。
- 委托是**确定性纯函数**（只读 `player.Character`）⇒ 满足 §0-9 的联机要求。

**界面文字**：新增本地化表 `ManosabaLin/localization/{zhs,eng,jpn,kor,rus}/relic_collection.json`，
各 3 键：`HEXTECH_POOL.MANOSABA_LIN`（来源标签）/ `HEXTECH_TAG.MANOSABA_LIN`（海克斯标签）/
`MANOSABA_LIN_HEXTECH_SECTION`（配置菜单小标题）。
⚠️ 表名必须是 `relic_collection`（游戏只合并与原版同名的表）；新表按技能规范为**无 BOM + LF + 2 空格 + 末尾换行**。

**⚠️ 本机无法验证「装了」分支**：2026-09-28 检查 `mods/` 里**没有 `HextechRunes`**（只有 ManosabaLin / MinionLib / RitsuLib / TestTheSpire）。
`.local/hex_src/` 那份反编译也**早于本指南**（`HextechRunesInterop` 只有 `RegisterExtraActProvider`，
`RegisterPlayerRune` 还在硬依赖的 `HextechRunesApi` 里，且要求继承 `HextechRelicBase`）。
⇒ 本次实现**只保证**：未装时零副作用、装了且版本够新时按指南注册、版本旧/能力缺失时**逐项降级并打日志**（不会崩）。

**验收（已实测）**：产物 DLL 里 `HextechRunes` 在 **`#Strings`(UTF-8) 命中 0 次**、只在 **`#US`(UTF-16) 命中 12 次**
⇒ 「零编译期引用」成立（§3-1 的判据）。

---

## 4. 已否决路线（记录理由，避免回头重走）

| 路线 | 否决理由 |
| --- | --- |
| **反射 + Harmony 窄 patch `HextechCatalog` 的 ~18 个静态方法**（本文档上一版的主方案） | ⛔ **官方指南明令禁止**（§0-1）。这条路线从「唯一可行」变成「违规」。 |
| 建可选子程序集引用 `HextechRunes.dll`，符文继承 `HextechRelicBase`，调官方 `RegisterPlayerRune<T>` | ① 仍是**硬引用**；② 版本绑定，对方改 API 就要重编；③ 对方没装时对加载时序要求苛刻（引擎与 `HextechCatalog` 都会 `assembly.GetTypes()`，基类缺失即 `TypeLoadException`）。**软依赖非泛型重载已完全覆盖需求，不需要它。** |
| 用 `[ModInterop("HextechRunes", …)]` 做主要接入 | 与指南「按程序集名检测、不要按 manifest id」冲突（`ModInteropAttributes.cs:18` 按 manifest id 解析）。仅作为可选补充。 |
| 我们主动 `ModHelper.AddModelToPool(typeof(SharedRelicPool), 我方符文)` | 海克斯注册时会自己加；我们重复加只会让符文更早暴露在原版其他抽取路径下。 |

---

## 5. 与作者的落差（→ 同步进配对文件）

1. **`characterPool` 没有 mod 角色。** 对方枚举只有 `Ironclad / Silent / Regent / Defect / Necrobinder`，
   我们的 5 个角色（艾玛 / 亚里沙 / 希罗 / 杏奈 / 雪莉）**只能落进「通用」**。
   绕过：`isAvailableForPlayer` 里按 `player.Character` 过滤（能实现效果，但**界面分组体现不出来**）。
   ⇒ 已写进配对文件的需求。
2. **软依赖面（`ApiVersion == 1`）只有 4 个成员**：
   `RegisterPlayerRune` / `SetPlayerRunePoolLabel` / `RegisterConfigSectionTitle` / `RegisterExtraActProvider`。
   我们想在软依赖下做**锻造器 / 事件遗物 / 附魔图标 / `RelicBundleGrantHelper.GrantRelics`** 都**不够用**，
   只能硬依赖。⇒ 已写进配对文件的需求。
3. **`HextechNaturalRelicPoolHooks` 的过滤是类型判定**（`!(relic is HextechRelicBase)`）⇒ 外部符文
   （不继承基类）**不会**被它滤掉，官方靠「必须 `Starter`」兜底。若我们不慎给某个符文标了非 Starter，
   它就会被**原版自然遗物奖励**随机发出。⇒ 我们的硬约束：**所有海克斯符文一律 `RelicRarity.Starter`**。
4. **待实测**：
   ① 我方符文同时进 `LinRelicPool`（我方登记）与 `SharedRelicPool`（海克斯登记）时的**去重表现**
   （图鉴/遗物列表会不会出现两份）；
   ② `LocString("relic_collection", <缺失键>)` 的确切表现（指南说「显示键名、不报错」，实测确认）；
   ③ 重复注册时对方的警告文案（用来核对我们的幂等保护）。
5. **待确认**：`ApiVersion` 是否随 `HextechRunesInterop` 新增能力而递增（指南承诺了，但当前仍是 1）；
   将来若它变 ≥ 2，我们要用「按需降级」而不是「整体拒绝」。
