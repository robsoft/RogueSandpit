# Actor occupancy architecture — brief plan

1. Flatten terrain before placing NPCs.
2. Replace provisional coordinate lists/retry loops with eligible map-cell candidates.
3. Centralize live NPC entry rules on `Map` and route wandering through them.
4. Add multi-seed placement and focused live-occupancy tests.

