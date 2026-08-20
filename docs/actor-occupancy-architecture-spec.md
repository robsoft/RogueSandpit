# Actor occupancy architecture — brief spec

- Initial NPC placement and live NPC movement use the same flattened-map terrain and occupancy rules.
- NPCs spawn only on walkable room floor, never on obstacles, corridors, the entrance, or another living NPC.
- A single map query determines whether an NPC may enter a cell, including terrain, living NPC, and player blocking.
- Dead NPCs do not block placement or movement.
- Pathfinding may still target the player's occupied cell while intermediate actor collisions remain blocked.
- This refactor must not change NPC counts or turn behavior.

