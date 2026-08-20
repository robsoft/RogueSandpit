# Simulation Boundary: Brief Spec

## Goal

Make gameplay rules runnable without a MonoGame window so future pathfinding, line-of-sight, combat, and map tests can use a clean simulation API.

## Requirements

- `Map` contains generation and terrain/occupancy data, but no graphics device or drawing code.
- A dedicated renderer draws a map without changing it.
- `GameState` accepts a small gameplay command rather than keyboard state.
- Player and NPC turns do not depend on `GameTime` when elapsed time is not used.
- Existing controls and gameplay behavior remain unchanged.
- Tests use the same public command API as the running game.

## Out of scope

- Changing map generation or combat balance.
- Adding A*, line of sight, or new NPC behavior.
- Splitting the code into additional production assemblies.
- Removing every MonoGame value type from map-generation models.

