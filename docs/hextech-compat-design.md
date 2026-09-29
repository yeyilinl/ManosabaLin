# 与「海克斯」模组联动 —— ManosabaLin 侧怎么落地

> 目标：把**联动内容（符文 / 锻炉 / 遗物 / 卡牌…）全部放在 ManosabaLin 里**，
> 海克斯被动地「识别 + 载入」这些 entry；**不硬引用海克斯的 dll**，它没装时整套联动安静地不出现。
>
> 参考对象：`YuWan886/Sts2-YuWanCard`（`YuWanCardCode/Core/Interop` + `Integrations/Hextech`）。
> 那份做法是「自研 interop + 反射 patch」，其中 **interop 那层可以直接换成 RitsuLib 的 `[ModInterop]`**，
> 省掉自己写 Harmony transpiler。
>
> ⚠️ **本文档写于「还不知道对方有官方注册 API」的时候**，其中 §2.4 / §8 关于「必须窄 patch 才能并池」的
> 结论**已作废**：海克斯作者已发布《外部模组对接指南》，**软依赖 `HextechRunesInterop` 就能注册符文，
> 并明确禁止对 `HextechCatalog` 这类 `internal` 类打 Harmony 补丁**。
> ⇒ 海克斯的现状请看 `docs/hextech-compat-ours.md`（我方施工单，**零 patch**）；本文档保留为
> **跨 mod 联动的通用套路参考**（L0~L4 分层、四条铁律、失败降级矩阵对任何第三方 mod 都适用）。
> 文中的 `[ModInterop]` 硬规则、`entry` 字符串交换原则仍然有效。

---

## 0. 结论速览

> ⚠️ 下表 ③④ 两行已被官方对接指南取代（见文首说明）；① ② ⑤ 仍然成立。

| 步骤 | 做什么 | 落在哪 |
| --- | --- | --- |
| ① 检测 | **优先按程序集名**（海克斯本体按游戏版本加载不同变体 DLL，manifest id 不可靠）；没装就整块跳过 | **已有**：`Compat/Core/OptionalModCompatRegistry.IsModLoaded`（按 manifest id，**海克斯这条线不能用它**，需另写按程序集名的检测） |
| ② 门控 | 未装海克斯时，联动内容不进任何池、不出现在图鉴/奖励里 | **半成品**：`Compat/Core/CompatContentGate`（3 个钩子只有 1 个接上了，见 §2.2） |
| ③ 代理 | ~~`[ModInterop]` 空壳~~ ⇒ 官方 `HextechRunesInterop` 是 public，可直接**反射**调（不推荐 `[ModInterop]`：它按 manifest id 解析目标程序集） | **新写**：`Compat/Hextech/HextechRuneRegistrar.cs` |
| ④ 并池 | ~~窄 patch~~ ⇒ **不需要 patch**：一次性调 `RegisterPlayerRune(...)` 把元数据交出去，对方注册表接管 | **新写**：同上（`RegisterPlayerRune` + `SetPlayerRunePoolLabel` + `RegisterConfigSectionTitle`） |
| ⑤ 交换 | 只交换 **entry 字符串 / 类型名**，不传类型对象 | **已有**：`Compat/Core/OptionalModModelResolver.TryFindCardByEntry` |

一句话：**内容是「我方登记、我方拥有」；第三方 mod 有公开注册 API 时就直接注册，没有时才退化为
「反射 patch 它的取数方法」——而对方明令禁止 patch 时，只能开 issue。**

---

## 1. 四条铁律

1. **不硬引用**：`.csproj` 里绝不加海克斯 dll 的 `Reference`／`ProjectReference`。
   所有对它的调用都经过 `[ModInterop]` 空壳类或反射。
2. **能力探测优先于版本号**：不要写 `if (hextechVersion >= x)`。用
   `IsReady` / `TryGet…` 这类**运行时探测**，拿不到就走降级分支。
3. **缺失是正常分支**：海克斯没装 ⇒ 联动内容**不登记**（而不是登记了再崩）。
   玩家体验上等价于「这个 mod 没装」，绝不能报错、不能白屏、不能卡存档。
4. **ID 前缀不撞**：联动内容的 entry 一律带 `MANOSABA_LIN_` 前缀（沿用现有本地化键规范）；
   向海克斯只递交**它的 entry 字符串**，不递交我方类型对象。

---

## 2. 五层，逐层对上你项目里已有的东西

### 2.1 L0 检测 —— 已就绪

`ManosabaLinCode/Compat/Core/OptionalModCompatRegistry.cs:17`：

```csharp
public static bool IsModLoaded(string modId) =>
    ModManager.GetLoadedMods().Any(mod =>
        string.Equals(mod.manifest?.id, modId, StringComparison.OrdinalIgnoreCase));
```

包了 `try/catch`（取不到就 `false`），符合铁律 3。**直接复用，不要另起一套。**

⇒ 唯一要补的是一个语义化门面，例如：

```csharp
public static class HextechCompat
{
    public const string ModId = "海克斯的manifest.id";   // ← 待填，见 §8
    public static bool IsLoaded() => OptionalModCompatRegistry.IsModLoaded(ModId);
}
```

### 2.2 L1 门控 —— 现有基建只有一半接上了

`CompatContentGate` 已经预留了三个「这条内容该不该存在」的钩子，但**只有 compendium 那条有调用点**：

| 钩子 | 现状 |
| --- | --- |
| `IsExternalCompatRelic(relic)` | **空壳**，无任何调用点 |
| `IsGameplayRelicAvailable(relic)` | **空壳**，无任何调用点 |
| `IsCompendiumCardVisible(card)` | 已接：`Characters/Ananlin/Cards/AnanlinCardHelpers.cs:325` |

注释里那三行被注掉的 `WindchaserCompat.IsLoaded()` 就是当初设想的用法。
**所以联动开工前先定一个策略，再把三个钩子接上**：

- **策略 A（推荐）：未装就不登记。** 联动内容不进卡池/遗物池/图鉴 —— 干净，但需要
  「注册时判断」，而 `[RegisterCard]` 这类特性是**编译期**的，做不到条件注册。
- **策略 B（推荐 A 做不到时）：登记但不可见/不可获得。** 内容照常注册，靠
  `CompatContentGate` 在**池构建 / 图鉴 / 奖励**三个出口把它滤掉。现有基建就是为 B 准备的。

无论 A/B：**要挑一个统一出口**，否则会出现「图鉴看得到、打不出来」这种半吊子状态。

### 2.3 L2 强类型代理 —— 用 RitsuLib，不要自研 transpiler

前置条件你**已经满足**：`MainFile.cs:60`

```csharp
ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
```

写法（照抄你贴的文档即可，这里只补**实测出来的硬规则**，来自
`.local/external/STS2-RitsuLib/src/Interop/Internal/ModInteropEmitter.cs`）：

```csharp
[ModInterop("海克斯modId", "海克斯.某静态类全名")]
public static class HextechApiInterop
{
    public static bool IsReady => false;              // 缺海克斯时保留这个默认值
    public static IReadOnlyList<string> GetAllXxx() => [];   // 缺海克斯时保留空集

    // 名字/类名对不上时逐成员指定
    [InteropTarget("海克斯.Catalog", "FindById")]
    public static RuneRef Find(string id) => throw new NotSupportedException();

    [InteropTarget("海克斯.TargetMod.Api.Entry")]
    public sealed class RuneRef : InteropClassWrapper   // 实例包装必须继承它
    {
        public RuneRef(string id) { }
        public string DisplayName => "";
        public int GetScore() => 0;
    }
}
```

**必须知道的四条（都是读源码确认的，踩了不会崩、只会静默失效）**：

| 规则 | 源码 | 违反了会怎样 |
| --- | --- | --- |
| 目标 mod **未加载** ⇒ 发射器 `return`，**保留你写的默认体** | `ModInteropEmitter.cs:41` | 门面 `IsReady => false` 成立 —— 这正是铁律 3 想要的 |
| 外层类成员必须 **public + static**（`requireStatic: true`） | 同上 `:47`、`:76` | 该成员被**跳过**，保留默认值 |
| 返回值类型必须与远端**完全相等**（`!=` 比较，不做协变） | 同上 `:210` | 抛 `return type mismatch` → 被 catch 成 **Warn，静默降级** |
| 嵌套包装类必须有 public 构造，且远端有**参数完全匹配**的构造 | 同上 `:102`、`:113` | 抛 `No matching constructor` → 整类 Warn + **跳过后续成员** |

⇒ 因此**必须加一条启动自检**：海克斯装了的话，检查 RitsuLib 日志里有没有
`[ModInterop] Generated interop method …` / `Generated interop type …`；
只有 `Warn` 没 `Generated` = 代理没挂上，此时 `IsReady` 会骗你。
建议在 `HextechCompat` 里加一条自己的 Info 日志，把「已加载 + 代理已生成」打印出来。

> `[AssemblyInterop]` 只在目标是**任意 CLR 程序集**（不是 mod）时才用；类型名里含逗号走它，
> 不含逗号走 `[ModInterop]`。跟海克斯联动用 `[ModInterop]` 就够。

### 2.4 L3 并池 —— ⚠️ 结论已改：**先问「对方有没有公开注册 API」**

**决策顺序（先问前两条，再考虑 patch）**：

1. **对方有公开注册 API 吗？** → 有就直接调。海克斯就有（软依赖 `HextechRunesInterop` 的
   `RegisterPlayerRune` / `SetPlayerRunePoolLabel` / `RegisterConfigSectionTitle`），**零 patch**。
2. **对方明令禁止 patch 它的内部类吗？** → 是就**停下开 issue**，别硬扛。海克斯的对接指南明确写了
   「`HextechCatalog` 等 `internal` 类不属于对外契约，请不要对它们打 Harmony 补丁」。
3. 只有「既没有公开 API、又没禁止 patch」时，才走下面的窄 patch 路线。

**如果你的内容是「我方独立内容」**（新卡、新遗物），任何情况下都不需要 patch：
在海克斯那边没有任何存在感，照常玩。

**只有当内容要进海克斯自己的可选池**，且 1、2 都不成立时，才需要动 patch ——
因为对方的枚举方法是它自己写的，它不会来问你：

```csharp
[HarmonyPatch]
internal static class HextechPoolPatch
{
    // 🔑 对方没装 ⇒ Prepare 返回 false ⇒ Harmony 干净跳过，整类不生效
    private static bool Prepare() => HextechCompat.IsLoaded();

    private static MethodBase? TargetMethod() =>
        AccessTools.Method(HextechApiInterop.ResolveType("海克斯.目录类全名"), "GetAllSelectableXxx");

    [HarmonyPostfix]
    private static void Postfix(ref IEnumerable<XxxType> __result)
    {
        // 只做「并集」，不改对方原有内容；我方 entry 用 MANOSABA_LIN_ 前缀，保证不撞
        __result = __result.Concat(ManosabaHextechEntries.All);
    }
}
```

要点：

- **先核实对方的公开面**：`internal` 的目录类用不了 `[ModInterop]` / `[AssemblyInterop]`（只能代理 public
  类型/成员），所以那条路只剩反射 + postfix —— 而对方的对接指南可能**明文禁止**这么做。**先看文档。**
- **条件化用 `[HarmonyPrepare]`**（`0Harmony.dll` 里确认存在），比 `TargetMethod() => null` 干净：
  后者在 HarmonyX 里的容错不如前者明确。备选：`MainFile.cs:100` 现在是**无条件 `harmony.PatchAll()`**，
  也可以改成「只有 `IsLoaded()` 才 `harmony.CreateClassProcessor(typeof(HextechPoolPatch)).Patch()`」。
- **只并集、不改写**：postfix 只 `Concat`，不要过滤掉对方的东西（否则玩家装了两个 mod 会互相打架）。
- **目标方法名/签名会随对方版本漂**：所以 `TargetMethod()` 走**反射解析**而不是 `[HarmonyPatch(typeof(...))]`
  直写类型 —— 直写就等于硬引用了。项目里的先例：`Patches/SamePlaceTruthSelectionLockPatch.cs:49`。
- 递交给对方的**只要 entry 字符串**；对方那边 `ModelDb` 解析出来是什么就是什么（见 §2.5）。

### 2.5 L4 资源归属与「只交换 entry」

**联动资源全部在 ManosabaLin 侧登记**：卡/遗物/能力照常走 `[RegisterCard]` / `[RegisterRelic]` 特性 +
`MainFile.CreateContentPack`，本地化键沿用 `MANOSABA_LIN_*`。

**跨 mod 只传 entry 字符串**，不要传类型对象 —— 你已经有这一层了：

```csharp
// Compat/Core/OptionalModModelResolver.cs
TryFindCardByEntry("HEXTECH_XXX", out var card)
// 匹配规则：entry 全等 / 以 "-X" 结尾 / 以 "_X" 结尾（大小写不敏感）
```

⇒ 这是「海克斯只做识别和载入」的技术含义：
**海克斯拿到的是字符串 ID，能不能解析、解析成什么由它自己的 `ModelDb` 决定**；
我方不把自己的类型塞进它的集合，双方版本各升各的也不会崩。

---

## 3. 命名与防撞

| 事项 | 规则 |
| --- | --- |
| 我方 entry | `MANOSABA_LIN_*` 前缀（本地化键同规） |
| 海克斯侧引用 ID | 原样透传它的 entry，**不要**加我方前缀 |
| 本地化键 | 沿用 `MANOSABA_LIN_{CARD,POWER,RELIC}_<SNAKE>`；`<SNAKE> = NormalizePublicStem`（数字不拆词） |
| 音效/美术 | 放 `ManosabaLin/` 下自己的路径，别去引用海克斯的 `res://`（它一改名你就静默丢资源） |

---

## 4. 时序与初始化顺序

`MainFile.Initialize()` 现状（`ManosabaLinCode/MainFile.cs`）：

```
:56  BeginModDataRegistration
:60  ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly)   ← interop 的前置，已在
:66  CreateContentPack(...).Apply()                             ← 我方内容登记
:82  ModCardPileRegistry…（雪莉琳案卷牌堆）
:97  GuardOneRewardRegistrar.Register()
:99  Harmony harmony = new(ModId); harmony.PatchAll();          ← 无条件补丁
```

要插的位置：

1. **`HextechCompat`（L0 门面）**：放 `Compat/Hextech/`，**不依赖初始化顺序**（纯静态判断），随取随用。
2. **`HextechInterop` 空壳类**：必须在 `RegisterModAssembly`（:60）之后才有意义 —— 发射器由
   RitsuLib 的 discovery 阶段驱动，我方只需**保证类存在于已注册的程序集里**即可，不用手动调用。
3. **`HextechPoolPatch`**：跟 `PatchAll()`（:100）走；条件靠 `[HarmonyPrepare]`。
4. **门控（L1）**：接在池构建 / 图鉴 / 奖励出口（见 §2.2），**不要**在 `Initialize` 里做一次性判断，
   否则读档、重开一局、换角色时状态不刷新。

---

## 5. 失败 / 降级矩阵

| 情况 | 期望行为 |
| --- | --- |
| 海克斯未装 | 联动内容**不进入任何池/图鉴**；`HextechCompat.IsLoaded()` = false；无日志噪音（最多 Info 一条） |
| 海克斯装了，但**我方的 interop 签名对不上** | RitsuLib 打 `Warn` + 保留默认值 ⇒ 表现为「联动静默不生效」。**必须靠 §2.3 的启动自检发现**，不要靠玩家反馈 |
| 海克斯装了，但**它把目标方法改名/删了** | postfix 挂不上（`Prepare` 通过但 `TargetMethod` 为 null / 类型解析失败）⇒ 走「我方独立内容」路径，不崩 |
| 海克斯装了，且目标方法签名变了 | `AccessTools.Method` 返回 null ⇒ 同上；**不要**为了兼容去写多版本分支（那是维护地狱） |
| 多人联机 | patch 只做**确定性并集**（不引入随机、不读本地状态）；interop 只读状态不写状态 |

---

## 6. 测试与验证

- **无头 harness 里没有海克斯** ⇒ `Compat/Hextech` 的代码路径天然只覆盖「未加载」分支。
  至少加一条用例锁死：`HextechCompat.IsLoaded() == false` 时联动内容不出现在可选池里。
- **代理真的挂上了没有**：不靠 `IsReady`（它缺目标时恒 false 是设计如此），靠
  RitsuLib 日志里有没有 `[ModInterop] Generated …`。
- **`-t:Compile` + 跑测试会把游戏目录 DLL 换成 Debug 版**；别人部署过之后要先
  `cp` 备份、跑完再 `cp -f` 还原（见 `.workbuddy/memory/MEMORY.md` 的「发布与验证」）。
- 产物核验：`ilspycmd -t <类型>` 看 patch/interop 类型真的在导出 DLL 里。

---

## 7. 落地清单（按顺序做）

- [x] ~~拿到海克斯的 `manifest.id`、程序集名、以及「可选池枚举方法」的签名~~ → **已拿到官方对接指南**，
      结论是**不需要 patch**（见文首说明与 `docs/hextech-compat-ours.md`）
- [ ] `Compat/Hextech/HextechCompat.cs`：门面（按**程序集名**检测 + `AssemblyLoad` 订阅 + `ApiVersion` 门槛 + 启动自检日志）
- [ ] `Compat/Hextech/HextechRuneCatalog.cs`：我方符文元数据表（品级 / 标志 / 角色池 / 标签 / 可用性）
- [ ] `Compat/Hextech/HextechRuneRegistrar.cs`：反射调 `RegisterPlayerRune` + 两个界面文字方法
- [ ] 先用只读调用验证「探测 + `ApiVersion` 读到了」（打印一次即可，别急着注册）
- [ ] 决定 L1 策略（A 不登记 / B 登记但不给），把 `CompatContentGate` 另外两个钩子接上
- [ ] ~~`Patches/HextechPoolPatch.cs`~~ → **不做**（对方明令禁止 patch `internal` 类）
- [ ] 我方联动内容与本地化（**zhs 是源语言，改完同步 eng/jpn/kor/rus**；
      海克斯界面文案进 `relic_collection.json`，符文自身文案进 `relics.json`）
- [ ] 用例：未加载分支 + 幂等（重复 `Initialize()` 不重复注册）
- [ ] `unset STS2_SKIP_PCK_EXPORT && dotnet publish`，核 md5 / PCK / `ilspycmd`

---

## 8. 我还需要你给的输入（缺了就只能写到这里）

1. ~~**海克斯的 mod id + 程序集名**~~ → **程序集名 `HextechRunes`**（按程序集名检测，不按 manifest id）。
2. ~~**它有没有公开的「注册/扩展」API**~~ → **有**：软依赖 `HextechRunesInterop`（`ApiVersion == 1`，
   含 `RegisterPlayerRune` / `SetPlayerRunePoolLabel` / `RegisterConfigSectionTitle` /
   `RegisterExtraActProvider`）；硬依赖 `HextechRunesApi` 覆盖面更全但要引用 dll。
   ⇒ **走 §2.3 代理/反射直呼它，完全不动 patch，最干净。**
3. **联动载体是什么**：海克斯符文（`Starter` 遗物）。（决定元数据表的字段与 `CompatContentGate` 要门控哪些内容。）
