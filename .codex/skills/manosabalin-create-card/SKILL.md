---
name: manosabalin-create-card
description: Create or modify cards in the ManosabaLin Slay the Spire 2 mod. Use when adding a CardModel, editing card behavior, wiring card registration, card art, dynamic vars, keywords, upgrades, generated cards, card localization, or card pool membership under ManosabaLinCode/Characters.
---

# ManosabaLin Create Card

## Workflow

1. Read `AGENTS.local.md`; use the local STS2, MinionLib, and RitsuLib paths for API checks before guessing signatures.
2. Pick the owning character/pool:
   - Hiro: `HiroCardPool`
   - Ema/Emalin: `EmalinCardPool`
   - Sherrylin: `SherrylinCardPool`
   - Shared/token cards: usually `LinCardPool`
3. Put the class under the owning character folder, normally `ManosabaLinCode/Characters/<Character>/Cards/`.
4. Prefer `ManosabaCardTemplate`:

```csharp
[RegisterCard(typeof(SherrylinCardPool))]
public sealed class ExampleCard()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.Value, ValueProp.Card, this);
    }
}
```

## Common Card Pieces

- Use `[RegisterCard(typeof(...Pool))]`; RitsuLib gives the public entry `MANOSABA_LIN_CARD_<TYPE_NAME>`.
- Use `protected override IEnumerable<DynamicVar> CanonicalVars` for text variables such as `DamageVar`, `BlockVar`, `CardsVar`, or project-specific vars.
- Use `CanonicalKeywords` for printed keywords such as `CardKeyword.Exhaust`.
- Put upgrade logic in `OnUpgrade(ComponentContext componentContext)`.
- Use command APIs (`DamageCmd`, `CreatureCmd`, `PowerCmd`, `CardPileCmd`, `PlayerCmd`) instead of mutating runtime state directly.
- For generated combat cards, create mutable cards through `CombatState.CreateCard<T>(owner)`, `ICombatState.CreateCard(canonicalCard, owner)`, or `CardFactory.GetForCombat(...)`.
- Attach model capabilities with `card.GetOrCreateCapability<TCapability>()`; use `$manosabalin-create-capability` for capability details.

## Localization

Update all supported locales unless the user explicitly narrows the task:

- `ManosabaLin/localization/eng/cards.json`
- `ManosabaLin/localization/zhs/cards.json`
- `ManosabaLin/localization/jpn/cards.json`

Use the RitsuLib public entry:

```json
"MANOSABA_LIN_CARD_EXAMPLE_CARD.title": "Example",
"MANOSABA_LIN_CARD_EXAMPLE_CARD.description": "Gain {Block:diff()} Block.",
"MANOSABA_LIN_CARD_EXAMPLE_CARD.selectionScreenPrompt": "Choose a card"
```

Only add extra keys actually used by the code, such as `selectionScreenPrompt`, `selectionScreenPrompt2`, or custom `LocString` suffixes.

## Art

`ManosabaCardTemplate` resolves art from the class name lowercased:

- Big: `ManosabaLin/images/cards/big/<classname>.png`
- Small: `ManosabaLin/images/cards/<classname>.png`
- Beta: `ManosabaLin/images/cards/beta/<classname>.png`
- Fallback: `ManosabaLin/images/cards/card.png`

Do not edit generated `.uid` files.

## Card frame (装饰卡框)

The decorative character frame is applied by `ManosabaLinCode/Patches/CharacterCardFramePatch.cs`
(`NCard.Reload` postfix) and **its owner character is derived from the card class's namespace**, not its
physical folder and not its `CardPool`. Namespace segment after `Characters` ⇒ character
(`ManosabaLin.Characters.Hiro.Cards` ⇒ `Hiro`), then it looks for
`images/characters/{Character|Characterlin}/{stem}.png` where stem is `{character}`/`{character}lin` ×
`card.png`/`_card.png` (e.g. `Ema` ⇒ `Emalin/emalincard.png`, `Hiro` ⇒ `Hiro/hirolincard.png`).

- **Wrong frame on a card?** Almost always a copy-pasted `namespace` on the card file. Fix the namespace to
  match the owning character (physical folder) — that also fixes `AudioEventPathProvider.CardHomeName`.
- **One-off frame, same character folder** → add `[typeof(MyCard)] = "mystem"` to `ExclusiveFrameStems`.
- **Frame living in another character's folder** (e.g. a Sherry card class placed under `Hiro`) → add
  `[typeof(MyCard)] = ("Sherrylin", "sherrylincard")` to `CrossDirectoryFrames`.
- **No decorative frame at all** → add the type to `NoFrameCards` (the engine's own Ancient/etc. frame still shows).

## Checks

- Search for nearby cards in the same character before inventing patterns.
- Keep localization keys structurally aligned across `eng`, `zhs`, and `jpn`.
- Build with `dotnet build ManosabaLin.csproj` (never the `.sln`); deploy with `dotnet publish ManosabaLin.csproj`.
- If JSON changed, parse the touched localization files.
