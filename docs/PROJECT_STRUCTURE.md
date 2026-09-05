# PROJECT_STRUCTURE.md — Sticker's Out

## Folder Layout

```
Assets/
  _Project/                      // all custom project content lives under here,
                                  // keeps it separated from imported third-party packages
    Art/
      Sprites/Fragments/         // picture puzzle fragment sheets, organized per category subfolder
      Sprites/Doors/             // door slot art + category icons
      Sprites/UI/                // buttons, HUD frames, screens
      Fonts/
    Audio/
      SFX/                       // short authored clips: pickup, drop, merge, ice-break, exit, win
      Music/                     // optional background loop(s)
    Prefabs/
      Gameplay/
        BlockView.prefab
        DoorView.prefab
        GridCellView.prefab
        ParticleFX_Sparkle.prefab
        ParticleFX_Confetti.prefab
      UI/
        HUD.prefab
        StartScreen.prefab
        WinScreen.prefab
        LoseScreen.prefab
        TutorialToast.prefab
    Scenes/
      Boot.unity                 // bootstraps services, loads MainMenu
      MainMenu.unity
      Gameplay.unity             // currently wired with Main Camera (CameraRigController) + GridDebug/Bootstrap test objects, pending real LevelLoader integration
    ScriptableObjects/
      Levels/                    // one LevelData asset per level, naming: Level_<levelId>.asset
      LevelSequence.asset
      CategoryDefinitions.asset
      GameConfig.asset
      GridStageConfig.asset      // fixed design-area width/height + camera margin, see DATA_MODEL.md
    Scripts/
      Core/
        StickersOut.Core.asmdef // own assembly so Core is referenceable by EditMode tests without depending on Assembly-CSharp
        GridModel.cs
        GridStageConfig.cs
        CameraFramingCalculator.cs
        GridCollisionMap.cs
        BlockModel.cs
        DoorModel.cs
        MergeSolver.cs
        LevelLoader.cs
      Gameplay/
        CameraRigController.cs  // applies CameraFramingCalculator output to the real Camera on level load
        GridDebugView.cs        // TEMPORARY Gizmos-only grid visualizer, remove once GridCellView/art exists
        GameplayTestBootstrap.cs // TEMPORARY scene wiring for manual camera/grid validation, pending real LevelLoader
        DragController.cs
        BlockView.cs
        DoorView.cs
        PowerupController.cs
        TimerController.cs
        WinLoseController.cs
      Tutorial/
        FtueController.cs
        TutorialStepDefinition.cs
      Save/
        SaveService.cs
        ProgressData.cs
      Audio/
        AudioService.cs
      VFX/
        VfxService.cs
      Editor/
        LevelEditorWindow.cs
        LevelDataDrawer.cs
    Tests/
      EditMode/                  // pure-logic tests: GridModelTests, CameraFramingCalculatorTests, MergeSolver, GridCollisionMap, etc. — no scene required
        StickersOut.Tests.EditMode.asmdef // Editor-only test assembly, references StickersOut.Core + UnityEngine.TestRunner
      PlayMode/                  // integration tests: drag-to-exit, full level completion flow
  ThirdParty/                    // imported packages that need Assets-folder placement (rare with UPM, but keep separate if it happens)
docs/                            // this file and its siblings (CLAUDE.md, ARCHITECTURE.md, etc.)
```

## Naming Conventions

| Item | Convention | Example |
|---|---|---|
| C# scripts | `PascalCase`, one public type per file, filename matches type name | `MergeSolver.cs` |
| ScriptableObject level assets | `Level_<levelId>.asset` | `Level_level_01.asset` |
| Prefabs | `PascalCase`, suffixed by role where it disambiguates | `BlockView.prefab`, `ParticleFX_Sparkle.prefab` |
| Scenes | `PascalCase`, one responsibility per scene | `Gameplay.unity` |
| Audio clips | `snake_case` or `PascalCase` consistently per team preference, prefixed by category | `sfx_merge_01.wav` |
| Sprite fragment sheets | Organized in a subfolder per category, filename matches the picture's identity | `Sprites/Fragments/Ocean/Ocean_Seahorse.png` |
| Category IDs (`CategoryDefinitions`) | `UPPER_SNAKE_CASE`, singular, no ad-hoc plural/singular drift (see `CATEGORY_TAXONOMY.md`) | `OCEAN`, `FRUIT` (not `FRUITS`) |

## Where Things Belong (Quick Lookup)

| If you're adding... | It goes in... |
|---|---|
| A new pure-logic system (no MonoBehaviour) | `Scripts/Core/` |
| A new gameplay MonoBehaviour that drives game flow | `Scripts/Gameplay/` |
| A new tutorial trigger type | `Scripts/Tutorial/TutorialStepDefinition.cs` (extend the data-driven definition, not a new hardcoded check) |
| A new save field | `Scripts/Save/ProgressData.cs` — update `DATA_MODEL.md` and `SAVE_SYSTEM.md` in the same change |
| A new SFX/VFX trigger | Route through `AudioService`/`VfxService`, don't call `AudioSource.Play`/instantiate particles directly from gameplay code |
| A new Level Editor feature | `Scripts/Editor/LevelEditorWindow.cs` |
| A new level | `ScriptableObjects/Levels/`, authored via the Level Editor tool, registered in `LevelSequence.asset` |
| A new category | `CategoryDefinitions.asset`, following `CATEGORY_TAXONOMY.md` naming rules — never a free-text string on a door/fragment |
| A new unit test | `Tests/EditMode/` if it exercises Core logic only, `Tests/PlayMode/` if it needs a running scene |

## Explicitly Not Present

- No `Images/` folder of externally-referenced, unmanaged picture files (the original prototype referenced paths like `Images/Birds_Owl.png` outside asset management) — all art here is imported and organized under `Art/Sprites/`.
- No legacy-content import folder or migration scripts — see `CLAUDE.md` Non-Goals.
