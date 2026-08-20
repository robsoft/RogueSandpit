# Player responsibility refactor — brief plan

1. Add explicit player placement/relocation methods and use them in runtime/game-state code.
2. Add selected-item action methods that expose outcomes without embedding UI messages.
3. Reduce direct inventory/equipment/position mutation in `GameState`.
4. Extend behavioral tests, update architecture notes, run the full suite and smoke-test.

