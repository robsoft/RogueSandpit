# Richer item impacts — brief spec

- Throws trace up to six cells and report both the first living NPC struck and the last valid landing cell before it.
- A thrown weapon deals its power as immediate damage and inflicts three actor-turns of `Bleeding` at power two.
- The weapon then lands as recoverable ground loot.
- A thrown healing potion shatters and is consumed instead of becoming ground loot.
- Other items retain their existing recoverable impact-noise behavior.
- Hunting traps retain their damage and also apply one actor-turn of `Stunned` to a surviving victim.
- Impact and status deaths preserve normal NPC loot and event behavior.

