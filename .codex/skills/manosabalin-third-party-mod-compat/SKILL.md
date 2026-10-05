---
name: manosabalin-third-party-mod-compat
description: Audit whether a third-party Slay the Spire 2 mod can coexist with ManosabaLin at runtime — Harmony patch-target overlap, soft-dependency contract drift, registration/init windows, and silent-failure acceptance criteria. Use when the user hands over another mod's source (e.g. HextechRunes) or asks whether a cross-mod feature will actually trigger.
---

# Third-Party Mod Coexistence Audit (ManosabaLin)

Use this when auditing coexistence with another STS2 mod: patch collisions, soft-dependency
contracts, init-order windows, and how failures surface.

**Read-only by default.** Produce a report; do not change code unless the user names the item.
If a prior audit report exists (usually `C:\Users\Lenovo\Desktop\*-verdict.md`), read it first and
only report what is *new* — do not re-state settled conclusions.

Ground rules for this repo: cross-mod code lives in `ManosabaLinCode/Compat/<Mod>/` and must be
**zero compile-time reference** to the other mod (reflection + string names only). Never Harmony-patch
the other mod's `internal` types. See `.workbuddy/memory/ENGINE-API.md` and `PITFALLS.md`.

## Step 1 — Harmony patch-target overlap

Run the scanner over our tree and theirs:

```bash
python .codex/skills/manosabalin-third-party-mod-compat/scripts/harmony_overlap_scan.py \
  ManosabaLinCode \
  "<other mod>/src"
```

For each `Type|method` in the intersection, classify it:

| Class | Meaning | Risk |
|---|---|---|
| **Both non-skipping** | both are prefix-void / postfix | none |
| **Stacking prefixes** | both `Prefix`, neither guarantees `return true` first | order-dependent |
| **Skip vs skip** | both may `return false` on the same input | one silently wins |

**Harmony priority rules that decide the outcome (verify, do not assume):**
- full enum: `Last=0, VeryLow=100, Low=200, LowerThanNormal=300, Normal=400, HigherThanNormal=500, High=600, VeryHigh=700, First=800`.
  An unannotated patch method is `Priority.Normal` = **400**; `[HarmonyPriority(Priority.Low)]` = **200**.
  ⚠️ Do not guess the numeric value — read `Priority` out of the decomp (`grep -n -B2 -A14 "enum Priority"`).
- a **`void` prefix can never skip** the original. Only a prefix whose declared return type is `bool`
  and which has a `return false;` on some path can. The scanner reads the *declaration form* precisely
  to avoid flagging `void` prefixes as skippable.
- **as soon as one prefix returns `false`, all remaining prefixes AND the original are skipped.**
- so: high-priority prefix that returns `false` ⇒ the low-priority one never runs ⇒ its
  bookkeeping/side effects are lost even though "nothing crashed".
- equality ⇒ Harmony uses patch order (install order), so prefer removing the tie rather than relying on it.

Report each real collision with: the input that triggers it, what the loser fails to do, and whether
it is *deterministic* (a real behavior gap) or *racy* (a timing bug). Deterministic + low probability
is normally acceptable — say so instead of inflating it.

## Step 2 — Soft-dependency contract drift

For every reflected call, confirm the target still exists with the **exact** parameter list:

- `GetMethod(name, Public|Static, null, types: [...], null)` returns `null` on any mismatch —
  a renamed/added parameter is a **silent** capability loss unless the code logs it.
- Check the other mod's assembly name (often pinned in its `.csproj` as `<AssemblyName>`), not its
  manifest id — variants may ship the same assembly name.
- Enumerate *all* overloads of the patched method (`grep -n "public static" …` in the decomp) and make
  sure our `[HarmonyPatch(typeof(X), "Y", [types])]` matches one exactly, including generic vs non-generic.

## Step 3 — Init / registration windows

Third-party registration APIs usually guard on a frozen state. Find the guard and locate it in the
call sequence:

- locate the throw (`InvalidOperationException` "too late" / "window closed") and read **what it
  protects** (model pool? serialization cache?).
- check **atomicity**: is the real registration the *last* statement? If validation runs first and
  registration last, failure is a clean "not registered" and not a half-registered wreck — verify by
  reading the entry point, don't infer.
- narrow the blast radius: a guard that only trips for types carrying a serialization attribute
  (e.g. `[SavedProperty]`) affects **only those types** — list which of ours qualify, don't say "all".
- our side should register **per item in try/catch** and log a summary count
  (e.g. `ok=9/9`). Silent partial registration is the most expensive failure mode: the game runs fine
  and the content just never appears.

**When the window is a runtime race, the acceptance criterion is the log line, not the code.**
State the exact string to grep for and what each failure variant means.

### 3b — Do NOT trust `AppDomain.AssemblyLoad` as "the target is ready"

A third-party mod may ship **two** assemblies: a small `<Mod>.Loader` (itself a `[ModInitializer]`)
that picks a per-game-version variant and then, **inside its own initializer**, does
`AssemblyLoadContext.LoadFromAssemblyPath(<Mod>.dll)` and only afterwards calls the real
`<Mod>.ModEntry.Initialize()`. Consequences:

- If our initializer runs first, our `AssemblyLoad` handler fires at the `LoadFromAssemblyPath`
  moment — i.e. **before** the target has installed its own bootstrap/config/registry state.
  Registration at that instant is a coin flip on mod load order.
- It is **not** an ALC issue: verified empirically that `AppDomain.AssemblyLoad` *does* fire for
  custom-ALC `LoadFromAssemblyPath` — so the event is reliable, the *timing* is what is not.
- Nor can we force it: `Assembly.Load("<Mod>")` resolves against the default probing path, but the real
  dll usually lives under `lib/<variant>/` and is version-picked by that loader. Forcing the load both
  fails to find it **and** risks picking an incompatible variant, defeating the loader's guard.

**Fix pattern — a deterministic "late sweep", still inside the window:**

```csharp
// RitsuLib gives replayable game-lifecycle events; these fire after ALL mod initializers,
// yet before any run exists (so the relic/model pools are not frozen yet).
RitsuLibFramework.SubscribeLifecycleOnce<GameReadyEvent>(_ => Sweep("GameReady"));
RitsuLibFramework.SubscribeLifecycleOnce<MainMenuReadyEvent>(_ => Sweep("MainMenuReady"));
```

`SubscribeLifecycleOnce<T>(handler, replayCurrentState: true)` invokes the handler once, and
**replays synchronously if the event already fired** — so subscribing late is safe too. Make the sweep
idempotent (Step 3c) and log the outcome. Note the replay only triggers after the event has actually
been raised, so subscribing during mod init does not itself re-introduce the early-call hazard.

### 3c — Make the fix idempotent per item, and make under-registration LOUD

- Track registered items in a set keyed by type name; `RegisterAll()` skips ones already done and
  re-attempts only the failures. This is what makes the Step 3b retry safe (no double registration).
- Expose `AllRegistered` so the sweeps can cheaply no-op, and so diagnostics can print it.
- Per-item `try/catch` must keep the mod loading (never let one item break init) — but the **summary
  when not full must be `Error`, not `Warn`**, listing the exception type + message per failed item.
  A `Warn` for "8 of 9 registered" is exactly the silent-failure mode this whole skill exists to kill.
- Keep auxiliary registrations (labels / config section titles) behind a flag that is only marked done
  when the main registrations are complete, so a later sweep redoes them for late-arriving items.

## Step 4 — Multiplayer / determinism review

Outside instructions say the same thing as our `mp-desync` rules; apply them:

- any "random" must come from `Owner.RunState.Rng.*`; flag `Random.Shared` and **indexing into a list
  whose order comes from registration order** (that order is mod-load-order dependent) — a
  `NextItem(unlockedCards)` can desync two clients with identical mod sets.
- turn hooks must confirm the holder via `participants` / `player != Owner`, because a teammate's
  extra turn re-enters the hook for that player.
- "first round of combat" ⇒ `PlayerCombatState.TurnNumber`, not `RoundNumber`.

## Step 5 — Report

Write to `C:\Users\Lenovo\Desktop\<mod>-compat-possible-issues.md` (or the path the user names).
Lead with a severity-sorted table (`file:line` evidence per row), then per-item detail with
**trigger / consequence / confirmation status**. Separate:

- *confirmed from source* vs *static inference, unverified* — the user runs no headless tests, so be
  explicit about which is which and never dress inference up as certainty;
- *real bug* vs *doc drift* vs *accepted trade-off / by design*.

End with a short "我建议的动手顺序" table and a reminder that nothing was changed without approval.

## Pitfalls

- Do not conclude "will work" from the fact that a method resolves. Resolution + window + priority are
  three independent failures.
- Do not re-derive the other mod's semantics from our own `docs/`; read **its** source. Its
  `INTEGRATION.md` is usually more authoritative than our notes and may contain rules our docs lack.
- Delegating to the other mod's patch that we replace: when we `return false`, we must reproduce
  **every** branch of the original. Diff our replacement against the decompiled original line by line —
  dropped branches (e.g. an extra-payment hook) are the usual miss.
- When we replace a **base-game** model's hook to implement a compat effect (e.g. a Harmony prefix on a
  power's `AfterSideTurnEnd`), reproduce that hook's **own guard verbatim**. "Improving" it to the
  participants-based check silently changes vanilla behavior for every other holder of that power.
- Check *which hook stream actually reaches the model before claiming a hook is missing*:
  `RunState.IterateHookListeners(combatState)` **skips** run-level models (relics, potions) whenever
  `combatState != null` — but its last step chains `CombatState.IterateHookListeners()`, which **adds
  each player's relics**. So a relic's `AfterCombatEnd` / `BeforeCombatStart` *does* fire even though the
  run-level branch looked closed. Verify by reading both iterators before adding a workaround.
- A `static Dictionary<Player, …>` registry (hanging orbs, pending selections, …) keyed on `Player` is a
  lifetime leak unless it has an explicit combat-end (or run-end) clear. Turn-start cleanup is **not**
  enough: "still registered when the combat ends / the holder dies" is a separate path.
