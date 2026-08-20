# Developer Diagnostics: Brief Spec

## Goal

Expose enough live map, NPC, and turn information to understand why the simulation is behaving as it does during development.

## Debug inspection

- Diagnostics are active only in the F1 cell/debug view.
- The cell beneath the mouse is highlighted.
- An overlay shows cell coordinates, terrain, containing map element, and any living NPC.
- NPC details include name, HP, damage, awareness, and last-known player position.
- Hovering an aware NPC visualizes its current target path.
- A line to the player indicates clear or blocked line of sight.

## Event log

- The simulation emits short messages for attacks, damage, NPC death, collecting the special, victory, and player death.
- Only a small bounded number of recent messages is retained.
- The latest messages are rendered on screen without requiring the console.
- Diagnostic presentation reads simulation state but does not change it.

## Out of scope

- A general developer console, selectable/frozen NPCs, saved logs, performance profiling, or production UI styling.

