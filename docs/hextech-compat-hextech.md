# 给海克斯符文作者的需求单（可直接当 Issue 发）

> **配对文件**：`docs/hextech-compat-ours.md`（我方施工单）。
> **提交对象**：HextechRunes（`manifest id = "HextechRunes"`，程序集名 `HextechRunes`，作者 `Natsuki`）。
> **本文档的事实基准**：作者发布的《外部模组对接指南》，以及
> `…/workshop/content/2868840/3747501308/lib/0.111.0/HextechRunes.dll` 的反编译。
>
> ✅ **先说好消息**：指南里的 **软依赖 `HextechRunesInterop`** 已经把我们上次提的 4 条需求**基本全解决了**
> —— 不需要继承 `HextechRelicBase`、不需要硬引用 dll、图标自己给、可用性过滤收 `Func<Player,bool>`。
> **本文档原 R1/R2#3/R2#4/R4 全部撤回**，只保留指南尚未覆盖的落差。

---

## 标题

**Feature request: 让软依赖 `HextechRunesInterop` 也能覆盖「mod 自定义角色池」与「锻造器 / 事件遗物 / 附魔图标」，并在非 Starter 外部符文上补一道防线**

---

## 摘要

指南的软依赖面现在有 4 个成员（`ApiVersion == 1`）：
`RegisterPlayerRune` / `SetPlayerRunePoolLabel` / `RegisterConfigSectionTitle` / `RegisterExtraActProvider`。
我们按它接入后**零 patch**，体验很好。剩下两个落差，都是「功能有了但覆盖不到我们的场景」：

1. `RegisterPlayerRune` 的 `characterPool` 只接受原版 5 个角色的名字，
   **mod 自己的角色无法表达**（只能填 `null` 落进「通用」）。
2. 软依赖面**不含**那一批「不需要继承 `HextechRelicBase` 的能力」
   （锻造器 / 事件遗物 / 附魔图标 / `SavedProperty` 载体 / `GrantRelics`），
   而这些在语义上并不要求硬引用。

另附一条**可选**的健壮性建议（第 3 节）。

> **验证现状（请留意）**：截至 2026-09-28，我们这边**没有安装** HextechRunes（`mods/` 目录里没有），
> 手上那份可用于反编译的 dll 也**早于本指南** —— 它的 `HextechRunesInterop` 只有 `RegisterExtraActProvider`，
> `RegisterPlayerRune` 还在要求继承 `HextechRelicBase` 的 `HextechRunesApi` 里。
> ⇒ 我们的接入代码是**严格照指南的签名写的反射桥**，并且对每个成员都做了能力探测（解析不到就只降级那一步）。
> 如果签名与实现有出入，我们这边不会崩，但会安静地少注册 —— 若能告知实际发布版本号，我们可以补一次实测。

---

## R1 【建议】`characterPool` 支持 mod 自定义角色

**现状**（指南「参数」表）：

```
characterPool: Ironclad | Silent | Regent | Defect | Necrobinder ；为通用 null
```

**问题**：`PlayerRuneCharacterPool` 是原版角色的枚举，第三方 mod 的角色（例如我们的
`Emalin` / `Yalisarin` / `Hiro` / `Ananlin` / `Sherrylin`）**无法表达**，只能：
- 传 `null` ⇒ 落进「通用」组，**界面分组体现不出角色归属**，玩家看不出这枚符文是给谁的；
- 用 `isAvailableForPlayer` 按 `player.Character` 过滤 ⇒ 功能上能限制，但分组/排序仍然是「通用」。

> **2026-09-28 更新（我方）**：我们已经**按上面第 2 条落地了** —— 6 个符文**全部**用
> `isAvailableForPlayer` 按角色收窄（3 张 `is Sherrylin`、3 张 `is Hiro`，**没有一个是 `null` 通用**）。
> ⇒ 所以这条请求的**优先级已经从「阻塞」降到「纯体验优化」**：功能不缺，只是三选一界面上
> 看不出这枚符文是为哪个角色准备的（全部会被归进「通用」组）。请按你的排期权重自行决定。

**建议（任一即可，改动都很小）**：

- **A（最小）**：在 `SetPlayerRunePoolLabel` 之外，允许 `characterPool` 传**任意字符串**；
  未命中内置枚举时，按字符串分组并在 `relic_collection` 里查 `HEXTECH_POOL.<那个字符串>`。
  （与 `SetPlayerRunePoolLabel` 的 `poolKey` 机制天然一致，等于把「池键」直接开放给第三方。）
- **B（更稳）**：新增重载，`characterPool` 收一个 `Func<Player, bool>` 或
  `IReadOnlyCollection<Type>`/`HashSet<string>` 的角色类型集合，
  海克斯自己把「该角色是否在场」交给它判断，同时用返回的**显示名**做分组标题。

> 依据：下游本来就是注册表/元数据驱动的（`PlayerRuneMetadataCatalog.TypesByCharacter`），
> 目前唯一的瓶颈就是 `PlayerRuneCharacterPool?` 这个**封闭枚举**。

---

## R2 【建议】把「不要求继承基类」的能力补进软依赖面

指南「硬依赖」一节列出的方法里，以下这些**语义上并不需要调用方继承 `HextechRelicBase`**：

| 方法 | 为什么不需要硬依赖 |
| --- | --- |
| `RegisterForge<T>(rarity, assetModId)` | 第三方完全可以自己提供一个 `RelicModel` 子类的锻造器；`HextechForgeBase` 只是**基类约定**，不是必须 |
| `RegisterEventRelic<T>(assetModId)` | 事件遗物本来就是 `RelicModel`（你们自己的 `OrobasPlusRelicBase` 就不继承 `HextechRelicBase`） |
| `RegisterEnchantmentIcon<T>(iconPath)` | 纯资源登记，与基类无关 |
| `RegisterSavedPropertyCarrier<T>()` | 纯载体登记，与基类无关 |
| `RelicBundleGrantHelper.GrantRelics(player, types)` | 纯发放辅助方法 |

**建议**：把这些（或其中一部分）也放一份到 `HextechRunesInterop`，并把 `ApiVersion` 递增到 2。
这样「不硬引用」的第三方也能做锻造器/事件遗物，不必为了一个 `RegisterForge` 去引你的 dll。

> 兼容性说明：`HextechRunesInterop` 的签名稳定性承诺（「已发布的签名不再改动，新能力走新方法并递增
> `ApiVersion`」）正好支持这个做法 —— 我们只读 `ApiVersion`，`>= 2` 才调新方法，否则自动降级。

---

## R3 【可选，健壮性】把「是否外部符文」的判定从类型改成注册表

指南说了「只有 `Starter` 能保证原版别的抽取路径抽不到外部符文」——我们完全接受这个约定，
也把**所有**外部符文都标成 `Starter`。

但底层判据仍是**类型判定**：
`HextechNaturalRelicPoolHooks.cs:26` 的 `relics.Where(r => !(r is HextechRelicBase))`。
既然软依赖已允许第三方符文**不继承** `HextechRelicBase`，这一处就**滤不掉外部符文**了
（指南用「Starter 抽不到」来兜底）。

**建议**：既然 `BuildModelIdLookupCache()`（`HextechCatalog.cs:482`）**已经**把
`RuneIds` / `ForgeIds` / `ShopOnlyRelicIds` / `EnemyHexIconRelicIds` 这些 ModelId 集合算好了，
把它包成一个统一判据并用上去（对第三方**违规标了非 Starter** 的情况也稳）：

```csharp
public static bool IsHextechOwnedRelic(RelicModel? relic)
    => IsHextechRelic(relic) || IsHextechForgeRelic(relic)
    || IsHextechShopRelic(relic) || IsHextechEnemyHexIconRelic(relic);
```

`HextechNaturalRelicPoolHooks.cs:26` 改成 `relics.Where(r => !HextechCatalog.IsHextechOwnedRelic(r))` 即可。
**这条不改我们也能跑**（我们守 Starter 约定），纯属降低第三方踩坑概率。

---

## 附一：本次请求的最小改动量

- **R1** = 「让 `characterPool` 接受自定义字符串并在 `relic_collection` 查 `HEXTECH_POOL.<key>`」，
  或 1 个新增重载 + 1 个委托参数。
- **R2** = 把 4~5 个现有 public 方法**再暴露一份**到 `HextechRunesInterop` + `ApiVersion = 2`。
- **R3** = 1 个新 public 方法 + 改 1 处判定（可选）。

## 附二：我们这边现在的做法（供你评估影响面）

我们**零 patch、零硬引用**：自己的符文用自己现有的遗物基类（`ManosabaRelicTemplate : RelicModel`），
`Rarity` 一律 `RelicRarity.Starter`；图标/文案/品级全在我们自己的 PCK 里。

接入方式严格照指南：
① 按**程序集名** `HextechRunes` 检测（不按 manifest id），没找到就订阅 `AppDomain.AssemblyLoad`；
② 反射读 `HextechRunesInterop.ApiVersion`，`< 1` 直接跳过；
③ 反射调 `RegisterPlayerRune(Type, rarity, flags, characterPool, characterOrder, tagKey, assetModId,
   isAvailableForPlayer)`，然后 `SetPlayerRunePoolLabel` + `RegisterConfigSectionTitle`；
④ `tagKey` / `poolKey` 用自造的 `MANOSABA_LIN`，文案写在
   `ManosabaLin/localization/{zhs,eng,jpn,kor,rus}/relic_collection.json`。

**我们没有对 `HextechCatalog` 或任何其它 `internal` 类型打过 Harmony 补丁**（也按指南要求不再这么做）。
所以：你们内部怎么重构都不会影响我们；我们只依赖指南承诺的 `HextechRunesInterop` 公开面。

## 附三：English abstract（如需在英文区发帖）

> **Feature request.** The soft-dependency surface (`HextechRunesInterop`, `ApiVersion == 1`) already lets
> third-party mods register runes with zero patching — thanks, that's a big improvement.
>
> Two gaps remain:
>
> 1. `RegisterPlayerRune`'s `characterPool` only accepts the five vanilla character names, so a mod's own
>    characters can't be expressed (they collapse into "generic"). Please either accept arbitrary strings
>    and look them up as `HEXTECH_POOL.<key>` in `relic_collection`, or add an overload taking a character
>    type set / `Func<Player, bool>` plus a display name.
> 2. The interop surface only exposes 4 members. `RegisterForge`, `RegisterEventRelic`,
>    `RegisterEnchantmentIcon`, `RegisterSavedPropertyCarrier` and `RelicBundleGrantHelper.GrantRelics`
>    do not actually require the caller to inherit `HextechRelicBase`, so they could be exposed through
>    `HextechRunesInterop` too (`ApiVersion = 2`), letting non-hard-dependency mods build forges and
>    event relics.
>
> Optional robustness note: `HextechNaturalRelicPoolHooks.cs:26` filters by
> `!(relic is HextechRelicBase)`, which no longer matches externally registered runes. Since
> `BuildModelIdLookupCache()` already computes the ModelId sets, a public
> `IsHextechOwnedRelic(RelicModel)` built on them would keep the "external runes never leak into the
> vanilla natural relic pool" guarantee even for third parties that don't use `RelicRarity.Starter`.
