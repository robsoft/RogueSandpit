# Fog of war — brief plan

1. Track current visibility and persistent discovery per map cell.
2. Calculate player field-of-view using range-limited line of sight that includes blocking endpoints.
3. Refresh visibility from game actions and apply a fog overlay to normal rendering.
4. Add deterministic visibility tests and update project notes.

