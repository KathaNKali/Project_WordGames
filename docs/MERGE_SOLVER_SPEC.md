# MERGE_SOLVER_SPEC.md — Sticker's Out (v2, Unity-native design)

**This supersedes the v1 literal-port spec.** The grouping/adjacency *algorithm* in v1 was already sound (it's genuine puzzle logic, not a browser workaround), so it's kept largely as-is here. What changes is the *data substrate*: string-keyed dictionaries and per-call allocations are replaced with flat-array lookups and pooled collections, because `MergeSolver` runs on every drag release and GC pauses are far more visible on mobile than they were in a browser tab.

---

## 1. Feel/Correctness Contract

1. Two fragments of the same picture merge only when they're both grid-adjacent *and* their slice coordinates are sequential neighbors — never merge non-adjacent slices even if the picture would "look right."
2. A block reports complete only when every slice of its picture is present exactly once.
3. Merging is scoped to the block that was just moved — blocks elsewhere on the board don't spontaneously merge on someone else's turn.
4. After any merge/split, every resulting block is spatially connected (no block silently represents two disconnected clusters of fragments).
5. Hammer splits strictly by *original source block*, never by current spatial adjacency.
6. Rocket removes a specific target's fragments board-wide, and everything left over re-settles into valid connected blocks.
7. Win state is exact: true precisely when no remaining fragment matches any remaining door.

---

## 2. Data Types (Unity-native)

```csharp
namespace StickersOut.Core
{
    [System.Serializable]
    public struct FragmentData
    {
        public string PictureId;      // which source picture this fragment belongs to
        public Vector2Int Slice;      // position within that picture's fragment grid
        public Vector2Int SliceCount; // total (cols, rows) the picture is sliced into
        public string Category;
        public string Color;
        public int OriginalBlockId;   // permanent provenance, set once at level load
        public Vector2Int Offset;     // dx,dy relative to the owning block's anchor
    }

    public class BlockModel
    {
        public int Id;
        public Vector2Int Position;             // anchor cell
        public List<FragmentData> Fragments;
        public bool IsNewMerge;
        public DoorEdge? ExitingDoor;

        public Vector2Int AbsoluteCell(FragmentData f) => Position + f.Offset;
    }
}
```

No string-interpolated dictionary keys (the prototype's `` `${x},${y}` `` pattern) anywhere in this system — `Vector2Int` is a legitimate, hashable, allocation-free dictionary key in C#/Unity, and is what backs any adjacency lookup here.

---

## 3. Adjacency Detection

Two fragments are merge-adjacent when **all** of:
- Same `PictureId`.
- Grid-adjacent: their absolute cells differ by exactly one step on a single axis (`(1,0)`, `(-1,0)`, `(0,1)`, or `(0,-1)`).
- Slice-sequential in the matching direction: e.g. for a horizontal pair, `right.Slice.x == left.Slice.x + 1 && right.Slice.y == left.Slice.y`; analogous for vertical.

Build this once per resolve pass as a `Dictionary<Vector2Int, FragmentData>` (rented from a pool, see §6) keyed by absolute cell, covering only non-exiting blocks — this is the direct, allocation-conscious equivalent of the prototype's `letterGrid`.

---

## 4. Merge Resolution (`MergeSolver.ResolveMerges`)

```
1. Rent an absolute-cell -> fragment lookup from the pool; populate from all
   non-exiting blocks.
2. Find all adjacent pairs per §3 ("segments").
3. Union pairs into groups (union-find or iterative merge-by-shared-cell,
   same approach as v1 — this part of the algorithm doesn't need to change).
4. For each resulting group:
   a. All tiles from the same block, and that block's fragment count already
      equals the group size -> no-op, skip.
   b. Not complete (per IsComplete, §5) and all tiles from a single block
      -> skip, nothing to do.
   c. Otherwise: remove these fragments from their old parent block(s),
      create a new BlockModel from the group (fresh dx/dy relative to the
      group's own min cell), flag IsNewMerge = true. If complete, include
      it in the result's "newly completed" list (Core does not trigger
      audio/VFX itself — see ARCHITECTURE.md).
5. Global re-normalization: for every block touched by this pass (not
   necessarily the whole board — track which blocks actually changed and
   only re-run connectivity splitting on those, which is a real efficiency
   improvement over v1's "re-check everything every time"), flood-fill by
   4-directional Offset adjacency to find connected components, split into
   one block per component, reassign IDs, carry forward IsNewMerge/ExitingDoor.
6. Return every affected block (created, mutated, removed) as an explicit
   result object — Controllers/Views diff against this, not against a
   full-board re-render.
```

**Optimization over v1, not just a translation:** v1's source re-ran the connectivity/ID-reassignment pass over *every* block on the board on every merge, regardless of whether that block was touched. Since we're tracking which blocks were actually mutated in step 4, step 5 only needs to re-normalize those — meaningfully cheaper on boards with many untouched blocks, with identical output.

---

## 5. Completeness Check

```csharp
public static bool IsComplete(BlockModel block)
{
    if (block.Fragments.Count == 0) return false;
    var pictureId = block.Fragments[0].PictureId;
    var sliceCount = block.Fragments[0].SliceCount;
    if (block.Fragments.Count != sliceCount.x * sliceCount.y) return false;

    // Every fragment shares the picture, and every slice coordinate in the
    // range is present exactly once — check via a pooled HashSet<Vector2Int>
    // rather than repeated O(n^2) scanning.
    foreach (var f in block.Fragments)
        if (f.PictureId != pictureId) return false;

    return AllSlicesPresent(block.Fragments, sliceCount); // pooled HashSet check
}
```

## 6. Pooling

Every collection used inside a resolve pass (`ResolveMerges`, `IsComplete`, flood-fill scratch lists/queues) is rented from `UnityEngine.Pool.ListPool<T>` / `UnityEngine.Pool.HashSetPool<T>` (or a small custom pool if a needed collection type isn't covered) and returned via `using var pooled = ...` / explicit `Release()` at the end of the call. This is a direct, Unity-native answer to the GC-pressure concern flagged earlier — no hand-rolled object pool needed, Unity already ships one.

---

## 7. Door-Exit Check After Merge, Whole-Board Door Sweep, Win Condition

Unchanged in behavior from v1 §5–§7 — these were never performance- or DOM-coupled concerns, just rule logic:

- **Door-exit check:** only newly-merged, now-complete blocks are checked against doors (category/color + `iceCount == 0` + position/span range).
- **Whole-board door sweep:** a door is removed once no remaining block has any fragment matching it and its `iceCount` is 0 — evaluated after any change that could reduce matching fragments (block exit completing, Rocket use).
- **Win condition:** true exactly when no remaining block has any fragment matching any remaining door.

---

## 8. Power-Ups

### 8.1 Rocket — `BlastTarget(pictureId, category, color)`
Removes every fragment across every block matching the target. Remaining fragments per affected block re-split via the same connectivity flood-fill as §4 step 5 (scoped to affected blocks only, same optimization). Decrements `iceCount` — **see §9, same discrepancy as v1, same recommendation (Option B: scope to matching category/color).**

### 8.2 Hammer — `SplitByOriginalBlockId(blockId)`
Groups a block's fragments by `OriginalBlockId` (not spatial adjacency), producing one new block per distinct original source. Rejected if the target block has only one distinct `OriginalBlockId` (never actually merged). Does not itself trigger a re-merge or door-exit check afterward — same as v1, preserved deliberately (split pieces sit until the player interacts with them).

---

## 9. ⚠️ Same Discrepancy as v1 — Still Needs a Decision

Restating from v1 since it's unaffected by the mechanism change: the prototype decrements ice on **all** iced doors on any normal block exit, but only on **matching** doors for Rocket. This is inconsistent and looks like an unintentional bug rather than a design choice.

**Recommendation unchanged: Option B — scope both mechanics identically (matching category/color only).** This still needs sign-off from whoever owns level difficulty/tuning before implementation, since it changes how fast ice doors clear relative to the shipped prototype's behavior.

---

## 10. Scenarios to Cover in Tests

Same list as v1 §10, plus two new ones specific to this design:

1–10. *(unchanged from v1 — adjacency correctness, non-adjacent-slice rejection, completeness detection, cross-block merge correctness, connectivity splitting, Rocket scoping, Hammer's provenance-based grouping, win condition, and the §9 decision, whichever option is chosen)*

11. **Pooling correctness:** running `ResolveMerges` repeatedly (e.g. 1000 iterations in a stress test) produces identical results each time and does not leak pooled collections (verify via a test that asserts pool rent/return counts balance, or via a GC-alloc assertion using Unity's `Assert.That(..., Is.AllocatingGCMemory())`-style tooling).
12. **Scoped re-normalization correctness:** construct a board with several untouched blocks and one merge event; confirm only the affected blocks are touched by the connectivity pass (e.g. via reference-equality checks that untouched `BlockModel` instances are literally the same object afterward, not just equal in value) — this is the regression test for the §4 step 5 optimization.
