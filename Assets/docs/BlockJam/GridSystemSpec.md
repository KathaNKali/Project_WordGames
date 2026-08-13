# Grid System Specification — Block Jam: Associations

> **Status:** In Progress
> **Version:** 0.1
> **Last Updated:** 2026-08-12
> **Depends On:** `GDD.md`, `LevelDataSchema.md`

---

## Task List

| # | Task | Status |
|---|---|---|
| 1 | Define Grid Data Model (`GridCell`, `GridConfig`) | ⬜ Not Started |
| 2 | Build `GridManager` | ⬜ Not Started |
| 3 | Build `GridCoordinateUtil` | ⬜ Not Started |
| 4 | Build `GridVisualizer` | ⬜ Not Started |
| 5 | Block Placement on Grid at Level Load | ⬜ Not Started |
| 6 | Block Movement Validation (single cell) | ⬜ Not Started |
| 7 | Multi-Cell Block Support (irregular shapes) | ⬜ Not Started |
| 8 | Grid State Debug Tools | ⬜ Not Started |
| 9 | Unit Tests | ⬜ Not Started |

---

## Overview

The Grid System is the **foundation of all gameplay**. It is split into two layers that must always stay in sync:

| Layer | Description |
|---|---|
| **Logical Grid** | An in-memory `GridCell[,]` 2D array. The single source of truth for all gameplay state. |
| **Visual Grid** | The 3D factory floor rendered in the scene. Driven entirely by the logical grid — no independent state. |

### Key Design Decisions
- Grid lies on the **XZ plane**. Blocks move horizontally and vertically only.
- **Void cells** are explicitly defined in `GridConfig` as `List<Vector2Int>`.
- All non-void cells are **playable** by default — no need to list them.
- Void cells have **no floor tile rendered** and are **impassable** to blocks.
- This gives the grid an irregular shape both visually and logically from a single data source.
- Block movement is **kinematic** — no physics, purely logic-driven grid snapping.

---

## Architecture

```
GridConfig (ScriptableObject)
        ↓
GridManager  ←—→  GridCoordinateUtil
        ↓
GridVisualizer  (reads GridManager, no logic)
        ↓
Block  (registers / unregisters with GridManager)
```

---

## Component Breakdown

---

### TASK 1 — Grid Data Model

**Files:** `GridCell.cs`, `GridConfig.cs`

#### `GridCell` — Pure Data Class (not MonoBehaviour)

| Field | Type | Description |
|---|---|---|
| `gridPosition` | `Vector2Int` | This cell's coordinate on the grid |
| `isOccupied` | `bool` | True if a block occupant is present |
| `isVoid` | `bool` | True if this cell is non-playable (no tile, impassable) |
| `occupant` | `Block` | Reference to occupying block; null if empty |

> `isVoid = true` means: no floor tile is rendered, and `CanMoveTo()` returns false for this cell.

#### `GridConfig` — ScriptableObject

| Field | Type | Description |
|---|---|---|
| `width` | `int` | Number of columns |
| `height` | `int` | Number of rows |
| `cellSize` | `float` | World-unit size of each cell (e.g., 1.0f) |
| `voidCells` | `List<Vector2Int>` | Explicit list of void (non-playable) cell positions |

**Void Cell Rules:**
- All cells default to **playable** on grid initialization.
- Cells listed in `voidCells` are marked `isVoid = true` after init.
- Void cells receive **no floor tile** from `GridVisualizer`.
- Void cells are always impassable regardless of `isOccupied`.

**Example — Irregular Grid (5x4 with corner voids):**
```
voidCells: [ (0,3), (1,3), (3,0), (4,0) ]

Visual result:
[V][V][ ][ ][ ]       [ ] = playable (floor tile)
[ ][ ][ ][ ][ ]       [V] = void (no tile, impassable)
[ ][ ][ ][ ][ ]
[ ][ ][ ][V][V]
```

---

### TASK 2 — GridManager

**File:** `GridManager.cs`
**Type:** Singleton MonoBehaviour

#### Initialization
```
Awake():
  1. Create GridCell[width, height] array
  2. Initialize all cells as playable (isVoid = false, isOccupied = false)
  3. Iterate GridConfig.voidCells → mark each as isVoid = true
```

#### Public API

| Method | Returns | Description |
|---|---|---|
| `IsCellValid(Vector2Int pos)` | `bool` | In bounds AND not void |
| `IsCellEmpty(Vector2Int pos)` | `bool` | Valid AND not occupied |
| `CanMoveTo(Vector2Int pos)` | `bool` | Valid AND not void AND not occupied |
| `RegisterBlock(Block block, Vector2Int pos)` | `void` | Marks cell occupied, sets occupant |
| `UnregisterBlock(Vector2Int pos)` | `void` | Marks cell empty, clears occupant |
| `GetBlockAt(Vector2Int pos)` | `Block` | Returns occupant or null |
| `GetCell(Vector2Int pos)` | `GridCell` | Returns the raw GridCell data |

#### Rules
- `RegisterBlock` must always be paired with `UnregisterBlock` on move.
- Never register a block onto a void cell.
- For multi-cell block groups, call `RegisterBlock` / `UnregisterBlock` for **each occupied cell individually**.

---

### TASK 3 — GridCoordinateUtil

**File:** `GridCoordinateUtil.cs`
**Type:** Static utility class (no MonoBehaviour)

#### Methods

| Method | Description |
|---|---|
| `Vector3 GridToWorld(Vector2Int gridPos)` | Returns XZ world position at the center of the given cell |
| `Vector2Int WorldToGrid(Vector3 worldPos)` | Returns nearest grid cell for a given world position |
| `bool IsOnGrid(Vector2Int pos)` | Bounds check only (does not check void) |

#### Notes
- `GridToWorld` maps `(col, row)` → `(col * cellSize, 0, row * cellSize)` + grid origin offset.
- `WorldToGrid` uses `Mathf.RoundToInt` on X and Z axes divided by `cellSize`.
- Y axis is always 0 (XZ plane) — blocks do not move vertically.
- Grid origin is configurable (default: world origin `Vector3.zero`).

---

### TASK 4 — GridVisualizer

**File:** `GridVisualizer.cs`
**Type:** MonoBehaviour

#### Behavior
- Reads `GridManager` state on level load.
- **Only spawns floor tiles for non-void cells.**
- Void cells are skipped entirely — no tile, no placeholder, no invisible object.

#### MVP (No Art)
- Draw grid cells using **Unity Gizmos** (Editor only).
- Color code:
  - 🟩 Green = playable + empty
  - 🟥 Red = playable + occupied
  - ⬛ Black / hidden = void (not drawn)
  - 🟦 Blue = valid drop target (when dragging a block)

#### Later (Art Pass)
- Swap Gizmos for actual **floor tile prefab** instantiated at each playable cell.
- Floor tile prefab is set in `GridVisualizer` inspector — swap without touching code.
- Void cells remain empty (no tile instantiated).

#### Rules
- `GridVisualizer` has **zero gameplay logic**.
- It never modifies `GridManager` state.
- It reacts to grid changes via events from `GridManager` (e.g., `OnGridUpdated`).

---

### TASK 5 — Block Placement on Grid at Level Load

**File:** `LevelLoader.cs` (references `GridManager`, `GridCoordinateUtil`)

#### Flow
```
1. Read block spawn data from LevelData ScriptableObject
2. For each block:
   a. Instantiate Block prefab
   b. Set Transform.position = GridCoordinateUtil.GridToWorld(spawnGridPos)
   c. Call GridManager.RegisterBlock(block, spawnGridPos)
3. For merged groups (Tangled Blocks):
   a. Instantiate as group
   b. Register each occupied cell individually
```

#### Rules
- Blocks are **never spawned on void cells**.
- Level data must be validated at authoring time (Level Editor) to prevent void-cell spawns.

---

### TASK 6 — Block Movement Validation (Single Cell Block)

**File:** `BlockMover.cs` or inside `Block.cs`

#### Flow (per move attempt)
```
1. Player swipes in a direction (Up / Down / Left / Right)
2. Calculate targetPos = currentGridPos + directionVector
3. Call GridManager.CanMoveTo(targetPos)
4. If true:
   a. GridManager.UnregisterBlock(currentGridPos)
   b. Transform.position = GridCoordinateUtil.GridToWorld(targetPos)
   c. GridManager.RegisterBlock(this, targetPos)
   d. currentGridPos = targetPos
5. If false:
   a. Reject move (optional: play bump feedback)
```

#### Direction Vectors (XZ plane)
| Input | Grid Delta |
|---|---|
| Up (forward) | `(0, +1)` |
| Down (back) | `(0, -1)` |
| Left | `(-1, 0)` |
| Right | `(+1, 0)` |

---

### TASK 7 — Multi-Cell Block Support (Irregular Shapes)

**File:** `BlockGroup.cs` or extended `Block.cs`

#### Data
- `List<Vector2Int> occupiedCells` — relative offsets from the group's pivot cell.
- **Only actually occupied cells are listed.** Empty cells within the bounding box are NOT included and remain passable.

#### Movement Validation for Groups
```
1. For each cell in occupiedCells:
   a. Calculate candidatePos = pivotGridPos + offset + directionVector
   b. Call GridManager.CanMoveTo(candidatePos)
      — Exception: skip check if candidatePos is currently occupied by THIS group
2. If ALL candidate positions are valid → allow move
3. If ANY candidate position is blocked → reject move
```

#### Example — L-Shaped Block (3 cells)
```
Bounding box (2x2):      Occupied cells (relative to pivot):
[ A ][ B ]               occupiedCells = { (0,0), (1,0), (0,1) }
[ C ][   ]               Empty cell (1,1) is NOT registered → passable
```

#### Registration
- On spawn: `RegisterBlock` for each cell in `occupiedCells`.
- On move: `UnregisterBlock` all, move pivot, `RegisterBlock` all new positions.
- On merge: rebuild `occupiedCells` list, re-register all cells.

---

### TASK 8 — Grid State Debug Tools

#### Console Debug — ASCII Grid Print
- Editor button or keyboard shortcut triggers `GridManager.PrintGridState()`.
- Outputs full grid as ASCII map:
```
  0  1  2  3  4
0 [V][V][ ][ ][ ]
1 [ ][ ][B][ ][ ]
2 [ ][B][B][ ][ ]
3 [ ][ ][ ][V][V]

Legend: [V]=Void  [B]=Block  [ ]=Empty
```

#### Scene View Gizmos
- `GridVisualizer` draws colored wireframe cubes per cell in Scene view.
- Toggle via `GridVisualizer.showDebugGizmos` bool in Inspector.

---

### TASK 9 — Unit Tests

**File:** `Tests/GridSystemTests.cs` (Unity Test Framework — EditMode)

| Test | Description |
|---|---|
| `Test_IsCellValid_InBounds` | Valid cell returns true |
| `Test_IsCellValid_OutOfBounds` | Out-of-bounds returns false |
| `Test_VoidCell_IsImpassable` | Void cell fails `CanMoveTo` |
| `Test_VoidCell_NoOccupant` | Cannot register block on void cell |
| `Test_RegisterUnregister_Consistency` | Register then unregister leaves cell empty |
| `Test_CanMoveTo_OccupiedCell` | Occupied cell fails `CanMoveTo` |
| `Test_GridToWorld_RoundTrip` | `GridToWorld → WorldToGrid` returns original position |
| `Test_IrregularShape_PassableEmptyCell` | Empty bounding box cell is passable |
| `Test_GroupMove_AllCellsValid` | Group move allowed only if all target cells valid |
| `Test_GroupMove_OneBlocked` | Group move rejected if any target cell blocked |

---

## Definition of Done

- [ ] Any grid size initializes correctly from `GridConfig`
- [ ] Void cells are impassable and have no floor tile
- [ ] Blocks register on spawn and unregister on clear
- [ ] Multi-cell irregular shapes register only their occupied cells
- [ ] Empty bounding box cells of irregular shapes are passable
- [ ] Movement validation works for single and multi-cell blocks
- [ ] Full grid state is printable as ASCII in console
- [ ] Scene view Gizmos show grid state during development
- [ ] All unit tests pass

---

## Recommended Build Order

```
Task 1 (Data Model)
  → Task 2 (GridManager)
    → Task 3 (CoordinateUtil)
      → Task 5 (Block Placement)
        → Task 6 (Single Cell Movement)
          → Task 4 (Visualizer)
            → Task 7 (Multi-Cell / Irregular)
              → Task 8 (Debug Tools)
                → Task 9 (Unit Tests)
```
