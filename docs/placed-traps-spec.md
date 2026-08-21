# Placed traps — brief spec

- A new `HUNTING TRAP` inventory item can be selected and placed with `P` plus an arrow.
- Placement requires an adjacent walkable, unoccupied cell with no ground item or existing trap.
- Successful placement consumes the item and one turn; invalid placement consumes neither.
- The first living NPC entering a trap takes its damage, triggers loud noise, and removes the trap.
- Traps are visible to the player on currently visible cells and fully visible in F1 mode.
- Traps do not block pathfinding, so NPCs currently have no trap avoidance.

