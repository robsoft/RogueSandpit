# Directional item throwing — brief spec

- `F` begins a directional throw for the selected inventory item; an arrow chooses direction and Escape cancels.
- An item travels up to six cells through traversable terrain and lands on the furthest free cell before terrain or a living NPC blocks it.
- Throwing fails without consuming a turn if no item is selected or no valid landing cell exists.
- A successful throw removes and unequips the item if necessary, places it as ordinary ground loot, consumes a turn, and creates impact noise at the landing cell.
- Thrown items can be recovered normally.

