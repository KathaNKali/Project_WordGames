# Game Design Document: Block Jam — Associations (Picture Edition)

> **Status:** In Progress
> **Version:** 0.1
> **Last Updated:** 2026-08-12

---

## Task List

| # | Task | Status |
|---|---|---|
| 1 | Define core gameplay loop | ✅ Done |
| 2 | Define core mechanics | ✅ Done |
| 3 | Define advanced mechanics | ✅ Done |
| 4 | Define power-up system | ✅ Done |
| 5 | Define gate system | ✅ Done |
| 6 | Define merge & collision rules | ✅ Done |
| 7 | Define meta-game structure | 🔄 In Progress |
| 8 | Define art style | 🔄 In Progress |
| 9 | Define audio design | 🔄 In Progress |
| 10 | Define monetization model | 🔄 In Progress |
| 11 | Finalize factory meta details | ⬜ Not Started |
| 12 | Finalize sticker book details | ⬜ Not Started |
| 13 | Finalize ad network selection | ⬜ Not Started |

---

## 1. Summary

**Title:** Block Jam: Associations (Picture Edition)
**Genre:** Hybrid-Casual Puzzle / Tile-Matching
**Platform:** Android (Primary), WebGL (Secondary), iOS (Planned)
**Target Audience:** Broad casual audience (ages 8+). Appeals to fans of Screw Jam, Block Jam 3D, and classic Mahjong or jigsaw mechanics.
**Engine:** Unity 6 (6000.3.21f1), URP
**Project Folder:** `Games/BlockJam/`

### Core Hook
A tactile, 3D grid-based puzzle game where players merge slices of fragmented images to reconstruct whole objects, then slide them into category-specific gates to clear the board. Completed pictures are manufactured into stickers and collected in a themed sticker-book meta-game.

---

## 2. Gameplay Loop

### Core Loop
1. **Identify** — Analyze the board to find scattered image slices belonging to the same picture.
2. **Interact** — Drag and slide image slices along the grid (XZ plane) to position them.
3. **Merge** — Adjacent slices in correct relative positions automatically snap into a single rigid group.
4. **Execute** — Slide the completed merged picture toward its matching category gate on the board edge.
5. **Clear** — The gate magnetically pulls in the picture when it is close enough, clears it from the board, and triggers a sticker fly-out VFX.
6. **Progress** — Repeat until all gates are satisfied and the board is cleared.

---

## 3. Core Mechanics

### Grid System
- All blocks exist on a **3D XZ plane** grid (top-angled camera perspective).
- Blocks are **resized cubes** with the picture image applied as a material on the top face.
- Blocks move **horizontally and vertically** along the grid only (no diagonal movement).
- Movement is **kinematic** — no physics simulation, purely logic-driven snapping.
- The grid surface is **visible** and themed to a factory aesthetic (exact art TBD).

### Block & Slice System
- Each picture is divided into **variable slices** per level (not fixed 2x2 or 3x3 — can be uneven, e.g., 2x4, 1x3).
- Completed pictures can form **irregular, non-rectangular shapes** (e.g., an L-shape or T-shape).
- Each slice carries: `pictureID`, `slicePosition (Vector2Int)`, `currentGridPosition`.

### Collision System
- Collision uses the **actual occupied cells** of a picture shape — not the full bounding box.
- **Empty cells within the bounding box are passable** by other blocks, enabling interesting puzzle interactions with irregular shapes.
- Once two or more slices merge, they become a **single rigid group** and move together as one unit.
- Individual slices **cannot be moved independently** after merging.

### Merge System
- When a slice is dropped adjacent to another slice of the **same picture** in the **correct relative position**, they automatically snap and merge.
- Merging is visual and immediate — slices snap together as a single 3D block group.
- Partial merges are allowed (e.g., 2 of 4 slices merged, waiting for the remaining 2).

### Gate System
- Gates are **walls with a hole** positioned on the edges of the grid.
- Each gate has a **required category** (e.g., Food, Animals, Vehicles).
- Gates may be **color-coded** and display a **category label or icon** (exact design TBD).
- When a completed picture group is slid **close enough** to a matching gate, it is **magnetically sucked in**.
- A picture is only accepted if:
  1. All slices are merged in the correct configuration.
  2. The picture's category matches the gate's required category.
- On acceptance: clear VFX plays, sticker fly-out animation triggers.

### Sticker Fly-Out
- When a picture clears a gate, a sticker version of the completed image appears and **flies to a TBD destination** (sticker book location or HUD icon).
- This is purely visual — actual sticker placement in the meta-game happens outside the gameplay scene.

---

## 4. Advanced Level Mechanics

| Mechanic | Unlock Level | Description |
|---|---|---|
| **Tangled Blocks** | Level 8+ | Slices start pre-merged in incorrect configurations or mixed with other pictures. Requires Hammer power-up or strategic unmerging logic. |
| **Ice Doors** | Level 16+ | Gates are frozen. Players must clear a specific number of valid pictures to chip away ice before the gate becomes usable. |

---

## 5. Power-Ups

| Power-Up | Unlock Level | Description |
|---|---|---|
| **Hammer** | Level 5 | Tap a merged group to shatter it back into individual unmerged slices. Essential for Tangled Blocks. |
| **Rocket** | Level 10 | Tap any slice to instantly clear all slices of that picture from the board, bypassing gate requirements. |

- Power-ups are **UI buttons** displayed at the bottom of the screen.
- Power-ups are **consumable** and refilled via soft currency or rewarded ads.

---

## 6. Monetization

- **Model:** Free-to-play with IAP + Ads
- **Soft Currency:** Earned in-game; spent on:
  - Power-up refills
  - Factory decorations (cosmetic)
  - Sticker-book customization
  - Optional progression conveniences
- Soft currency is **not required** for core level progression.
- **Ad Network:** TBD (Unity Ads / AdMob / Mediation)
- **IAP:** Currency packs, power-up bundles
- **Social Features:** Require network connection (leaderboards, etc. — scope TBD)

---

## 7. Meta-Game — Factory & Sticker Book

> ⚠️ **Status: TBD — Details to be finalized in a future design sprint.**

### Concept
A themed **sticker-book collection meta** with **light factory progression**. Players complete puzzle levels to manufacture stickers, then place them into themed sticker-book pages.

### Meta Loop (Draft)
1. Complete a puzzle level → picture clears gate → sticker produced.
2. Sticker flies to the factory/sticker book.
3. Place sticker in the correct themed sticker-book page.
4. Complete a full sticker-book page → unlock new themes, factory areas, content.

### Factory Areas
- Separate zones/rooms tied to sticker themes (e.g., Animal Wing, Food Zone).
- Upgradeable and expandable — exact mechanics TBD.
- **Factory decorations** purchasable with soft currency (cosmetic; light gameplay effects TBD).

### Sticker Book
- Themed pages with fixed sticker slots — stickers snap into designated positions.
- Completing pages unlocks: new chapters, picture themes, factory areas, special mechanics, obstacles, power-ups.
- Sticker fly destination (HUD icon vs. direct book screen) — TBD.

---

## 8. Controls

- **Primary:** Touch / Drag & Drop (mobile), Mouse drag (WebGL)
- **Block Movement:** Single-finger swipe / mouse drag to slide blocks on the XZ grid
- **Power-Up Use:** Tap UI button, then tap target block
- **Feedback:** Particle burst on merge snap, audio cues on pickup/drop/merge/gate clear

---

## 9. Art Style

> ⚠️ **Full art pipeline TBD. All assets designed to be easily swappable via ScriptableObjects.**

- **Style:** Chunky 3D, highly tactile, vibrant. Supercell-inspired UI.
- **Blocks:** Resized cubes — thick, glossy top face showing picture slice.
- **Grid Surface:** Visible factory floor (exact material/theme TBD).
- **Gates:** Walls with a hole; color-coded by category (exact palette TBD).
- **Background:** Dark, contrasting backdrop to make blocks pop.
- **Typography:** *Lilita One* — chunky, readable, mobile-friendly.
- **VFX:** Particle bursts on merge, gate clear, sticker fly-out, level complete.
- **Asset Swapping:** All picture images mapped via `PictureAssetConfig` ScriptableObject (picture ID → Sprite). Swap art without touching code.

---

## 10. Audio Design

> ⚠️ **Full audio pipeline TBD. All audio triggers are wired and ready for asset swapping.**

- **Engine:** Unity AudioSource + `AudioConfig` ScriptableObject (enum/string key → AudioClip).
- **Trigger points are implemented in code** — swap AudioClip assets in `AudioConfig` without touching code.

| Trigger | Description |
|---|---|
| Block Pickup | Short, light rising tone |
| Block Drop | Low, weighty falling tone |
| Slice Merge / Snap | Satisfying mid-tone click |
| Gate Clear / Exit | Melodic rewarding chime |
| Power-Up Activate | Distinct per power-up |
| Level Complete | Ascending arpeggio |
| Meta — Sticker Placed | Soft satisfying pop |
| Meta — Page Complete | Celebratory fanfare |

---

## 11. Technical Overview

> Full details in `TDD.md`.

| Spec | Value |
|---|---|
| Engine | Unity 6 (6000.3.21f1) |
| Render Pipeline | URP |
| Grid Plane | XZ |
| Block Physics | Kinematic |
| Level Data | ScriptableObjects |
| Primary Platform | Android |
| Secondary Platform | WebGL |
| Planned Platform | iOS |
| Network Required | Yes (Ads, IAP, Social) |
| Offline Play | Not supported |
