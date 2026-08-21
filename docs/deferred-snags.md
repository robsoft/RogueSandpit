# Deferred snags

Small observed issues worth revisiting after the current presentation work. These are investigation notes, not committed feature scope.

## Clustered doors at short corridor and room intersections

Procedural generation can create short corridors and room intersections with several doors bunched into a very small area. This is visually noisy and can make movement feel over-segmented.

Later, investigate selectively omitting doors where nearby door density exceeds a small threshold. Some corridor-to-room connections may remain permanently open rather than every connection receiving a doorway.

Before changing generation, verify:

- Every generated objective and required return route remains reachable.
- Locked-door and entrance-key guarantees still make sense.
- Door removal cannot isolate a room or create an invalid entrance/exit.
- NPC pathfinding and closed-door opening continue to work.
- Fog-of-war room/corridor discovery remains coherent across an open connection.
- Sound propagation still reflects the intended topology.
- Doorway-strength trail clues are not incorrectly assigned to open corridor joins.
- Seeded maps remain deterministic.
- Clusters are reduced without removing tactically useful doors indiscriminately.

A likely approach is a post-generation doorway-pruning pass which scores local clusters, preserves mandatory/locked doors, tests topology before removal, and converts selected doorway cells to ordinary floor.
