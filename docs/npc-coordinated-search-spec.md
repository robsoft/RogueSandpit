# NPC coordinated search — brief spec

- The spotting NPC continues to pursue the observed player position.
- Alerted allies receive distinct walkable search cells around that position in stable cardinal order.
- Assignments are deterministic and use only the single location contained in the alert.
- If there are more allies than distinct adjacent cells, remaining allies fall back to the observed cell.
- Existing evidence priority still decides whether an ally accepts an assignment.

