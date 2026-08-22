# Engineering onboarding

Rogue Sandpit is a self-contained MonoGame DesktopGL game targeting .NET 9. It has no backend or external services. The repository is currently at a playable systems-prototype milestone, immediately before focused visual, UI, and theme exploration.

Start with [README.md](README.md) for the player-facing overview and complete controls. This document records the engineering shape of the prototype and the assumptions that should survive future presentation work.

## Quick start

Run commands from the repository root:

```powershell
dotnet build RogueSandpit.slnx
dotnet test RogueSandpit.slnx
dotnet run --project RogueSandpit/RogueSandpit.csproj
```

Useful launch combinations:

```powershell
dotnet run --project RogueSandpit/RogueSandpit.csproj -- --scale 1
dotnet run --project RogueSandpit/RogueSandpit.csproj -- --fullscreen
dotnet run --project RogueSandpit/RogueSandpit.csproj -- --fullscreen --realtime --turn-seconds 1.5
```

The native render canvas is always 800×600; window scaling and fullscreen presentation do not alter simulation or layout coordinates.

## Architecture

### Application boundary

- `Program.cs` constructs and runs `GameWrapper`.
- `GameOptions.cs` parses integer scaling, fullscreen, initial real-time mode, and the timed-turn interval.
- `GameWrapper.cs` owns the MonoGame update/draw loop, input translation, prompts, focus handling, and presentation scaling.
- Real-time mode does not create a second simulation path. It submits the same player `Wait` action used by turn-based play when its idle interval expires.

### Simulation and turns

- `Models/GameState.cs` coordinates the current run and player-to-NPC turn boundary.
- `Models/RunStatistics.cs` is the framework-independent per-run record used by pause, developer, and terminal reports. `GameState` updates it from successful actions and reconciled before/after simulation state rather than event-log text.
- `Models/NpcTurnScheduler.cs` snapshots the active NPC phase and rotates initiative between turns.
- Simulation models are independent of MonoGame so rules can be exercised by xUnit without a graphics device.
- NPC actions currently resolve sequentially. Rotation provides fairness, but this is not simultaneous intent resolution.

### World and navigation

- `Models/Map.cs` owns its generation seed, entrance-distance/depth data, generated terrain, encounter pacing, doorway pruning, and occupancy decisions. It also coordinates noise, alerts, evidence, ground items, placed traps, throws, and environmental effects.
- `Models/EnvironmentalEffect.cs` represents temporary smoke and fire.
- `Models/PathFinding.cs` provides cardinal A* navigation using actor-aware blocking rules.
- Rooms, corridors, doors, cells, obstacles, the entrance, and objective are separate map-model types under `Models/`.

### Actors and items

- `Models/Player.cs` holds player health, inventory, equipment, and player-specific actions.
- `Models/BaseNPC.cs` contains shared NPC state and behaviour; `NPCs.cs` and the awareness, morale, and ranged profiles define archetype differences.
- `Models/StatusEffects.cs` implements shared timed conditions such as bleeding and stun.
- `Models/Items.cs` contains item models, generated equipment tiers, inventory selection, and item creation.
- `Models/PlacedTrap.cs` models armed traps on map cells.

### Presentation

- `Graphics/MapRenderer.cs`, `PrimitiveDrawer.cs`, `PixelFont.cs`, and `ViewportMapper.cs` render the native canvas, UI, fog, and diagnostic overlays.
- `Content/Content.mgcb` is the MonoGame content-pipeline entry point. It is intentionally sparse because the current presentation is built from primitives.
- Normal rendering respects discovered and visible cells. F1 is omniscient and adds inspection, paths, line-of-sight information, awareness colours, and the real-time countdown.

## Important invariants

Preserve these behaviours when changing input, UI, animation, or timing:

- One successful player turn advances at most one complete NPC phase.
- Invalid actions, opening or navigating a selection UI, and cancelled directional prompts do not spend a turn.
- Automatic real-time waits are silent in the event log; deliberate wait actions are logged.
- Invalid and UI-only actions do not alter run statistics. A restarted or new `GameState` always begins with a fresh statistics record, even when the map seed is reused.
- Timed turns pause for modal prompts, the inventory panel, terminal game states, and lost window focus.
- A defeated NPC neither acts nor occupies a cell, but its death remains available as casualty evidence and its drops remain on the map.
- Normal fog of war and F1 omniscience are distinct presentation modes over the same simulation.
- Reinitialising a map resets its owned seed and reproduces terrain, doors, actors, loot, and the objective. New runs explicitly choose a different seed.
- Initial NPC placement remains beyond the protected entrance distance; generation depth affects archetype weights, objective guards, loot location, and equipment tier without changing later NPC movement rules.

## Development workflow

The project has generally progressed in small feature pairs:

1. Agree a brief specification and implementation plan.
2. Create a focused feature branch.
3. Implement rules with focused tests, then run the full suite.
4. Inspect the diff and perform a runtime smoke test where presentation or input changed.
5. Open a pull request and merge at a coherent milestone.

Completed feature specifications and plans live under `docs/`. They are useful design history, but the README and this file are the authoritative summaries of the present project.

Small observations intentionally deferred from active milestones are recorded in [`docs/deferred-snags.md`](docs/deferred-snags.md).

## Known debt and deliberately deferred work

- NPC initiative rotates fairly but actions are not planned and resolved simultaneously.
- Combat values, spawn rates, item availability, and NPC profiles are prototype balance.
- The event log and F1 view expose useful state but are not a finished player-facing information design.
- Rendering is predominantly primitive geometry and a tiny built-in pixel font.
- Rebindable controls, controller support, and broader accessibility settings are not implemented.
- Save/load, multiple levels, progression outside a run, audio, and release packaging are not implemented.

## Recommended next step

Pause broad simulation expansion long enough to define the presentation direction. Establish the theme and tone, decide which game states must be legible without F1, sketch the in-game HUD and interaction prompts, and choose conventions for resolution, sprites, fonts, animation, input, and accessibility. Once those decisions exist, integrate a small representative asset slice before committing to a complete art pipeline.
