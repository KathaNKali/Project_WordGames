# ARCHITECTURE.md — Sticker's Out

## Layering Principle

The codebase is split into three layers so core logic stays testable and rendering/input stays swappable:

```
┌─────────────────────────────────────────────┐
│  UI / Views (MonoBehaviours, Prefabs)         │  <- renders state, forwards input
│  BlockView, DoorView, HUD, Screens             │
├─────────────────────────────────────────────┤
│  Controllers / Services (MonoBehaviours)       │  <- own game flow, wire events
│  DragController, PowerupController,            │
│  TimerController, WinLoseController,           │
│  FtueController, SaveService, AudioService,     │
│  VfxService                                     │
├─────────────────────────────────────────────┤
│  Core (plain C#, no MonoBehaviour dependency)  │  <- pure logic, unit tested
│  GridModel, GridCollisionMap, BlockModel,       │
│  DoorModel, MergeSolver, LevelLoader            │
└─────────────────────────────────────────────┘
```

**Rule of thumb:** if a class needs `transform`, `GameObject`, or a Unity lifecycle method (`Update`, `OnTriggerEnter`, etc.), it belongs in the Controller or View layer. If it's answering a question about game state ("do these fragments merge?", "is this cell occupied?", "has this door's condition been met?"), it belongs in Core and should be constructible/testable without a scene.

## System Responsibilities

### Core Layer

| System | Responsibility |
|---|---|
| `GridModel` | Grid dimensions, wall cells, void cells — static board layout for the current level |
| `GridCollisionMap` | O(1) lookup matrix (wall / void / block-ID / empty), rebuilt on every state mutation |
| `BlockModel` | A block's grid position + its fragment list (shape) |
| `DoorModel` | Edge, position, span, category, color, ice-count, exiting state |
| `MergeSolver` | Given the current block list, determines which fragments merge (adjacent, same `src`, neighboring `sliceX/sliceY`) and whether a resulting group is a complete picture |
| `LevelLoader` | Deserializes/loads a `LevelData` asset into runtime `GridModel`/`BlockModel`/`DoorModel` instances |

### Controller / Service Layer

| System | Responsibility |
|---|---|
| `DragController` | Reads pointer input, steps a dragged block's position against `GridCollisionMap`, resolves door-exit conditions |
| `PowerupController` | Rocket (remove all fragments matching a word/color target) and Hammer (split a merged block into original source pieces) logic + unlock-state gating |
| `TimerController` | Countdown, triggers lose state on expiry |
| `WinLoseController` | Evaluates win condition (no blocks left matching any active door), triggers win/lose screens |
| `FtueController` | Drives tutorial toasts from a data-driven `TutorialStepDefinition` list — no hardcoded per-level-ID checks in code |
| `SaveService` | Reads/writes the local JSON progress file (FTUE-seen flags, unlocked level index) |
| `AudioService` | Central place to trigger SFX by event name/enum — Views/Controllers call this, never `AudioSource.Play` directly in scattered places |
| `VfxService` | Central place to trigger particle effects by event name/enum, same pattern as `AudioService` |

### View Layer

| System | Responsibility |
|---|---|
| `BlockView` | Renders a `BlockModel` as pooled sprite fragments, animates merge/exit/drag transforms |
| `DoorView` | Renders a `DoorModel` as a door slot + category/ice icon |
| `GridCellView` | Renders static wall/void cells |
| HUD / Screens | Timer display, power-up buttons + counts, start/win/lose screens, tutorial toast |

## Data Flow (Typical Turn)

1. `DragController` reads pointer delta → proposes a new block position.
2. `GridCollisionMap` is queried per-step to allow/deny movement (including the "escaping through an open door" exception).
3. On drag release, `MergeSolver` re-evaluates the board — completed groups get flagged.
4. Completed groups checked against `DoorModel` list for a category/color/ice-state match → block flagged `exitingDoor`.
5. `BlockView` animates the exit; on completion, block is removed from `BlockModel` list, `GridCollisionMap` rebuilt.
6. `WinLoseController` re-checks win condition after every removal.
7. Relevant `AudioService`/`VfxService` events fire alongside each of the above steps — these are **event-driven, not tightly coupled calls** (Core layer never calls Audio/VFX directly; Controllers do, in response to Core results).

## Event-Driven Decoupling

Prefer C# `Action<T>`/`event` (or `UnityEvent` where Inspector wiring helps) over direct references between systems. Example: `MergeSolver` should never call `AudioService.Play(...)` itself — it returns a result, and the Controller that invoked it decides what side effects (audio, VFX, save) follow. This keeps Core logic pure and swappable, and keeps unit tests free of Unity dependencies.

## Explicitly Not Present in This Architecture

Per current project scope (see `CLAUDE.md` for the authoritative list):
- No text/word matching path in `MergeSolver`.
- No sticker-collection/meta save data or events.
- No physics-engine-driven movement.
- No legacy content importer/migration tooling.

If a future task requires any of the above, treat it as a scope change requiring explicit confirmation, not an architecture gap to silently fill in.
