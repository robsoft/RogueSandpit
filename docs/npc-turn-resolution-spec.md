# Fair NPC turn resolution — brief spec

- The active NPC set is snapshotted at the start of each NPC phase.
- Initiative rotates through that snapshot each turn, so map-list position does not grant permanent priority when actors contest a cell.
- Each eligible NPC still acts at most once, and player death immediately ends the phase.
- The scheduler is framework-independent and provides a seam for future simultaneous-intent resolution.

