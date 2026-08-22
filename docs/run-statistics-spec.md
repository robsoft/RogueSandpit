# Run statistics and report specification

## Structured statistics

Each GameState owns a fresh RunStatistics instance. It records successful simulation outcomes rather than parsing event-log text:

- total, deliberate, and automatic real-time turns;
- objective collection and escape turns;
- NPC defeats by archetype and total damage dealt;
- damage received, healing received, melee attacks, ranged shots/hits, and detection episodes;
- maximum simultaneous pursuers and NPCs newly entering pursuit;
- items collected, consumed, dropped, and thrown;
- doors opened, closed, and unlocked;
- traps placed and triggered;
- a concise defeat cause when applicable.

Invalid/cancelled commands and inventory selection do not alter statistics. Damage and defeats caused later by player-created bleeding, fire, or traps are included by reconciling actor state across the completed turn.

## Presentation

- Pause shows seed, turns, objective state, defeats, damage dealt/received, items collected, detections, and maximum pursuers above the existing menu.
- F1 shows a compact live statistics line.
- Victory/defeat uses a dedicated full report with outcome, seed, cause, timing, combat, stealth, inventory, doors, and traps.
- Terminal choices are Restart This Seed, New Run, and Quit; arrows and Enter navigate, Space remains a New Run shortcut.
- All terminal/pause navigation remains simulation-safe and costs no turns.

## Acceptance

- Statistics reset with every restarted or new run.
- Same-seed restart reproduces the world but starts a blank report.
- Counters are covered at rule boundaries, including delayed damage and no-op actions.
- The statistics model remains framework-independent so future UI, balance tooling, history, scoring, or achievements can reuse it.
