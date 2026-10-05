# Request to Hextech Runes author — guaranteed slot for character-specific external runes (R4)

> Standalone, copy-paste-ready message for the Hextech Runes mod author (Natsuki).
> Companion to `docs/hextech-compat-hextech.md` (full design notes / R1–R3).

---

**Subject: Feature request — guaranteed-slot API for character-specific external runes (`HextechRunesInterop`)**

Hi Natsuki,

We're the authors of **ManosabaLin** (a Slay the Spire 2 mod). We've integrated with Hextech
Runes through `HextechRunesInterop` and it works great — **zero Harmony patches, zero hard
references** to your assembly. We register 9 character-exclusive player runes (3 for Sherrylin,
3 for Hiro, 3 for Ananlin) via `RegisterPlayerRune`, gating each with `isAvailableForPlayer` so
they only surface for the matching character. Thank you for the clean interop surface.

There is one capability we cannot achieve through the current public surface, and we'd like to
request it.

## What we want

Our users expect: when the current character is one of ours and has a linkage rune the player
hasn't disabled in settings, the **first rune selection of each act must always include one of
that character's runes** (regardless of the rarity rolled for that floor). If the player has no
such rune, skip. After a reroll, fall back to normal rules.

## Why the current surface can't do it

- `isAvailableForPlayer` only *filters* (can exclude, cannot force inclusion).
- `tagKey` / `characterWeightPercent` only *bias* the weights — and we found the 150%
  character bonus is effectively **dead code for mod characters**: `TryGetRuneCharacterPool`
  returns `null` for any non-vanilla character, so `IsRuneForCharacter` is always `false` and the
  multiplier never applies to our runes. So we can't even get a boost — only a hard guaranteed
  slot would help.
- `RegisterChaosTransform` is the only candidate-rewriting hook, but it only runs under
  `HextechMayhemModifier` with `ChaosRuneChancePercent > 0`, **and** `TryAcceptTransformResult`
  rejects external runes via `IsHextechRelic`. So it's unusable for us.

## Proposed API (any one of these; **A** is our preference)

**A (minimal):** add a guaranteed-rune provider to `HextechRunesInterop` (`ApiVersion → 2`):

```csharp
// return null = no guarantee this selection; otherwise the returned rune must be
// an already-registered player rune (incl. external runes)
public static void RegisterGuaranteedPlayerRune(Func<Player, RelicModel?> provider);
```

Semantics: after `PickWeightedDistinct` fills the 3 candidates, if `provider(player)` returns a
legally-obtainable rune (registered, allowed this act, character-eligible, not already
owned/blocked), it replaces slot 1; the other 2 are drawn normally. Rerolls use the existing
reroll logic, no special handling.

**B (closer to metadata-driven):** add a `GuaranteedFirstPick` flag to `RegisterPlayerRune`,
meaning that rune is forced into the selection whenever its character is eligible and not
disabled. Requires a "guaranteed candidate" check point in your candidate-generation chain.

**C (most general):** open a candidate post-process hook (like `RegisterChaosTransform`, but
**not** gated on Mayhem and **not** requiring `IsHextechRelic` — only that the returned candidate
is a *registered* player rune, i.e. swap `IsHextechRelic` → `TryGetPlayerRuneRarityById`). Both
this guarantee and future candidate-rewrite needs could reuse the same hook.

## Constraints

We will **not** patch any of your `internal` types (per your guideline). We depend only on the
documented `HextechRunesInterop` surface and read `ApiVersion` to degrade gracefully.

If this isn't on your roadmap, just let us know and we'll add an in-game "feature unavailable"
fallback note instead.

Thanks for the great interop support!

— ManosabaLin team
