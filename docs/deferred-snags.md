# Deferred snags

Small observed issues worth revisiting after the current presentation work. These are investigation notes, not committed feature scope.

## Clustered doors at short corridor and room intersections

Procedural generation can create short corridors and room intersections with several doors bunched into a very small area. This is visually noisy and can make movement feel over-segmented.

Later, investigate selectively omitting doors where nearby door density exceeds a small threshold. Some corridor-to-room connections may remain permanently open rather than every connection receiving a doorway.

Before changing generation, verify:

- Every generated objective and required return route remains reachable.
- Locked-door and entrance-key guarantees still make sense.
- Door removal cannot isolate a room or create an invalid entrance/exit.
- NPC pathfinding and closed-door opening continue to work.
- Fog-of-war room/corridor discovery remains coherent across an open connection.
- Sound propagation still reflects the intended topology.
- Doorway-strength trail clues are not incorrectly assigned to open corridor joins.
- Seeded maps remain deterministic.
- Clusters are reduced without removing tactically useful doors indiscriminately.

A likely approach is a post-generation doorway-pruning pass which scores local clusters, preserves mandatory/locked doors, tests topology before removal, and converts selected doorway cells to ordinary floor.

## Selecting within a one-item inventory

When the inventory contains fewer than two selectable items, Select Previous Item and Select Next Item have no meaningful result. Treat either command as a silent no-op: do not change selection, add an event-log entry, or consume a player turn. This must hold whether the inventory panel is open or the selection command is used during normal play.

Add focused simulation tests for empty, one-item, and multi-item inventories when this is addressed. Multi-item selection should retain its existing wrap-around behaviour.

## Inventory selection, equipment, and throwing clarity

The inventory popup does not yet communicate item state clearly enough, particularly when preparing smoke bombs and fire bombs. Review its visual language so the player can immediately distinguish:

- The currently selected inventory slot.
- Items currently equipped as melee weapon, ranged weapon, or armor.
- The selected item that will be used by the next throw command; throwable items may not actually need a separate "equipped" state, but the intended action must be obvious.
- Items which can be consumed, equipped, thrown, or used in some other way.

Consider Enter as a context-sensitive equip/unequip toggle while the inventory popup is open. Define its behaviour for non-equippable items explicitly rather than silently implying that bombs, potions, and ordinary equipment share the same state. Preserve the existing dedicated action key as a remappable alternative if useful.

Because the popup already labels its eight slots `1` through `8`, consider making the number row direct selection shortcuts. Empty or unavailable slots should be silent no-ops and must not consume a turn. Decide separately whether choosing a slot is purely UI navigation or also closes the popup/performs an action; the safer default is selection only.

Throwing currently accepts any selected item, including equipment such as an axe. Keep that freedom, but give non-special thrown items appropriate impact damage derived from their item category and power. Verify what happens when an equipped item is thrown: it should leave the inventory, be unequipped consistently, land or be recoverable under the normal throwing rules, damage the target at most once, and consume exactly one turn. Smoke bombs, fire bombs, distractions, ordinary weapons, armor, consumables, and keys need focused coverage so permissive throwing cannot create state inconsistencies.
