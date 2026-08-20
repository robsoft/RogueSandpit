# Window scale option — brief spec

- Internal rendering remains fixed at 800×600.
- A normal launch defaults to a 2× (1600×1200) window for accessibility.
- `--scale 1` launches the original 800×600 debug-sized window.
- `--scale N` and `--scale=N` accept integer scales from 1 through 4.
- Invalid or missing values report a clear command-line error rather than silently changing behavior.
- Resizing and mouse-to-map coordinate conversion continue to use the existing aspect-ratio pipeline.

