# Developer loadout specification

## Goal

Provide a deterministic, zero-turn testing shortcut for HUD, inventory, equipment, item-action, and combat-state work.

## Behaviour

- F11 restores a living player to maximum health.
- It replaces the current inventory with a full representative eight-item set: steel axe, chain mail, hunter bow, healing potion, brass key, hunting trap, smoke bomb, and fire bomb.
- It equips the supplied melee weapon, armor, and ranged weapon.
- It does not advance the simulation or NPC phase.
- It resets the real-time idle countdown and records a developer-loadout event.
- It is a development convenience, not a discoverable player mechanic or balance feature.
