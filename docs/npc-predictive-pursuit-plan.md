# NPC predictive pursuit — brief plan

1. Capture successive visible player positions and their latest cardinal movement vector per NPC.
2. Add a deterministic map query that projects a traversable continuation.
3. Switch from the last-seen cell to that prediction only when direct sight is lost.
4. Expose the prediction in diagnostics and test both continuation and fallback behavior.

