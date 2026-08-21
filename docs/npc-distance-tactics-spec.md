# NPC distance-keeping tactics — brief spec

- A ranged Goblin adjacent to the player first tries to step to a traversable cell that increases distance.
- Retreat selection avoids occupied cells, the player, and hazards the Goblin knows about.
- If no retreat step exists, the Goblin falls back to its normal melee attack.
- Outside firing range, ranged NPCs continue to pursue normally.

