# Map generation reproducibility plan

1. Parse and test an optional integer launch seed.
2. Make Map own its seed and provide deterministic same-seed and explicit-new-seed regeneration.
3. Split pause restart into Restart This Seed and New Run; update terminal restart and presentation text.
4. Build a deterministic doorway-candidate pruning pass which always preserves locked doors.
5. Add broad seeded tests for determinism, spacing, pruning, reachability, and mandatory content.
6. Update the README and diagnostics, run the full suite, and perform targeted seed runtime smoke tests.
