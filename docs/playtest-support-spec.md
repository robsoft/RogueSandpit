# Playtest support specification

## Goal

Make an unfinished build easier to learn, reproduce, and report on without depending on final UI or assets.

## Help screen

- Add Help to the pause menu.
- Present the active gameplay bindings rather than a duplicated hard-coded key list.
- Group movement, inventory/combat, world interaction, and fixed developer/application controls on one primitive screen.
- Escape or Enter returns to pause; opening Help never advances the simulation or real-time countdown.

## Completed-run reports

- On the first transition to victory or defeat, write one versioned JSON report beneath the per-user `RogueSandpit/run-reports` directory.
- Include timestamp, outcome, seed, runtime mode/interval, final player state, and the complete structured run statistics.
- Use a unique timestamp-and-seed filename so reports do not overwrite one another.
- Reporting is best-effort: an I/O failure must not interrupt the terminal screen.
- Show the saved filename, or a concise failure message, on the terminal report.

## F1 player inspection

Hovering the player cell in F1 mode identifies PLAYER and displays position, health, damage, defence, equipment, objective, and active effects. Existing NPC and cell inspection remains unchanged.

## Acceptance

- Help reflects remapped bindings immediately and remains modal/turn-safe.
- Each completed run creates at most one report during the current application session.
- Report serialization and I/O failure paths have focused tests.
- Player hover works only through the existing F1 diagnostic path.
- Existing tests and application-screen invariants remain green.
