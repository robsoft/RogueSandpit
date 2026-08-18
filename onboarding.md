# Onboarding: Rogue Sandpit

A grid-based rogue-like built with **MonoGame** (DesktopGL) on **.NET 9**. Single project, no backend/services — a self-contained desktop game.

## What this project is

From the README: a simple rogue-like where a player moves around a procedurally generated map (rooms + corridors), fights NPCs, and (eventually) collects loot. It's fully turn-based — NPCs only act once the player has made a move.

Current gameplay: move with arrow keys, NPCs adjacent to the player deal damage, F1 toggles a debug map view that reveals the whole map and NPCs, SPACE restarts with a new map.

## Build & run

Requires the .NET SDK (project targets `net9.0`; this machine has 9.0.306 and 10.0.400 installed — either works since `RollForward` is set to `Major`).

```bash
cd RogueSandpit
dotnet build     # restores MonoGame + MGCB content-pipeline tools automatically, builds cleanly
dotnet run        # launches the game window
```

There's also a VS Code launch config (`RogueSandpit/.vscode/launch.json`, "C#: RogueSandpit Debug") and a solution file at the repo root (`RogueSandpit.slnx`) if you'd rather open it in an IDE. The README states it builds & runs on both Mac and Windows.

No test project exists yet — there's nothing to run beyond building/launching.

## Controls

| Key | Action |
|---|---|
| Arrow keys | Move player (WASD not yet implemented, despite README saying "will come") |
| F1 | Toggle debug/map viewer (shows full map + NPCs) |
| SPACE | Generate a new map / restart |
| ESCAPE | Quit |

## Architecture

- **`Program.cs`** — trivial entry point, just constructs and runs `GameWrapper`.
- **`GameWrapper.cs`** — the MonoGame `Game` subclass. Owns the update/draw loop, keyboard state diffing (current vs previous frame, to detect key-*press* rather than key-*down*), and window resize/aspect-ratio handling (renders to a fixed-size `RenderTarget2D` then scales it to the window). Delegates actual gameplay to `GameState`.
- **`Models/GameState.cs`** — turn logic. Reads player input, calls `Map.IsWalkable` before moving, then advances NPCs and the player for that turn. A move only "counts" (`PlayerTakenTurn`) once an arrow key is pressed, even if the player walked into a wall.
- **`Models/Map.cs`** (the biggest file) — procedural map generation: rooms, corridors, doorways, cell types (`Wall`/`Floor`/`Door`/`Special`), plus rendering (`RenderMode.Rooms` vs `RenderMode.Cells` for debug view).
- **`Models/Player.cs`**, **`Models/BaseNPC.cs`** / **`NPCs.cs`** — character state (HP, Damage, position) and NPC movement/AI (currently: random wandering within a room, attacks when adjacent to the player; chase/line-of-sight/A* logic is stubbed out in comments, not wired up).
- **`Models/PathFinding.cs`** — A* groundwork referenced by NPC comments but not yet in use.
- **`Models/Room.cs`, `Corridor.cs`, `Doorway.cs`, `MapCell.cs`, `BaseMapElement.cs`, `Obstacle.cs`, `Special.cs`** — map-generation building blocks.
- **`Models/RandGen.cs`** — seeded RNG wrapper (the map seed is shown in the window title, e.g. "Rogue Sandpit - Seed: 123").
- **`Graphics/PrimitiveDrawer.cs`** — simple shape/primitive rendering helpers used since there's no sprite art yet.
- **`Content/`** — MonoGame Content Pipeline (`Content.mgcb`); currently minimal/no real assets, matching the "Graphics!" TODO in the README.

## Current state (per README + code)

Working: map generation, player movement, NPCs that damage the player on contact, debug map view, turn-based flow.

**Uncommitted work in progress** (`RogueSandpit/GameWrapper.cs` has local changes not yet committed): splitting `Update`/`Draw` into live vs. dead-player states — when the player's `Health` hits 0 (`Player.Dead`), the map stops rendering and the only input handled is SPACE to restart. This looks like an in-progress "game over" screen; worth finishing (e.g. actually showing a game-over message rather than just blanking the screen) or committing once it feels done.

Known rough edges (from the README's "Pressing TODOs"):
- No hover-to-inspect in debug mode yet.
- `Special` tile placement needs better logic.
- NPC `Speed != 1.0` causes stacking/warping bugs.
- `MapCell`/`occupiedSpaces` tracking should be simplified/unified.
- `Player` should own its own movement logic (currently split across `GameState`/`Player`).
- Possible wall-clipping at hard-adjacent room boundaries via `IsWalkable`.
- No combat from the player side yet (NPCs can hit the player; the player can't hit back), no loot, no UI (HP/damage aren't displayed anywhere on screen).
- A*/line-of-sight NPC AI is scaffolded (see `PathFinding.cs` and commented-out code in `BaseNPC.Move`) but disabled.

## Where to look for "what's next"

The README's **Pressing TODOs** and **Intended Features** sections are the closest thing to a backlog — check there first before starting new work.
