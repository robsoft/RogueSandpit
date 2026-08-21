# Optional real-time turn mode — brief spec

- `F12` toggles real-time mode without consuming a turn.
- While enabled, one second without a successful player turn submits the existing wait command automatically.
- Timer-generated waits do not add `PLAYER WAITS` to the event log; explicit manual waits still do.
- Successful turns reset the countdown; invalid and non-turn commands do not.
- The countdown pauses while inventory or a directional prompt is open, while the window lacks focus, and after the game ends.
- F1 debug mode shows the mode state and remaining time; normal play keeps the timer hidden. The interval is configurable with `--turn-seconds`.
