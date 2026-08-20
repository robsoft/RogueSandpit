# Item dropping — brief spec

- `D` drops the most recently acquired inventory item onto the player's current cell.
- A successful drop consumes a turn and records the item in the event log.
- Dropping fails without consuming a turn when the inventory is empty or the cell already holds loot.
- Dropping the equipped weapon unequips it immediately and updates player damage.
- Dropped items use the existing ground-loot rendering and can be collected again later.

