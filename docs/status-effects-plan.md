# Timed status effects — brief plan

1. Add a framework-independent effect collection shared by Player and BaseNPC.
2. Process bleeding and action-skipping at each actor's turn boundary.
3. Centralize NPC death loot consequences for direct, trap, impact, and status damage.
4. Add diagnostics and tests for application, refresh, expiry, skipped actions, damage, and death.

