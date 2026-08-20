# Special placement — brief spec

- The special objective is placed on a reachable, unoccupied room-floor cell.
- Distance is measured by cardinal traversal through generated terrain, with openable doors treated as passable.
- The chosen cell has the greatest actual path distance from the entrance among eligible cells.
- Placement remains deterministic for a given map seed.
- The existing retrieve-and-return win condition is unchanged.

