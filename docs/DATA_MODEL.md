# DATA_MODEL.md — Sticker's Out

Defines the schema for level content and related config. This is the contract between the **Level Editor tool**, **LevelLoader**, and **runtime gameplay** — changes here ripple to all three, so treat this file as the source of truth and update it in the same PR as any schema change.

All level content is **image/picture fragments only** — there is no text/word schema. See `CLAUDE.md` Non-Goals.

---

## `LevelData` (ScriptableObject)

One asset per level, authored via the in-Unity Level Editor.

| Field | Type | Notes |
|---|---|---|
| `levelId` | `string` | Unique stable ID, e.g. `"level_01"`. Used by `LevelSequence` and save data — do not reuse or reassign once a level ships. |
| `timeLimit` | `int` (seconds) | Countdown duration |
| `grid` | `GridData` | See below |
| `doors` | `List<DoorData>` | See below |
| `walls` | `List<Vector2Int>` | Static blocked cells |
| `blocks` | `List<BlockData>` | Initial block layout |

## `GridData`

| Field | Type | Notes |
|---|---|---|
| `rows` | `int` | |
| `cols` | `int` | |
| `voidCells` | `List<Vector2Int>` | Cells excluded from the playable grid entirely (visually and for collision) |

## `DoorData`

| Field | Type | Notes |
|---|---|---|
| `edge` | `DoorEdge` (enum: `Top`, `Bottom`, `Left`, `Right`) | Which board edge the door sits on |
| `position` | `int` | Starting cell index along that edge |
| `span` | `int` | Number of cells the door covers |
| `category` | `string` | Must match a value in `CategoryDefinitions` (see `CATEGORY_TAXONOMY.md`) |
| `color` | `string` | Fallback match key, used when a fragment has no category or as secondary matching, mirrors source engine behavior |
| `iceCount` | `int` (default 0) | If > 0, door requires this many category/color-matching completions elsewhere on the board before it will accept an exit |

## `BlockData`

| Field | Type | Notes |
|---|---|---|
| `id` | `int` | Unique within the level at authoring time; runtime may reassign IDs after merges/splits (mirrors source engine behavior — do not treat as a stable identity across gameplay) |
| `x`, `y` | `int` | Anchor grid position |
| `shape` | `List<FragmentData>` | The fragment cells making up this block |

## `FragmentData`

Represents a single picture-fragment cell within a block. This replaces the source engine's dual `text`/`image` shape entry — **only the image fields exist in this project.**

| Field | Type | Notes |
|---|---|---|
| `src` | `SpriteReference` (or asset GUID string) | The source picture this fragment belongs to |
| `sliceX`, `sliceY` | `int` | This fragment's position within the source picture's fragment grid |
| `gridCols`, `gridRows` | `int` | How many fragments the source picture is sliced into total — must match across all fragments sharing the same `src` |
| `targetCategory` | `string` | Matches a `DoorData.category` — must match a value in `CategoryDefinitions` |
| `color` | `string` | Fallback match key, mirrors `DoorData.color` |
| `dx`, `dy` | `int` | Offset from the parent `BlockData.x/y` anchor |

**Completeness rule (owned by `MergeSolver`, documented here for reference):** a block is a "complete picture" when its fragment count equals `gridCols × gridRows` for its `src`, and every `sliceX/sliceY` combination in that range is present exactly once.

---

## `CategoryDefinitions` (ScriptableObject, singleton config asset)

Canonical list of valid categories — replaces the old prototype's inconsistent ad-hoc strings (see `CATEGORY_TAXONOMY.md` for the actual list and naming rules).

| Field | Type | Notes |
|---|---|---|
| `categories` | `List<CategoryEntry>` | |

### `CategoryEntry`

| Field | Type | Notes |
|---|---|---|
| `id` | `string` | Canonical category key, referenced by `DoorData.category` / `FragmentData.targetCategory` |
| `displayName` | `string` | Player-facing label |
| `icon` | `Sprite` | Door tab icon |

The Level Editor should validate against this list rather than allowing free-text category entry, to prevent the naming drift seen in the original prototype (`FRUIT` vs `FRUITS`, etc.).

---

## `LevelSequence` (ScriptableObject)

| Field | Type | Notes |
|---|---|---|
| `orderedLevelIds` | `List<string>` | Play order, referencing `LevelData.levelId` values. Editable via drag-reorder in the Level Editor tool. |

---

## `GameConfig` (ScriptableObject)

Tuning constants and global references that shouldn't be hardcoded in scripts.

| Field | Type | Notes |
|---|---|---|
| `cellSize` | `float` | Base grid cell size in world units |
| `wallThickness` | `float` | Ported ratio from source (`CELL_SIZE * 0.4`) — confirm against final art before hardcoding |
| `hammerUnlockLevelIndex` | `int` | Index into `LevelSequence` at which Hammer unlocks |
| `rocketUnlockLevelIndex` | `int` | Index into `LevelSequence` at which Rocket unlocks |
| `startingPowerupCounts` | struct/dict | Per-level starting counts for Rocket/Hammer |

---

## Runtime-Only State (not part of `LevelData`, not serialized to assets)

These exist only during play and are rebuilt/discarded per level load — do not add them to the ScriptableObject schema:

- `GridCollisionMap` instance
- Per-block `exitingDoor`, `isNewMerge`, `originalBlockId` tracking (used for Hammer-split and exit-animation bookkeeping, mirrors source engine's runtime-only fields)
- Drag state (pointer ID, velocity, ghost position, etc.)

---

## Save Data Schema (`ProgressData`, see `SAVE_SYSTEM.md` for full detail)

| Field | Type | Notes |
|---|---|---|
| `ftueSeen` | `Dictionary<string, bool>` | Which one-shot tutorial steps have been shown |
| `unlockedLevelIndex` | `int` | Furthest unlocked index into `LevelSequence` |

**No sticker-collection or meta-related fields exist here.** Do not add any without an explicit scope change — see `CLAUDE.md` Non-Goals.

---

## Schema Change Checklist

If you need to add/change a field above:
1. Update this file first.
2. Update `LevelEditorWindow.cs` to expose the new field.
3. Update `LevelLoader` deserialization.
4. Confirm no already-authored `LevelData` assets break (write a migration step if they do — level assets are hand-authored and few in number at this stage, so simple manual fixup may be acceptable; use judgment).
5. Note the change in `CHANGELOG.md`.
