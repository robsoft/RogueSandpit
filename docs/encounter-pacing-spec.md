# Encounter pacing specification

## Generation depth

- Calculate cardinal traversable distance from the entrance after terrain and doors are generated.
- Classify reachable cells and rooms as shallow, middle, or deep across the minimum-to-maximum reachable room-floor distance span, so long entrance corridors cannot eliminate the shallow band.
- Expose cell distance and depth band for tests and F1 inspection.
- All calculations remain deterministic for the map seed.

## Safer opening

- No generated NPC may begin within ten traversable steps of the entrance.
- The entrance key remains immediately available and no generated active hazard is placed in the protected approach.
- The rule constrains initial placement only; alerted NPCs may subsequently pursue the player into the safe area.

## Threat curve

- Shallow placement strongly favours Wretches and Goblins and excludes Trolls.
- Middle placement uses a balanced archetype mixture with occasional Trolls.
- Deep placement favours Orcs and Trolls while retaining some weaker variety.
- The objective room contains at least two generated NPCs where eligible cells permit it.
- NPC-held equipment uses the cell's depth band to select equipment tier.

## Reward curve

- Six generated loot placements plus the entrance key remain the baseline.
- Healing and basic melee equipment appear shallow, armor and a trap appear around the middle, and a high-tier ranged weapon plus a useful consumable appear deep.
- Generated equipment records tier explicitly and uses tier 1/2/3 for shallow/middle/deep placement.
- Placement falls back safely when an unusually shaped map has no eligible cell in the requested band.

## Diagnostics and acceptance

- F1 hover reports entrance distance and depth band.
- Seeded tests cover the entrance exclusion, objective guards, stronger deep population, depth-tiered loot, reachability, occupancy, and complete generation reproducibility.
