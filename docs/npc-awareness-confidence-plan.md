# NPC awareness confidence — brief plan

1. Store confidence with NPC investigation state and decay it during investigating turns.
2. Derive starting confidence from evidence strength plus archetype persistence.
3. Preserve evidence priority and refresh rules when receiving observations.
4. Expose confidence in diagnostics and cover decay, expiry, and refresh with tests.

