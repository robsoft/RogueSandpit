# NPC predictive pursuit — brief spec

- While an NPC can see successive player moves, it remembers the latest cardinal movement direction.
- When sight breaks, the NPC projects up to four traversable cells beyond the last-seen position in that direction.
- Open floor and open or closed doors may form the projected route; walls and locked doors stop it.
- The projected cell becomes the initial investigation destination, while the true last-seen cell remains available for diagnostics.
- If there is no observed direction or no continuation, investigation falls back to the last-seen cell.
- Prediction never updates from the player's hidden movement.

