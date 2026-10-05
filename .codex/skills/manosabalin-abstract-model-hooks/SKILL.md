---
name: manosabalin-abstract-model-hooks
description: Choose and implement common AbstractModel hooks for ManosabaLin cards, relics, powers, monsters, and RitsuLib model capabilities. Use when deciding between Before/After/Modify/TryModify hooks, owner hook capabilities, combat hook participation, clone lifecycle, or command-safe gameplay timing.
---

# ManosabaLin AbstractModel Hooks

## First Rule

Read the actual signatures before editing. 本项目可用的引擎反编译源码在 `D:\ManosabaLin\.local\probe_engine\decomp\`（`.local/` 被 gitignore ⇒ 检索工具必须显式传 `path=`）：

- Base game: `...\decomp\MegaCrit.Sts2.Core.Models\AbstractModel.cs`
- Cards: `...\decomp\MegaCrit.Sts2.Core.Models\CardModel.cs`
- Relics: `...\decomp\MegaCrit.Sts2.Core.Models\RelicModel.cs`
- Powers: `...\decomp\MegaCrit.Sts2.Core.Models\PowerModel.cs`
- Monsters: `...\decomp\MegaCrit.Sts2.Core.Models\MonsterModel.cs`
- Rooms / 奖励: `...\decomp\MegaCrit.Sts2.Core.Rooms\CombatRoom.cs`
- RitsuLib: `D:\ManosabaLin\.local\external\STS2-RitsuLib\src\`（含 `Models\HookedSingletonModel.cs`）

Do not guess nullability, `PlayerChoiceContext`, or return types.

## Hook Selection

- Use `Before...` hooks to validate, prepare, or mutate state before the game action.
- Use `After...` hooks for side effects after the action succeeds.
- Use `Modify...` hooks for pure value changes; return only the delta or replacement shape the base method expects.
- Use `TryModify...` hooks when the hook needs to report whether it changed an object/list/value.
- Use `AfterModifying...` hooks to perform follow-up side effects after a modifying hook has been selected.

Common hooks:

- Combat lifecycle: `BeforeCombatStart`, `BeforeCombatStartLate`, `AfterCombatEnd`, `AfterCombatVictory`, `AfterCreatureAddedToCombat`.
- Turn lifecycle: `BeforeSideTurnStart`, `AfterSideTurnStart`, `AfterPlayerTurnStart`, `BeforeSideTurnEnd`, `AfterSideTurnEnd`, late/early variants.
- Card flow: `AfterCardEnteredCombat`, `AfterCardGeneratedForCombat`, `AfterCardChangedPiles`, `AfterCardDrawn`, `AfterCardDiscarded`, `AfterCardExhausted`.
- Card play: `BeforeCardPlayed`, `AfterCardPlayed`, `AfterCardPlayedLate`, `ModifyCardPlayCount`, `ModifyCardPlayResultPileTypeAndPosition`.
- Damage/block: `BeforeAttack`, `AfterAttack`, `ModifyAttackHitCount`, `ModifyDamageAdditive`, `ModifyDamageMultiplicative`, `BeforeDamageReceived`, `AfterDamageReceived`, `ModifyBlockAdditive`, `AfterBlockGained`.
- Powers: `BeforePowerAmountChanged`, `AfterPowerAmountChanged`, `ModifyPowerAmountGivenAdditive`, `TryModifyPowerAmountReceived`.
- Economy/rewards/map: `AfterGoldGained`, `ModifyGoldGained`, `TryModifyRewards`, `AfterRewardTaken`, `ModifyGeneratedMap`, `AfterRoomEntered`.

## Reward Offer Hooks & Save Restore (⚠️ read before using `BeforeCombatRewardOffered`)

`CombatRoom.OfferRoomEndRewards()` is reached from **two very different contexts**:

| Context | Call path | Room node mode | State |
|---|---|---|---|
| 正常打完战斗 | `CombatManager.CombatWon` → `NCombatUi.OnCombatWon` → `ShowRewards` | `CombatRoomMode.ActiveCombat` | UI 就绪、画面正常 |
| **读档恢复**（战斗已结束、只是重新进房间） | `RunManager.EnterRoomInternal` → `CombatRoom.EnterInternal` → `StartPreFinishedCombat` | `CombatRoomMode.FinishedCombat` | 画面已 FadeOut 到全黑、`FadeIn` 要等整条链返回后才执行 |

`OfferRoomEndRewards()` 内部顺序是：`RewardsCmd.GenerateForRoomEnd` → **`await Hook.BeforeCombatRewardOffered(...)`** → `TaskHelper.RunSafely(reward.Offer())`。

⇒ 在 `BeforeCombatRewardOffered` 里 `await` 任何**交互式屏幕**（尤其 `CardSelectCmd.*` 模态选卡），**正常战斗没问题，但读档恢复会把整次加载挂死在黑屏上**（奖励屏都还没出现）；玩家只能强退，强退取消会打断房间恢复链，最终在 `RunManager.EnterRoomInternal` 抛 `NullReferenceException`。

**用法**：hook 里必须按「读档恢复」跳过，判据是引擎专门为这个场景提供的房间节点模式：

```csharp
if (NCombatRoom.Instance?.Mode == CombatRoomMode.FinishedCombat) return false;
```

（`CombatRoomMode.FinishedCombat` 文档原文：*"Used when loading a save after combat ended but before leaving the room."*；正常战斗中节点模式恒为 `ActiveCombat`，`NCombatRoom.Mode` 只在 `NCombatRoom.Create` 里赋值。）

注意 `CombatRoom.IsPreFinished` **不能**用来区分——`CombatManager.EndCombatInternal` 在战斗一结束就 `MarkPreFinished()`，两条路径都是 `true`。

若要做「每个首领只发一次」的幂等，别用机器本地 `HashSet`（联机两端会判断分叉而卡同步）；本奖励是靠 `PlayerChoiceSynchronizer` 同步模态选卡的。

## Model-Specific Notes

- `CardModel` hooks for the card itself include `AfterCreated`, `AfterTransformedFrom`, `AfterTransformedTo`, `OnEnqueuePlayVfx`, plus `OnPlay`/`OnUpgrade` through `ManosabaCardTemplate`.
- `RelicModel` has `AfterObtained`, `AfterRemoved`, `IsAllowed`, `IsAllowedAtNeow`, `ShowCounter`, and `DisplayAmount`.
- `PowerModel` has `BeforeApplied`, `AfterApplied`, `AfterRemoved`, `ShouldPowerBeRemovedAfterOwnerDeath`, `PowerType`, `PowerStackType`, and `AllowNegative`.
- `MonsterModel` has `AfterAddedToRoom`, `BeforeRemovedFromRoom`, `AfterDeath`, and move-state machinery.

## Combat Hook Participation

- `AbstractModel.ShouldReceiveCombatHooks` controls whether combat hooks are called.
- Cards in combat piles, powers, relics, and active combat models normally receive relevant hooks.
- Disconnected canonical models should not be treated as mutable runtime state.
- Use `AssertMutable()` only to guard against misuse; do not paper over canonical/mutable mistakes by cloning at the wrong layer.

## RitsuLib Capability Hooks

- `OwnerHookCapability<TModel>` lets a capability receive the owning model's vanilla hooks.
- `CardCapability` receives card lifecycle helpers such as owner card upgraded/downgraded/transformed.
- `IModelCapabilityHookListener.OwnerHookOrder` controls ordering: negative before owner, zero/positive after owner.
- Gameplay-affecting multiplayer logic should use awaited vanilla hooks or RitsuLib owner-hook capabilities, not fire-and-forget side channels.

## Safety Rules

- Use command APIs for gameplay effects; avoid direct mutation when a command exists.
- Check owner/side/target guards explicitly.
- If adding events or delegate fields to an `AbstractModel`, inspect `AfterCloned`; shallow-copied delegates must not leak to clones.
- Validate with `dotnet publish ManosabaLin.csproj -c Release`（在仓库根目录执行；游戏运行时会锁 DLL ⇒ 先关游戏）。
