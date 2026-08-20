# Map topology integrity — brief spec

- Distinct rooms must not share a directly traversable edge unless a generated corridor occupies that threshold.
- Corridors must retain their identity in the flattened cell map instead of being overwritten by later room painting.
- Every room, the special objective, and generated loot must remain reachable from the entrance when openable doors are treated as traversable.
- Generation invariants must hold across a representative range of deterministic seeds.
- This milestone changes topology correctness only, not the overall BSP layout style.

