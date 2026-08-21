# Directional actions and door closing — brief spec

- `C` closes the only immediately adjacent open door without an extra prompt.
- With multiple adjacent open doors, `C` enters a directional choice; an arrow closes the door in that direction.
- With no adjacent open door, `C` reports that no door is available and consumes no turn.
- An invalid direction keeps the choice active; Escape cancels it without quitting or consuming a turn.
- Closing a door consumes one turn, blocks sight and movement normally, and creates door noise.
- The directional-action UI is reusable by other actions and shows a concise prompt.

