# Fog of war — brief spec

- Normal play distinguishes visible, previously discovered, and undiscovered cells.
- The player sees cells within 12 tiles when terrain and doors permit line of sight.
- Visible cells become permanently discovered; discovered cells remain dim when no longer visible.
- Undiscovered cells are black. NPCs and ground items are only revealed while currently visible.
- Opaque walls and closed/locked doors are themselves visible when they stop sight beyond them.
- Visibility refreshes after movement and door interaction.
- F1 debug mode remains omniscient and keeps all existing overlays.

