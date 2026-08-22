# Save and restore game plan

1. Inventory mutable model state and define versioned, framework-independent snapshot records.
2. Make the deterministic random source export and restore its state.
3. Add explicit capture/restore boundaries to models with private behavioural state.
4. Implement a single-slot JSON store with validation and atomic writes.
5. Reconstruct a complete GameState without exposing partially loaded state.
6. Add pause-menu Save Game and Load Game actions with concise status feedback.
7. Cover round trips, random continuation, corrupt/missing/versioned files, and turn-safety with tests.
8. Update project documentation, run the full suite, smoke-test, and open a review PR.
