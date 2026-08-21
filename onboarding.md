# Onboarding: Rogue Sandpit

A grid-based rogue-like built with **MonoGame** (DesktopGL) on **.NET 9**. Single project, no backend/services — a self-contained desktop game.

## What this project is

From the README: a simple rogue-like where a player moves around a procedurally generated map (rooms + corridors), fights NPCs, and collects loot. Its simulation remains turn-based, with an optional timed-input mode that automatically waits for an idle player.

Current gameplay: explore through persistent fog-of-war, fight NPCs with melee weapons and bows, collect tiered equipment, and retrieve the yellow special tile before returning to the entrance. Goblins shoot from a preferred distance and disengage when crowded. Thrown smoke bombs create temporary sight-blocking cover; fire bombs create damaging terrain that NPCs avoid. Bandages stop bleeding and restore some health. The player can operate doors, lay false trails, throw items, and place varied traps. Optional F12 timed turns automatically wait for an idle player. SPACE restarts.

## Build & run

Requires the .NET SDK (project targets `net9.0`; this machine has 9.0.306 and 10.0.400 installed — either works since `RollForward` is set to `Major`).

```bash
cd RogueSandpit
dotnet build     # restores MonoGame + MGCB content-pipeline tools automatically, builds cleanly
dotnet run        # launches at the accessibility-friendly 2x window scale
dotnet run -- --scale 1 --turn-seconds 1.5  # compact window and custom real-time interval
dotnet run -- --fullscreen --realtime       # borderless fullscreen with timed turns enabled
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
| B | Use the selected bandage to stop bleeding and heal |
| E | Equip the selected melee weapon, ranged weapon, or armor |
| D | Drop the selected item |
| C | Open or close the only operable adjacent door, or choose among several with an arrow |
| T + arrow | Lay a false trail pointing in that direction |
| F + arrow | Throw the selected inventory item up to six cells |
| P + arrow | Place the selected trap on an adjacent cell |
| R + arrow | Fire the equipped ranged weapon up to six cells |
| Period / numpad 5 | Wait one turn |
| F1 | Toggle debug/map viewer (shows full map + NPCs) |
| F12 | Toggle real-time mode; an idle countdown submits a wait turn |
| SPACE | Generate a new map / restart |
| ESCAPE | Cancel a directional action, otherwise quit |

## Architecture

- **`Program.cs`** — trivial entry point, just constructs and runs `GameWrapper`.
- **`GameOptions.cs`** / **`GameWrapper.cs`** — command-line scaling/fullscreen and real-time launch modes plus the MonoGame update/draw loop, UI, input translation, and aspect-ratio-preserving presentation. The native canvas remains 800×600 in every mode.
- **`Models/GameState.cs`** / **`Models/NpcTurnScheduler.cs`** — framework-independent turn coordination. The active NPC phase is snapshotted and its initiative rotates each turn, avoiding permanent map-list priority while retaining sequential action resolution.
- **`Models/Map.cs`** / **`EnvironmentalEffect.cs`** — procedural map generation and centralized terrain/actor occupancy queries. The map distributes noise and alerts and owns evidence trails, actor-aware throws, ground loot, placed traps, and turn-aged smoke/fire effects.
- **`Models/Player.cs`**, **`Models/BaseNPC.cs`** / **`NPCs.cs`** / **`NPCAwarenessProfile.cs`** / **`NPCMoraleProfile.cs`** / **`NPCRangedProfile.cs`** / **`StatusEffects.cs`** — character state, shared timed actor effects, and NPC identity/movement/AI. Five seeded archetypes have distinct combat, perception, tracking, morale, retreat, and help-call behavior; Goblins additionally maintain range and shoot.
- **`Models/Items.cs`** / **`PlacedTrap.cs`** — item, ground-loot, inventory selection, item-factory, and placed-trap models. The player has an eight-slot inventory; tiered equipment modifies combat, potions heal, bandages stop bleeding, keys unlock doors, and varied traps can be placed.
- **`Models/PathFinding.cs`** — cardinal A* used by NPC pursuit; walls and living NPCs block paths.
- **`Models/Room.cs`, `Corridor.cs`, `Doorway.cs`, `MapCell.cs`, `BaseMapElement.cs`, `Obstacle.cs`, `Special.cs`** — map-generation building blocks.
- **`Models/RandGen.cs`** — seeded RNG wrapper (the map seed is shown in the window title, e.g. "Rogue Sandpit - Seed: 123").
- **`Graphics/MapRenderer.cs`**, **`PrimitiveDrawer.cs`**, **`PixelFont.cs`**, and **`ViewportMapper.cs`** — map/UI presentation kept separate from simulation rules. Normal rendering applies persistent cell fog-of-war and archetype colours. Debug mode is omniscient and supports scaled mouse-to-cell inspection, path overlays, and line-of-sight lines.
- **`Content/`** — MonoGame Content Pipeline (`Content.mgcb`); currently minimal/no real assets, matching the "Graphics!" TODO in the README.

## Current state (per README + code)

Working: map generation, fog-of-war exploration, player and Goblin ranged combat, five named NPC archetypes with temperament, morale, and distance-aware AI, rotating initiative, casualty investigation, hazard avoidance, coordinated searches, evidence/hearing, tiered equipment, recovery items, actor-aware throwing, varied traps, temporary smoke/fire terrain, shared status effects, doors, objective, HUD/debug views, strict turn-based play, and optional timed turns.

Known rough edges (from the README's "Pressing TODOs"):
- Initiative is now fair across turns, but NPC actions still resolve sequentially rather than through fully simultaneous declared intentions.

## Where to look for "what's next"

The README's **Pressing TODOs** and **Intended Features** sections are the closest thing to a backlog — check there first before starting new work.
