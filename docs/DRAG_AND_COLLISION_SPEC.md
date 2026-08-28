# DRAG_AND_COLLISION_SPEC.md — Sticker's Out (v2, Unity-native design)

**This supersedes the v1 literal-port spec.** v1 extracted the HTML prototype's exact algorithm (substepped pointer-chase, per-pixel margin testing). This version keeps the *feel* that algorithm produced but re-derives the *mechanism* using Unity's own systems. Do not reference v1 for implementation — it's kept in history only as a record of what the original behavior was, in case a feel regression needs to be traced back to "what did the prototype actually do here."

---

## 1. Feel Contract (what must survive the port — this is the actual acceptance criteria)

1. Drag feels elastic/"chunky" — the block follows the pointer with a slight lag/spring, not a rigid 1:1 attach.
2. Dragging diagonally into a wall still lets the block slide along it on the free axis — never a hard stop on both axes at once.
3. Fragments can sit nearly touching without a false-positive collision (small tolerance, not exact-edge-to-edge).
4. A complete picture can exit through a matching, non-iced door when dragged past the boundary; an incomplete one never can, regardless of how far it's dragged.
5. On release, the block snaps to the grid cell it actually resolved to — not to wherever the pointer happened to be.
6. All of the above must hold consistently across device refresh rates (60/90/120Hz), since this ships on a wide spread of Android hardware.

Any implementation that satisfies 1–6 is a valid port. The sections below are the recommended concrete design, not the only possible one — but deviating from them should be a deliberate choice, not an accident.

---

## 2. Board Representation

Grid logic operates entirely in **integer cell space** (`Vector2Int`), never in pixels/world units. World-space is purely a rendering concern, converted at the boundary via a single `GridToWorld` / `WorldToGrid` pair, ideally backed by an actual Unity `Grid` component so Scene-view gizmos and tools work for free during development.

Occupancy is stored as a **flat 2D array** (`CellState[,]`, or a 1D array indexed `y * cols + x` if profiling ever calls for it — not needed at launch grid sizes), rebuilt from the current block list whenever it changes. This is the direct equivalent of the prototype's `collisionMap`, kept because it was already the right idea (O(1) lookup, cheap full-rebuild at this board scale) — the *mechanism* around it (drag stepping) is what's being redesigned, not this part.

```csharp
public enum CellState { Empty, Wall, Void }
// occupancy by block: separate lookup, Dictionary<Vector2Int, int> blockIdAt,
// or an int[,] where 0 = empty and positive values are block IDs — pick one
// and keep GridCollisionMap.cs as the single source of truth either way.
```

---

## 3. Drag Movement — Swept Resolution, Not Substepping

**What the prototype did:** split each frame's movement into ~1px substeps and re-test collision at every substep (`dragLoop`'s `ceil(max(|vx|,|vy|))` loop). This exists specifically to avoid tunneling through walls in a substep-free naive implementation — it's a real problem, just solved with a brute-force loop because that's what was easy in JS.

**What we do instead:** compute the *maximum legal travel distance* directly per axis, per frame — a swept/raycast-style resolution, which is both cheaper (no variable-length inner loop) and more precise (no risk of a substep landing just past a boundary).

```csharp
// Pseudocode — see GridModel/GridCollisionMap for the actual lookup API.
Vector2 ResolveAxisMove(BlockModel block, Vector2 currentPos, float delta, Axis axis, float skinWidth)
{
    if (Mathf.Approximately(delta, 0f)) return currentPos;

    float direction = Mathf.Sign(delta);
    float remaining = Mathf.Abs(delta);

    // Walk cell-by-cell in the direction of travel, checking every fragment's
    // swept footprint against GridCollisionMap (wall/void/other-block), and
    // against the door-exit exception (§4) if the block is complete.
    // Stop at the first blocking boundary, or allow the full `remaining`
    // distance if nothing blocks. Apply `skinWidth` as a small tolerance so
    // adjacent-but-not-merged fragments don't false-collide (replaces the
    // prototype's flat 4px margin — tune as a GameConfig value, not a
    // hardcoded literal).

    float allowed = ComputeMaxTravel(block, currentPos, direction, remaining, axis, skinWidth);
    return axis == Axis.X
        ? currentPos + new Vector2(direction * allowed, 0f)
        : currentPos + new Vector2(0f, direction * allowed);
}
```

**Critically, X and Y are still resolved independently, each against the block's pre-move position on the other axis** — this is the one property from the prototype that must be preserved exactly, because it's *why* wall-sliding works (feel contract #2). Resolving combined diagonal movement as a single swept test would reintroduce the "hard stop on any contact" problem the prototype avoided.

This is O(distance-in-cells) per axis per frame, which for realistic mobile drag speeds on small puzzle boards is a handful of cell checks — cheaper than the prototype's substep loop, and with no variable iteration count to worry about under profiling.

## 4. Door-Exit Exception

Unchanged in *intent* from v1, re-stated for this design:

A block may leave the grid boundary only if:
- It's currently a complete picture (`MergeSolver.IsComplete(block)`).
- A door exists whose `category` (or `color` fallback) matches the block's fragments, with `iceCount == 0`.
- The block's position falls within that door's `position..position+span` range along the edge it's exiting through.

Implement this as an explicit `DoorExitEvaluator.CanExit(block, targetCell, doors)` check, called *from* `ComputeMaxTravel` when a sweep would otherwise be blocked by the grid boundary — keep it a separate, named, unit-testable function rather than inlining door logic into the general collision sweep. This was flagged as a structural fix in v1 and still applies here.

Keep two distinct tolerances, same rationale as v1:
- **Skin width** (§3) — governs whether a sweep step is blocked at all; small, physics-facing.
- **Exit commit threshold** — a `GameConfig`-driven distance past the boundary at which a complete block's drag is treated as committed to exiting (ends the drag early into the exit animation, rather than requiring pixel-perfect drag-to-edge). This is a feel/UX tuning value, not a collision constant — keep it in `GameConfig`, not hardcoded.

## 5. Presentation Layer — Separated From Collision

This is the main structural improvement over v1: **the "elastic chase" feel is not part of collision resolution at all.**

- `DragController` (Controller layer) resolves the block's true grid-legal position every frame via §3, and updates `BlockModel.Position` (or a continuous fractional position while mid-drag, if smooth sub-cell dragging is desired — decide once and document the choice here when implemented).
- `BlockView` (View layer) renders that resolved position through a spring/lerp (e.g. a simple critically-damped spring, or DOTween's `DOMove` retargeted every frame) so the sprite visually trails the resolved position slightly — this is what produces the "chunky" feel, entirely as a presentation concern.
- Tilt/squash-and-stretch ("wiggle" in the prototype) is likewise a pure View-layer effect driven by recent velocity, with zero influence on collision.

This means feel can be retuned (spring stiffness, tilt amount) without touching collision code, and collision can be debugged/tested without any visual layer involved at all — a real gap in the prototype's design, where feel and physics were the same function.

## 6. Release & Snap

```
1. Cancel any active spring/tween on the View.
2. block.Position = the resolved integer cell from the final swept position
   this frame (round to nearest, from the *resolved* position — never from
   raw pointer input).
3. Run MergeSolver.ResolveMerges(scoped to this block) — see MERGE_SOLVER_SPEC.md.
4. Run door-exit evaluation for any newly-complete blocks.
5. Rebuild GridCollisionMap.
6. Emit a state-changed event for the View layer to sync to (see
   ARCHITECTURE.md's event-decoupling rule) — Controller does not reach
   into View internals directly.
```

## 7. Input Handling

Unity Input System, pointer-phase driven (`Began` / `Moved` / `Ended` / `Canceled`). Explicit guard: **only one active drag at a time** — reject a second concurrent touch outright rather than letting it interfere (the prototype had no such guard; this is a deliberate mobile-hardening addition, not a port of anything).

## 8. Framerate Independence

Because §3 computes a legal *distance* per frame rather than a fixed per-frame chase fraction, this design is naturally framerate-appropriate as long as `remaining` is derived from `Time.deltaTime`-scaled pointer velocity (not a fixed per-frame constant). This sidesteps the v1 spec's open question about the JS `0.5` chase factor entirely — there's no equivalent magic constant carried over to begin with. The presentation-layer spring (§5) should use a Unity-idiomatic frame-rate-independent damping (`Vector2.SmoothDamp`, or DOTween with a duration rather than a per-frame multiplier) for the same reason.

## 9. Scenarios to Cover in Tests

Same intent as v1 §6, restated against this design — write these against `DragResolver`/`ComputeMaxTravel` directly, with a fake `GridCollisionMap`, no scene required:

1. Unobstructed drag reaches the full requested distance.
2. Diagonal drag into a wall: blocked axis stops at the wall boundary, free axis reaches full distance (wall-slide).
3. Blocked by another block's occupied cells; not blocked by its own cells.
4. Incomplete picture is rejected at the grid boundary even when swept far past it.
5. Complete picture is allowed past the boundary only at a matching, non-iced door's position/span range; rejected elsewhere on the same edge.
6. Iced door blocks exit until `iceCount` reaches 0.
7. Skin-width tolerance allows near-touching fragments without falsely blocking a sweep.
8. Snap-on-release uses the resolved position, confirmed via a case where the pointer target was inside a wall.
9. (Once presentation spring is implemented) confirm collision-layer tests pass with zero dependency on the View/spring code — proves the separation in §5 actually holds.
