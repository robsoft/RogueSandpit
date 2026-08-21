# Adjacent door toggle — brief spec

- `C` operates an immediately adjacent unlocked door without moving the player.
- A closed door opens; an open door closes. Either result consumes one turn, refreshes visibility, and creates normal door noise.
- If exactly one adjacent door can be operated, `C` acts immediately. Multiple candidates prompt for an arrow direction.
- Locked doors and open doorways occupied by living NPCs are not operable and do not create ambiguity.
- With no operable adjacent door, or an invalid selected direction, no turn is consumed. Escape cancels directional selection.

