# Doors and keys — brief spec

- Generated room entrances may contain closed or locked doors.
- Closed and locked doors block movement, NPC pathfinding, and line of sight.
- Bumping a closed door opens it and spends the turn; a second move crosses it.
- Bumping a locked door opens it only when the player carries a key. Keys are retained.
- Every generated map provides a reachable key before any locked door is required.
- Door state is visible in both map render modes and in cell diagnostics.

