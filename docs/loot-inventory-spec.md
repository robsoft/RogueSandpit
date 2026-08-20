# Loot and Inventory: Brief Spec

## Goal

Add a small but extensible item loop: find loot, carry a limited amount, use consumables, equip a weapon, and receive drops from defeated NPCs.

## Items and placement

- The first item types are healing potions, weapons, and keys.
- Ground items occupy walkable cells but do not block actors or pathfinding.
- Initial loot is placed on reachable floor cells without overlapping the entrance, special, NPCs, or other loot.
- Defeated NPCs may drop one carried item on their cell.
- Debug hover inspection identifies ground items.

## Inventory

- The player has an eight-item inventory.
- Entering a cell automatically picks up its item when capacity is available.
- If inventory is full, the item remains on the ground and an event explains why.
- Keys are carried for the later doors milestone but have no active use yet.

## Using items

- `H` uses the first healing potion, restoring health without exceeding maximum health.
- `E` equips the first unequipped weapon, replacing the current weapon.
- Using or equipping an item consumes a turn so NPCs respond normally.
- Equipped weapon power contributes to player damage; the HUD shows inventory count and weapon.

## Out of scope

- Inventory menus, item dropping, armor, currency, stacking, item rarity, identification, or locked-door behavior.

