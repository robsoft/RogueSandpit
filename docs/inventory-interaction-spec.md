# Inventory interaction specification

- Previous/next selection is a silent, zero-turn no-op with fewer than two items; multi-item selection still wraps.
- While inventory is open, number-row or numpad `1`–`8` selects an occupied slot without taking a turn. Empty and already-selected slots do nothing.
- Enter and the remappable Equip action toggle the selected weapon, armor, or ranged weapon. Successful equip and unequip actions consume one turn.
- The popup distinguishes the selected throw target from melee, armor, and ranged equipment and shows a contextual action hint.
- Any selected item can be thrown. Removing an equipped item also unequips it, and recoverable items use the existing landing rules.
- Weapons retain full-Power impact damage and bleeding. Armor and bows deal half Power rounded up; traps deal one third rounded up; small ordinary items deal one damage.
- Smoke and fire bombs remain effect-driven so their effects are not double-counted as blunt impact damage.

Selection-only actions never create log entries or advance the simulation. Invalid equip and throw attempts retain their existing zero-turn behaviour.
