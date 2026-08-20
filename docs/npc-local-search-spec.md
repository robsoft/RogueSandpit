# NPC local search — brief spec

- An NPC that loses sight still travels to the player's last visible position.
- On arrival it checks the walkable cardinal cells around that position in a stable order.
- It remains `Investigating` throughout the search and exposes its current search target for diagnostics.
- Seeing the player at any point immediately cancels the search and resumes pursuit.
- After checking all available cells it forgets the target and returns to wandering.
- A living actor temporarily blocking a route makes the NPC wait and retry.

