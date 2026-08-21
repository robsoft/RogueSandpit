# Onboarding: Rogue Sandpit

A grid-based rogue-like built with **MonoGame** (DesktopGL) on **.NET 9**. Single project, no backend/services — a self-contained desktop game.

## What this project is

From the README: a simple rogue-like where a player moves around a procedurally generated map (rooms + corridors), fights NPCs, and (eventually) collects loot. It's fully turn-based — NPCs only act once the player has made a move.

Current gameplay: explore through persistent fog-of-war, bump into NPCs to attack, collect loot, and retrieve the yellow special tile before returning to the entrance. The player can close open doors with `C`; the only adjacent door closes immediately, while multiple doors prompt for an arrow direction. `T` plus an arrow lays a short-lived false trail. Named archetypes have distinct combat, awareness, tracking, and morale profiles. Wounded Goblins, Wretches, and critically hurt Orcs retreat and call for help once; Skeletons are fearless and wounded Trolls enrage for bonus damage. Retreats use remembered threat positions rather than hidden player movement. F1 exposes paths, evidence, confidence, predictions, trail clues, morale, and retreat targets. SPACE restarts.

## Build & run

Requires the .NET SDK (project targets `net9.0`; this machine has 9.0.306 and 10.0.400 installed — either works since `RollForward` is set to `Major`).

```bash
cd RogueSandpit
dotnet build     # restores MonoGame + MGCB content-pipeline tools automatically, builds cleanly
dotnet run        # launches at the accessibility-friendly 2x window scale
dotnet run -- --scale 1  # original 800x600 window used for compact debugging
```

There's also a VS Code launch config (`RogueSandpit/.vscode/launch.json`, "C#: RogueSandpit Debug") and a solution file at the repo root (`RogueSandpit.slnx`) if you'd rather open it in an IDE. The README states it builds & runs on both Mac and Windows.

Rule-level tests live in `RogueSandpit.Tests`; run them with `dotnet test` from the repository root.

## Controls

| Key | Action |
|---|---|
| Arrow keys | Move player (WASD not yet implemented, despite README saying "will come") |
| Left/right bracket | Select an inventory item |
| I | Open/close the eight-slot inventory panel (arrows select while open) |
| H | Use the selected healing potion |
| E | Equip the selected weapon or armor |
| D | Drop the selected item |
| C | Close the only adjacent open door, or choose among several with an arrow |
| T + arrow | Lay a false trail pointing in that direction |
| Period / numpad 5 | Wait one turn |
| F1 | Toggle debug/map viewer (shows full map + NPCs) |
| SPACE | Generate a new map / restart |
| ESCAPE | Cancel a directional action, otherwise quit |

## Architecture

- **`Program.cs`** — trivial entry point, just constructs and runs `GameWrapper`.
- **`GameOptions.cs`** / **`GameWrapper.cs`** — command-line window scaling plus the MonoGame update/draw loop, inventory and directional-action UI, input translation, and aspect-ratio-preserving resizing. The native canvas remains 800×600 at every window scale.
- **`Models/GameState.cs`** — framework-independent turn coordinator. It resolves targets, doors, objectives, event messages, and NPC response order while delegating player state changes and shared occupancy rules to their owning models.
- **`Models/Map.cs`** — procedural map generation and centralized terrain/actor occupancy queries: rooms, corridors, doorways, and flattened cell types (`Wall`/`Floor`/`Door`/`Special`). It distributes noise and alerts, projects observed movement, and owns genuine and false terrain-sensitive trails.
- **`Models/Player.cs`**, **`Models/BaseNPC.cs`** / **`NPCs.cs`** / **`NPCAwarenessProfile.cs`** / **`NPCMoraleProfile.cs`** — character state and NPC identity/movement/AI. Five seeded archetypes have distinct combat, perception, tracking, morale, retreat, and help-call behavior. NPCs investigate evidence, coordinate searches, flee toward bounded safe targets, or enrage according to their profile.
- **`Models/Items.cs`** — item, ground-loot, inventory selection, and item-factory models. The player has an eight-slot inventory; potions heal, weapons add damage, armor adds defence, and reusable keys unlock doors.
- **`Models/PathFinding.cs`** — cardinal A* used by NPC pursuit; walls and living NPCs block paths.
- **`Models/Room.cs`, `Corridor.cs`, `Doorway.cs`, `MapCell.cs`, `BaseMapElement.cs`, `Obstacle.cs`, `Special.cs`** — map-generation building blocks.
- **`Models/RandGen.cs`** — seeded RNG wrapper (the map seed is shown in the window title, e.g. "Rogue Sandpit - Seed: 123").
- **`Graphics/MapRenderer.cs`**, **`PrimitiveDrawer.cs`**, **`PixelFont.cs`**, and **`ViewportMapper.cs`** — map/UI presentation kept separate from simulation rules. Normal rendering applies persistent cell fog-of-war and archetype colours. Debug mode is omniscient and supports scaled mouse-to-cell inspection, path overlays, and line-of-sight lines.
- **`Content/`** — MonoGame Content Pipeline (`Content.mgcb`); currently minimal/no real assets, matching the "Graphics!" TODO in the README.

## Current state (per README + code)

Working: map generation, fog-of-war exploration, player movement and bump combat, five named NPC archetypes with temperament and morale-driven AI, retreat and one-shot help calls, coordinated searches, prediction, terrain-sensitive and false trails, decaying confidence, hearing, loot/inventory/equipment, doors, directional actions, objective, HUD, debug view, and turn-based flow.

Known rough edges (from the README's "Pressing TODOs"):
- Directional actions currently cover door closing and false trails but not throwing, traps, or ranged combat.

## Where to look for "what's next"

The README's **Pressing TODOs** and **Intended Features** sections are the closest thing to a backlog — check there first before starting new work.
