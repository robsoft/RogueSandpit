# Fair NPC turn resolution — brief plan

1. Extract NPC phase ordering into a stateful turn scheduler.
2. Rotate the first resolver deterministically while snapshotting eligible actors.
3. Route `GameState` through the scheduler and test repeated cell contention.
4. Expose initiative in developer inspection and document the remaining resolution boundary.

