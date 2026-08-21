# Scrolling map viewport specification

## Goal

Render the existing large procedural world through a readable local camera without changing simulation coordinates, map generation, fog, pathfinding, or NPC behaviour.

## Normal presentation

- Use 32×32 destination tiles at their atlas source size.
- Show a 16×16-cell map viewport occupying the top-left 512×512 pixels of the 800×600 native canvas.
- Reserve the 288-pixel right side and lower 88 pixels for HUD, log, prompts, and later Gum UI.
- Follow the player only after they leave a four-cell inset dead zone.
- Move the camera in whole cells and clamp it to the map boundaries.
- Cull all world drawing to the camera rectangle so off-screen sprites cannot spill into UI space.
- Preserve world-cell fog state while scrolling.

## Developer presentation

- F1 retains the compact whole-map view at the existing 10-pixel cell scale.
- Existing hover inspection, paths, line-of-sight lines, fog bypass, and debug colours remain available.
- F1 does not change the camera or simulation.

## Acceptance

- The player begins in view and remains inside the camera dead zone except where map-edge clamping prevents it.
- The camera reaches and clamps correctly at every world edge.
- World-to-screen transforms and visibility tests are deterministic and covered by unit tests.
- Normal fog, doors, actors, items, traps, trails, smoke, and fire appear only inside the map panel.
- F1 still shows the full map and supports correct mouse inspection.
- Scale 1, default scale 2, and fullscreen retain crisp point sampling and correct aspect-ratio presentation.
