# Onboarding: Rogue Sandpit

A grid-based rogue-like built with **MonoGame** (DesktopGL) on **.NET 9**. Single project, no backend/services — a self-contained desktop game.

## What this project is

From the README: a simple rogue-like where a player moves around a procedurally generated map (rooms + corridors), fights NPCs, and (eventually) collects loot. It's fully turn-based — NPCs only act once the player has made a move.

Current gameplay: explore through persistent fog-of-war, bump into NPCs to attack, collect potions/weapons/keys/armor, and retrieve the yellow special tile before returning to the entrance. Closed doors take a turn to open; locked doors need a carried reusable key. Chasing NPCs can open closed doors but cannot unlock them. Brackets select inventory items; H uses a selected potion, E equips selected weapons or armor, and D drops the selected item. Named Orcs, Goblins, Skeletons, Trolls, and Wretches have distinct combat profiles and may drop carried loot. F1 toggles an omniscient debug map with hover inspection, paths, line of sight, and door state. SPACE restarts.

## Build & run

Requires the .NET SDK (project targets `net9.0`; this machine has 9.0.306 and 10.0.400 installed — either works since `RollForward` is set to `Major`).

```bash
cd RogueSandpit
dotnet build     # restores MonoGame + MGCB content-pipeline tools automatically, builds cleanly
dotnet run        # launches the game window
```

There's also a VS Code launch config (`RogueSandpit/.vscode/launch.json`, "C#: RogueSandpit Debug") and a solution file at the repo root (`RogueSandpit.slnx`) if you'd rather open it in an IDE. The README states it builds & runs on both Mac and Windows.

Rule-level tests live in `RogueSandpit.Tests`; run them with `dotnet test` from the repository root.

## Controls

| Key | Action |
|---|---|
| Arrow keys | Move player (WASD not yet implemented, despite README saying "will come") |
| Left/right bracket | Select an inventory item |
| H | Use the selected healing potion |
| E | Equip the selected weapon or armor |
| D | Drop the selected item |
| Period / numpad 5 | Wait one turn |
| F1 | Toggle debug/map viewer (shows full map + NPCs) |
| SPACE | Generate a new map / restart |
| ESCAPE | Quit |

## Architecture

- **`Program.cs`** — trivial entry point, just constructs and runs `GameWrapper`.
- **`GameWrapper.cs`** — the MonoGame `Game` subclass. Owns the update/draw loop, translates key presses into `PlayerCommand` values, and handles window resizing/aspect ratio. Delegates gameplay to `GameState` and drawing to renderers.
- **`Models/GameState.cs`** — framework-independent turn logic. Accepts one `PlayerCommand`, attempts the player action, then advances NPCs. A directional command consumes a turn even when terrain blocks movement. It also owns the bounded `GameEventLog` used by the on-screen event feed.
- **`Models/Map.cs`** — procedural map generation and terrain/occupancy queries: rooms, corridors, doorways, and flattened cell types (`Wall`/`Floor`/`Door`/`Special`). It can be constructed and exercised without graphics.
- **`Models/Player.cs`**, **`Models/BaseNPC.cs`** / **`NPCs.cs`** — character state and NPC identity/movement/AI. Five seeded, named archetypes have distinct health/damage profiles. NPCs attack from cardinal adjacency, pursue a player visible within 12 cells, investigate the last visible position after losing sight, and then return to wandering.
- **`Models/Items.cs`** — item, ground-loot, inventory selection, and item-factory models. The player has an eight-slot inventory; potions heal, weapons add damage, armor adds defence, and reusable keys unlock doors.
- **`Models/PathFinding.cs`** — cardinal A* used by NPC pursuit; walls and living NPCs block paths.
- **`Models/Room.cs`, `Corridor.cs`, `Doorway.cs`, `MapCell.cs`, `BaseMapElement.cs`, `Obstacle.cs`, `Special.cs`** — map-generation building blocks.
- **`Models/RandGen.cs`** — seeded RNG wrapper (the map seed is shown in the window title, e.g. "Rogue Sandpit - Seed: 123").
- **`Graphics/MapRenderer.cs`**, **`PrimitiveDrawer.cs`**, **`PixelFont.cs`**, and **`ViewportMapper.cs`** — map/UI presentation kept separate from simulation rules. Normal rendering applies persistent cell fog-of-war and archetype colours. Debug mode is omniscient and supports scaled mouse-to-cell inspection, path overlays, and line-of-sight lines.
- **`Content/`** — MonoGame Content Pipeline (`Content.mgcb`); currently minimal/no real assets, matching the "Graphics!" TODO in the README.

## Current state (per README + code)

Working: map generation, fog-of-war exploration, player movement and bump combat, five named NPC archetypes with pursuit/search AI, loot/inventory/equipment, closed and locked doors, a retrieve-and-return objective, HUD, visible win/loss states, debug map view, and turn-based flow.

Known rough edges (from the README's "Pressing TODOs"):
- Initial placement and live actor occupancy could be consolidated further.
- `Player` should own its own movement logic (currently split across `GameState`/`Player`).
- NPC local searches are deliberately short and do not yet predict exits, react to sound, or share awareness.

## Where to look for "what's next"

The README's **Pressing TODOs** and **Intended Features** sections are the closest thing to a backlog — check there first before starting new work.
