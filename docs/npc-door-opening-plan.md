# NPC door opening — brief plan

1. Let NPC-aware pathfinding distinguish closed doors from locked doors.
2. Make chase/investigation movement open a closed door instead of entering it.
3. Add event reporting and deterministic tests for closed, locked, and wandering cases.
4. Update notes, run the full suite and runtime smoke test, then commit with local search.

