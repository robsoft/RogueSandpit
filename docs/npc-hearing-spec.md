# NPC hearing — brief spec

- Noisy player/world actions create a location and hearing radius:
  - combat: 10 cells;
  - opening or unlocking a door: 6 cells;
  - dropping an item: 4 cells.
- Active NPCs in Manhattan range investigate the noise location without gaining line-of-sight knowledge.
- Pursuing NPCs ignore noise, and direct-sighting or ally-report investigations are not replaced by lower-confidence noise.
- NPC attacks also create combat noise.
- The event log reports how many otherwise-unaware NPCs a noise drew.
- After reaching a noise location, NPCs use the existing local search and eventually return to wandering.

