# Optional real-time turn mode — brief spec

- `F12` toggles real-time mode without consuming a turn.
- While enabled, one second without a successful player turn submits the existing wait command automatically.
- Successful turns reset the countdown; invalid and non-turn commands do not.
- The countdown pauses while inventory or a directional prompt is open, while the window lacks focus, and after the game ends.
- The HUD shows mode state and remaining time. The interval is configurable with `--turn-seconds`.

