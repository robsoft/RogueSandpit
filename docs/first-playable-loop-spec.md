# First Playable Loop: Brief Spec

## Goal

Turn the current movement prototype into a small, complete rogue-like loop: enter the map, fight enemies, retrieve the special item, return to the entrance, and either win or die.

## Occupancy and movement

- A map cell may contain at most one living actor.
- The player and living NPCs cannot move through one another.
- NPCs cannot stack with other NPCs.
- Terrain walkability and actor occupancy are separate concepts with one authoritative query for each.
- Movement remains cardinal and turn-based; attempting a blocked move still consumes a turn.
- Actor speed is one cell per turn. The existing fractional `Speed` property is not part of this milestone.

## Combat

- Moving toward an adjacent NPC performs a melee attack instead of movement.
- The NPC takes the player's damage value.
- A killed NPC no longer blocks movement, acts, or renders.
- NPCs continue to attack the player when adjacent.

## Objective and end states

- The special is placed on a reachable floor cell away from the entrance.
- Entering its cell collects it.
- Returning to the entrance with the special wins the game.
- Reaching zero health loses the game.
- SPACE starts a fresh game after either outcome.

## Presentation

- A compact HUD shows health, damage, and whether the special is carried.
- Win and game-over states remain visible and explain how to restart.
- Primitive visuals are acceptable; adding production art is out of scope.

## Out of scope

- Inventory and loot beyond the single special item.
- Doors and keys.
- A*, line of sight, advanced enemy behavior, animation, and sound.
- Balancing beyond making the loop functional.

