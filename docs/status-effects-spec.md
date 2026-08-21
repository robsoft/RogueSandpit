# Timed status effects — brief spec

- Players and NPCs own a collection of explicit timed effects with type, remaining actor turns, power, and source.
- `Stunned` skips the affected actor's next action.
- `Bleeding` deals its power as damage at the start of each affected actor turn.
- Reapplying an effect refreshes to the longer duration and stronger power rather than creating duplicate rows.
- Effects expire after their remaining turns reach zero.
- Status damage can kill; NPC status deaths use the normal held-loot drop rules.
- F1 inspection exposes NPC effect type and remaining duration; the HUD summarizes player effects.

