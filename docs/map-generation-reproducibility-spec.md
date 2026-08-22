# Map generation reproducibility specification

## Seeds and run lifecycle

- `--seed <integer>` and `--seed=<integer>` choose the initial run seed.
- Without an explicit seed, launch chooses a fresh non-negative seed.
- A map owns and exposes its seed; its title displays that value rather than incidental global random state.
- Restart This Seed reconstructs the same map and resets all run state.
- New Run chooses a different seed and generates a different run.
- The pause menu exposes both choices. Space after victory or defeat starts a new run.
- Reinitialising a map with its seed reproduces terrain, doors, actors, loot, and objective placement.

## Doorway pruning

- Door candidates remain corridor cells touching a room.
- Locked candidates are always retained.
- A deterministic post-candidate pass omits closed doors within a small local radius of another retained door, converting those joins to ordinary corridor floor.
- Pruning only removes door state; it must not alter the underlying room/corridor topology.
- The entrance, objective, loot, and every room remain reachable when doors are treated as openable terrain.
- Generated closed doors are not locally bunched; unavoidable locked-door proximity is permitted.
- The number of pruned candidates is exposed for generation diagnostics and seed-based test failures.

## Fixed controls

Seed selection is a launch/developer facility rather than a remappable gameplay action. Pause-menu arrows, Enter, terminal Space, and Escape retain their existing fixed application-control roles.
