# Inventory selection — brief spec

- The inventory tracks one selected item when it is non-empty.
- Left/right bracket cycle selection with wraparound and do not spend a turn.
- `H` uses the selected item when it is a healing potion.
- `E` equips the selected item when it is a weapon or armor.
- `D` drops the selected item; selection moves safely after removal.
- The HUD shows the selected item, and invalid contextual actions explain the problem without spending a turn.

