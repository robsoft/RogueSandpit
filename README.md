# Rogue Sandpit

Rogue Sandpit is a playable roguelike prototype built with MonoGame DesktopGL and .NET 9. Explore a procedurally generated level, survive an increasingly coordinated cast of enemies, recover the yellow objective, and return it to the entrance.

The project has reached its first systems-complete prototype milestone. Its next phase is deliberately less about adding mechanics and more about establishing a visual identity, clearer UI, assets, animation, and balance.

## The playable loop

Each run places the player, enemies, equipment, consumables, traps, doors, environmental hazards, and an objective on a seeded rooms-and-corridors map. Normal play uses persistent fog of war; the complete simulation can be inspected through the F1 developer view.

The simulation is strictly turn based. Every successful player action advances the NPC phase once, while cancelled prompts and invalid selections cost no time. F12 optionally enables a real-time input mode: if the player remains idle for the configured interval, the game submits a silent wait action. Timed turns pause while a prompt, inventory, pause, or options panel is open and whenever the window loses focus.

## Current systems

### Exploration and environment

- Seeded procedural rooms, corridors, doors, entrance, and retrieval objective
- Reproducible run seeds and deterministic pruning of visually clustered doorway candidates
- Path-distance encounter pacing with a protected entrance approach and shallow/middle/deep generation bands
- A scrolling 18×16 local viewport using native-size 32×32 tiles, with the compact whole-map view retained under F1
- A structured right-hand HUD for player state, equipment, inventory, objective, effects, and recent events
- Keyboard-driven pause and runtime options screens with simulation-safe modal behaviour
- Versioned single-slot JSON save/restore with safe validation and faithful random continuation
- Live run statistics, a compact pause snapshot, and full victory/defeat reports suitable for playtest feedback
- Binding-aware in-game Help and automatic JSON reports for completed playtest runs
- Remappable gameplay controls and persistent per-user runtime settings
- Persistent fog of war with an omniscient developer view
- Doors that can be opened or closed in place; locked doors require a reusable key
- Sound propagation, physical trails, and player-created false trails
- Temporary smoke that blocks sight and ranged attacks
- Temporary fire that damages actors and influences pathfinding

### Combat and items

- Bump-to-attack melee combat and directional short-bow attacks
- Three generated power tiers for weapons, armor, and bows
- Eight-slot selectable inventory with automatic first-weapon equip
- Potions, bandages, keys, throwable distractions, smoke bombs, and fire bombs
- Hunting, snare, and alarm traps; shared bleeding and stunned effects
- Recoverable thrown weapons and loot drops from defeated NPCs

### NPC simulation

- Orc, Goblin, Skeleton, Troll, and Wretch archetypes with seeded names and distinct profiles
- Depth-weighted populations: weaker shallow encounters, mixed middle rooms, stronger deep opposition, and objective guards
- Line of sight, A* pursuit, hearing, last-known-position memory, prediction, and local searching
- Confidence decay, evidence trails, coordinated alerts, distributed searches, calls for help, and casualty investigation
- Archetype-specific morale, tracking skill, trap awareness, and hazard avoidance
- Goblin ranged combat and retreat-to-range behaviour
- Rotating initiative so fixed list order does not permanently decide contested movement

## Build and run

From the repository root:

```powershell
dotnet build RogueSandpit.slnx
dotnet run --project RogueSandpit/RogueSandpit.csproj
dotnet test RogueSandpit.slnx
```

The project targets `net9.0` and restores MonoGame and its content-pipeline tooling through NuGet. It has been developed for Windows and macOS using the DesktopGL backend. The default window is 1600×1200, presenting an integer-scaled 800×600 native canvas.

### Launch options

| Option | Effect |
|---|---|
| `--scale 1` to `--scale 4` | Select an integer window scale; the default is `2` |
| `--fullscreen` | Use borderless desktop fullscreen with aspect-ratio-preserving scaling |
| `--realtime` | Start with timed turns enabled |
| `--turn-seconds <number>` | Set the idle interval used by real-time mode |
| `--seed <integer>` | Reproduce a specific generated run |

Options can be combined:

```powershell
dotnet run --project RogueSandpit/RogueSandpit.csproj -- --fullscreen --realtime --turn-seconds 1.5 --seed 123
```

## Controls

| Key | Action |
|---|---|
| Arrow keys | Move or answer a directional prompt |
| `Space`, `.`, or numpad `5` | Wait one turn |
| `[` / `]` | Select an inventory item |
| `I` | Open or close the inventory panel; arrows or `1`–`8` select while open |
| `E`, or `Enter` in inventory | Equip or unequip the selected weapon, bow, or armor |
| `H` / `B` | Use the selected healing potion / bandage |
| `D` | Drop the selected item |
| `C` | Toggle an adjacent unlocked door; choose a direction if ambiguous |
| `F`, then arrow | Throw the selected item; ordinary items inflict type/power-based impact damage |
| `R`, then arrow | Fire the equipped ranged weapon |
| `P`, then arrow | Place the selected trap |
| `T`, then arrow | Lay a false trail |
| `F1` | Toggle the developer view |
| `F11` | Restore health and apply a representative developer test loadout |
| `F12` | Toggle real-time mode |
| `Space` | Start a new generated run after victory or defeat |
| `Escape` | Cancel a prompt, close inventory, return from options, resume, or pause |
| Arrows / `Enter` | Navigate and confirm pause/options menu choices |

Gameplay keys can be changed under **Pause → Options → Controls**. Each action has a primary and optional secondary binding; Tab chooses the slot, Enter captures a new key, Backspace resets one action, and Delete clears its secondary binding. Conflicting keys are rejected. Escape, Enter, menu arrows, F1, F11, and F12 remain fixed so navigation and developer controls cannot become inaccessible.

Bindings and runtime options are saved to the platform's per-user application-data folder (`RogueSandpit/settings.json`). The single game-save slot is stored alongside it as `RogueSandpit/save-game.json`. Missing or damaged files are handled safely.

The pause menu provides **Help**, **Save Game**, and **Load Game**, plus both **Restart This Seed**, which reconstructs the current generated run, and **New Run**, which chooses a fresh seed. Help always reflects the active remapped controls. Loading returns to a clean paused state and never spends a turn. Its compact statistics snapshot makes useful playtest information available without entering developer mode. Victory and defeat show a fuller run report; use arrows and Enter to restart the same seed, start a new run, or quit. Space remains a quick New Run shortcut. The window title and F1 diagnostics show the active seed for reporting or reproducing interesting maps.

Every completed run also writes a versioned JSON playtest report under the per-user `RogueSandpit/run-reports` directory. The terminal screen shows the generated filename; report failure never interrupts play.

## Developer diagnostics

F1 reveals the full map and development-only state. Hovering a cell identifies its contents, entrance distance and generation-depth band, and exposes NPC intent, line of sight, paths, and awareness state. NPC colours distinguish pursuit, investigation, retreat, and rage. The active seed, retained/pruned doorway counts, real-time countdown, and compact live run statistics are shown only in this view.

These diagnostics intentionally expose simulation information that normal play hides. They are expected to evolve or disappear behind better player-facing visual language later.

## Project layout

| Path | Purpose |
|---|---|
| `RogueSandpit/` | MonoGame application and framework-independent game models |
| `RogueSandpit/Models/` | Map generation, actors, items, combat, AI, and turn simulation |
| `RogueSandpit/Graphics/` | Primitive rendering, pixel text, viewport mapping, and debug presentation |
| `RogueSandpit/Content/` | MonoGame content pipeline; intentionally sparse at this milestone |
| `RogueSandpit.Tests/` | xUnit rule and simulation tests |
| `docs/` | Short specifications and plans from completed milestones |
| `onboarding.md` | Engineering handoff, invariants, architecture, and workflow |

## Current boundary and next phase

Rogue Sandpit currently communicates almost everything with coloured primitives, terse text, and developer diagnostics. That was useful while proving the rules, but it is now the main constraint on further design.

Before expanding the simulation again, the next useful decisions are:

- Theme, tone, palette, and the visual character of the world
- Tile, character, item, effect, and animation approach
- A production HUD and contextual interaction language
- Which simulation signals deserve player-facing emphasis and which should remain hidden
- Font, resolution, input, accessibility, and asset-pipeline conventions

The agreed implementation sequence, nine-tile atlas contract, and UI architecture are recorded in [the presentation and UI roadmap](docs/presentation-roadmap.md).

NPC actions still resolve sequentially within a fair rotating initiative rather than through simultaneous intent and conflict resolution. That remains worthwhile architectural work, but it can wait until the presentation direction clarifies what the game most needs next.
