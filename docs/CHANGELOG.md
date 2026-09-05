# CHANGELOG.md — Sticker's Out

Dated log of scope, schema, and architecture decisions. Add an entry whenever a change ripples across docs (`DATA_MODEL.md`, `ARCHITECTURE.md`, `PROJECT_STRUCTURE.md`) so Claude/Copilot can quickly see *why* the code looks the way it does without re-deriving it from diffs.

Newest entries at the top.

---

## Unreleased

### Added
- Portrait HUD layout support: `GridStageConfig.topHudRatio`/`bottomHudRatio` (fractions of full screen height, default 0.2/0.2, i.e. 60% center game area), with a derived `GameAreaRatio` and `OnValidate` clamping if the two ratios leave too little room. `CameraRigController.Refit()` now applies a normalized `Camera.rect` for this center band before computing framing, so `CameraFramingCalculator` uses the band's own aspect ratio instead of the full screen's.
- `GridStageConfig` (ScriptableObject, `Assets/_Project/Scripts/Core/GridStageConfig.cs`): fixed `designAreaWidth`, `designAreaHeight`, and `margin` used to derive adaptive per-level cell size. Default asset at `Assets/_Project/ScriptableObjects/GridStageConfig.asset`.
- `GridModel` (`Assets/_Project/Scripts/Core/GridModel.cs`): pure C# grid model with adaptive `CellSize`/`WorldWidth`/`WorldHeight`, wall/void cell state, and `GridToWorld`/`WorldToGrid` conversions. Two constructors: raw dimensions or `GridStageConfig`-driven.
- `CameraFramingCalculator` (`Assets/_Project/Scripts/Core/CameraFramingCalculator.cs`): pure orthographic camera-fit math (`Calculate(...)` returns a `CameraFraming` struct with orthographic size + center), fit-by-width vs fit-by-height.
- `CameraRigController` (`Assets/_Project/Scripts/Gameplay/CameraRigController.cs`): Gameplay-layer camera owner; forces orthographic mode, applies framing via `LoadLevel`/`Refit`.
- `GridDebugView` and `GameplayTestBootstrap` (`Assets/_Project/Scripts/Gameplay/`): temporary, debug-only Gizmo visualization and scene wiring for manual validation. **Not** part of the shipped rendering pipeline — replace with real block/grid views.
- `Assets/_Project/Scenes/Gameplay.unity`: manual validation scene wiring `CameraRigController` + `GridDebugView` + `GameplayTestBootstrap` to the default `GridStageConfig` asset.
- EditMode tests: `GridModelTests.cs`, `CameraFramingCalculatorTests.cs` under `Assets/_Project/Tests/EditMode/`.
- `StickersOut.Core.asmdef` (Core assembly) and `StickersOut.Tests.EditMode.asmdef` (Editor-only test assembly referencing Core + TestRunner).
- `docs/CHANGELOG.md` (this file) and `.github/copilot-instructions.md` (points Copilot at `docs/CLAUDE.md`).

### Changed
- `docs/ARCHITECTURE.md`: documented `GridModel`, `CameraFramingCalculator`, and `CameraRigController` under the Core/Controller layering.
- `docs/PROJECT_STRUCTURE.md`: added the new scripts, asmdefs, scene, and temporary debug/bootstrap files to the folder map.
- `docs/DATA_MODEL.md`: added `GridStageConfig` schema section under `GridData`; flagged that `GameConfig.cellSize` is superseded by `GridStageConfig`-derived adaptive cell sizing pending a follow-up reconciliation.

### Notes / Open Items
- HUD Canvas (Top/Bottom panels) must be built manually in the Unity Editor as a Screen Space - Overlay `Canvas` with anchors matching `topHudRatio`/`bottomHudRatio` — this is not code-driven yet. Top HUD content is still undecided (placeholder only); Bottom HUD reserved for power-up buttons (not yet implemented).
- `GridDebugView` / `GameplayTestBootstrap` are temporary scaffolding for manually verifying camera framing before real rendering (block/fragment views) exists — remove once superseded.
- `GameConfig.cellSize` (per `DATA_MODEL.md`) has not been reconciled with the new `GridStageConfig`-driven adaptive sizing; decide whether to remove, repurpose, or keep both for different systems (e.g. wall thickness ratio).
- Manual verification in the Unity Editor (Play Mode, EditMode test run) is still pending — was not run in this environment because no Unity-generated solution was loaded.
