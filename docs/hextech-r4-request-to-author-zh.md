# 给海克斯符文（Hextech Runes）作者的对接请求 —— 角色专属外部符文的「保底槽」能力（R4）

> 独立、可直接复制粘贴的中文消息，发给海克斯符文 mod 作者。
> 配套设计文档 `docs/hextech-compat-hextech.md`（完整的 R1–R3 设计笔记）。

---

**标题：功能请求 —— 给角色专属外部符文加一个「保底槽」公开 API（`HextechRunesInterop`）**

作者你好：

我们是 **ManosabaLin**（杀戮尖塔 2 的一个 mod）的作者。我们一直通过 `HextechRunesInterop` 和你对接，体验非常好——**零 Harmony patch、零硬引用**你的程序集。我们用 `RegisterPlayerRune` 注册了 9 个「角色专属」玩家符文（雪莉 3 / 希罗 3 / 安安 3），每个都用 `isAvailableForPlayer` 按角色收窄，只有对应角色才会出现。感谢你提供了这么干净的对接面。

不过目前公开能力里有一件事我们做不到，想向你提个需求。

## 我们想要什么

玩家预期是这样的：**当前角色是我们三个角色之一、且该角色的专属联动符文没有被玩家在设置里关掉时，每一层第一次选海克斯的「三选一」里，必须强制包含该角色的一个专属符文**（无视这一层原本摇出的稀有度）。如果玩家没有这类符文就跳过；被刷新掉之后按正常规则刷新。

## 为什么现有能力做不到

- `isAvailableForPlayer` 只能**过滤**（能排除，不能强制包含）。
- `tagKey` / `characterWeightPercent` 只能**偏置权重**——而且我们发现那个 150% 的角色加权对 mod 角色基本是**死代码**：`TryGetRuneCharacterPool` 对任意非原版角色恒返回 `null`，于是 `IsRuneForCharacter` 永远为 `false`，那个乘子永远套不到我们的符文上。所以连「提升概率」都拿不到，只有硬保底槽才能真正解决。
- `RegisterChaosTransform` 是唯一能改写候选的钩子，但它只在 `HextechMayhemModifier` 且 `ChaosRuneChancePercent > 0` 时才会跑，而且 `TryAcceptTransformResult` 里用 `IsHextechRelic` 把外部符文拒掉了。所以这个钩子我们也用不了。

## 提议的 API（任意一个都行，我们最想要 **A**）

**A（最小改动）：** 在 `HextechRunesInterop` 里加一个保底符文提供者（`ApiVersion → 2`）：

```csharp
// 返回 null = 这次选择不保底；否则返回的必须是已注册的玩家符文（含外部符文）
public static void RegisterGuaranteedPlayerRune(Func<Player, RelicModel?> provider);
```

语义：`PickWeightedDistinct` 填满 3 个候选之后，如果 `provider(player)` 返回一个「本层合法可获取」的符文（已注册、本层允许、角色匹配、未被拥有/未禁用），就把它顶替掉第 1 个槽位；剩下 2 个照常抽取。刷新走你现有的刷新逻辑，无需特殊处理。

**B（更贴近元数据驱动）：** 给 `RegisterPlayerRune` 加一个 `GuaranteedFirstPick` 标志，意思是只要该符文的角色符合条件且未被禁用，就强制塞进选择。需要在你候选生成链里加一个「保底候选」判定点。

**C（最通用）：** 开放一个候选后处理钩子（和 `RegisterChaosTransform` 类似，但**不要**限定在 Mayhem 下、**也不要**要求 `IsHextechRelic`——只要返回的候选是一个「已注册」的玩家符文即可，即把 `IsHextechRelic` 换成 `TryGetPlayerRuneRarityById` 之类的判定）。这个保底需求以及以后任何改写候选的需求都能复用同一个钩子。

## 我们的约束

我们**不会** patch 你任何 `internal` 类型（遵守你的规范）。我们只依赖文档化的 `HextechRunesInterop` 对接面，并读取 `ApiVersion` 做优雅降级。

如果你那边暂时没有排期，也直接告诉我们，我们就改成游戏内「此功能不可用」的降级提示。

非常感谢你一直维护这么好的对接支持！

—— ManosabaLin 团队
