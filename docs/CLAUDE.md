# CLAUDE.md — AI Assistant Context for Sticker's Out

This file is read by Claude/Copilot before working in this repo. Keep it accurate — if a decision below goes stale, update it in the same PR that changes the decision.

## Project Snapshot

- **Name:** Sticker's Out
- **Engine:** Unity (mobile, iOS + Android)
- **Genre:** Grid-based picture-fragment merge & exit puzzle
- **Core loop:** Drag picture fragments around a grid. Adjacent matching fragments merge. A fully-reassembled picture can be dragged off the board through a matching door (gated by category/color). Clear all target fragments before the timer runs out.
- **Origin:** Ported from an HTML5/JS prototype (`Block Jam: Associations`). The prototype supported both word/text fragments and image fragments — **this project only implements the image/picture path.** See "Explicit Non-Goals" below.
- **Full docs index:** see the table in `PROJECT_STRUCTURE.md` and the doc list in `README.md`.

## Explicit Non-Goals (do not implement unless asked)

These were deliberately cut from the original prototype's scope. Do not re-add them "for completeness" — ask first if a task seems to require one:

- **No text/word fragment system.** No `char`, `targetWord`, letter-axis matching, or word-run scanning. `MergeSolver` only ever matches image fragments by `src` + slice coordinates.
- **No sticker-collection / meta-game save data.** A future "sticker-making machine + collection book" meta layer has been discussed but is **not being built and not being hooked into `SaveService` or any completion event.** Don't add `StickerDefinition`, `StickerCollectionService`, or similar unless explicitly scoped.
- **No YouTube Playables SDK integration.** The prototype had this; it's irrelevant for native mobile and should not be ported.
- **No physics engine (Rigidbody2D/PhysX) for block movement.** Drag/collision is a custom stepped-increment solver against a cached grid matrix, ported deliberately from the prototype to preserve exact game feel. Do not "simplify" this into physics-based movement.
- **No renamed engine terminology (yet).** Code uses generic terms — `Block`, `Door`, `Grid`, `Fragment` — not sticker-themed names like "Sticker Piece" or "Collection Slot." This is a deliberate, deferred decision, not an oversight. Don't rename classes/fields to sticker language without being asked.

## Key Architectural Decisions (why things are the way they are)

- **Levels are entirely data-driven.** All level content (grid size, walls, void cells, doors, blocks/fragments) lives in `LevelData` ScriptableObjects, not hardcoded in gameplay scripts. See `DATA_MODEL.md`.
- **Core logic is engine-agnostic where possible.** `MergeSolver`, `GridCollisionMap`, and related pure-logic classes should not depend on `MonoBehaviour` — they take/return plain data so they're unit-testable in EditMode without a scene. Rendering/input glue lives in separate `MonoBehaviour` "View"/"Controller" classes that call into this core logic.
- **All 30 levels from the original prototype are reference-only.** They are not shipped content and should not be imported/migrated. New levels are authored from scratch via the in-Unity Level Editor (`LevelEditorWindow.cs`).
- **No legacy content importer.** There is intentionally no "import old Levels.js" tool in this project — don't build one unless asked.

## Coding Conventions

Full detail in `CODING_CONVENTIONS.md`. Summary for quick reference:
- C# standard Unity naming: `PascalCase` for classes/methods/public fields, `camelCase` for private fields (no `_` prefix unless the team adopts one — check `CODING_CONVENTIONS.md` before assuming).
- Prefer composition over deep inheritance for gameplay MonoBehaviours.
- Favor events/callbacks (`Action<T>`, `UnityEvent`) over direct cross-references between systems (e.g., `MergeSolver` should not know about `AudioService`; something higher up wires "on merge complete → play sound").
- New systems should have a corresponding EditMode test where logic is pure (see `TESTING.md`).

## When Asked to Add a Feature

1. Check whether it touches something listed under **Explicit Non-Goals** above — if so, flag it and confirm before building.
2. Check `GAME_DESIGN.md` and `ARCHITECTURE.md` for whether an existing system already owns this responsibility before creating a new one.
3. Check `DATA_MODEL.md` before adding new fields to `LevelData` or related ScriptableObjects — level data changes affect the Level Editor tool and any already-authored levels.
4. Prefer extending an existing data-driven system (e.g., `TutorialStepDefinition` list) over hardcoding new special cases into controller scripts.

## Reference Docs

| File | Contents |
|---|---|
| `ARCHITECTURE.md` | System list, responsibilities, data flow |
| `DATA_MODEL.md` | `LevelData` / `BlockData` / `DoorData` / `GridData` schemas |
| `PROJECT_STRUCTURE.md` | Folder layout and naming conventions |
| `GAME_DESIGN.md` | Rules, win/lose conditions, power-up behavior, category/door matching |
| `MERGE_SOLVER_SPEC.md` | Exact fragment-matching/merge algorithm |
| `DRAG_AND_COLLISION_SPEC.md` | Drag physics constants, collision margin behavior |
| `GRID_SYSTEM.md` | Grid coordinate conventions, cell size, wall thickness |
| `POWERUPS.md` | Rocket/Hammer behavior and unlock rules |
| `TUTORIAL_FTUE.md` | Data-driven tutorial trigger system |
| `SAVE_SYSTEM.md` | Save file schema and persistence rules |
| `LEVEL_EDITOR.md` | In-Unity Level Editor tool usage/spec |
| `CATEGORY_TAXONOMY.md` | Canonical category list for doors/fragments |
| `ART_STYLE_GUIDE.md` | Visual direction for fragment art, UI, icons |
| `AUDIO_VFX_SPEC.md` | SFX cue list, particle event hooks |
| `CODING_CONVENTIONS.md` | C# style rules |
| `TESTING.md` | What must be unit tested, test project layout |
| `ROADMAP.md` | Phased delivery plan |
| `CHANGELOG.md` | Dated log of scope/decision changes |

## If Something Here Seems Wrong

If a request conflicts with something stated in this file, say so explicitly rather than silently picking one side — these decisions were made deliberately with the project owner and may need an explicit override, not a guess.
