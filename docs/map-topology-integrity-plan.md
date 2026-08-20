# Map topology integrity — brief plan

1. Add multi-seed tests for unintended room-to-room edges and whole-level reachability.
2. Establish a consistent room → corridor → obstacle flattening order.
3. Correct any remaining shared room boundaries without breaking intended connections.
4. Remove obsolete warnings, run the full suite and runtime smoke test, then commit.

