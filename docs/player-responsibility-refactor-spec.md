# Player responsibility refactor — brief spec

- `GameState` continues to coordinate turn order, combat targets, doors, objectives, and event messages.
- `Player` owns relocation side effects: position, map player coordinates, discovery, and visibility refresh.
- `Player` owns selected-item use, equip, drop, and removal rules, including equipment cleanup.
- UI wording and successful/failed turn costs remain unchanged.
- Public compatibility needed by deterministic rule tests is retained during this incremental refactor.

