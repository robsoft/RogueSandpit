# NPC Pursuit: Brief Spec

## Goal

Replace purely random enemy movement with understandable pursuit using line of sight and cardinal A* pathfinding.

## Behaviour

- NPC melee attacks require cardinal adjacency, matching player movement.
- An NPC sees the player when they are within 12 cells and no wall blocks the line between them.
- A seeing NPC takes one cardinal step along a shortest available path toward the player.
- Paths may cross floor, door, and special cells, but not walls or living NPCs.
- The player cell is a valid path destination; the NPC attacks rather than entering it.
- If the player is not visible or no path is available, the NPC uses its existing wandering behaviour.
- NPCs still take exactly one action per player turn and never overlap.

## Technical requirements

- Line of sight and pathfinding are simulation APIs with no rendering dependency.
- A* returns an empty path when the destination is unreachable.
- Tests cover clear/blocked sight, shortest-path detours, pursuit, cardinal attacks, and occupied paths.

## Out of scope

- Remembering and investigating the player's last known position.
- Hearing, stealth, field-of-view cones, ranged combat, or differing sight ranges by species.
- Opening locked doors or coordinated group tactics.

