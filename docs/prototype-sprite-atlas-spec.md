# Prototype sprite atlas specification

## Goal

Prove the first hand-drawn presentation slice in the running game without requiring artwork for every existing model.

## Atlas contract

`prototype-slice.png` is a 96×96 texture containing nine fixed 32×32 regions:

| Row | Column 0 | Column 1 | Column 2 |
|---|---|---|---|
| 0 | Floor | Wall | Player |
| 1 | Orc | Healing potion | Smoke |
| 2 | Open door | Closed door | Fire |

The exported PNG is runtime content. Editable `.aseprite` sources remain under `ArtSource/` and are not processed by MonoGame.

## Behaviour

- Load the atlas once through MonoGame's content pipeline.
- Render atlas-backed terrain before transparent features and actors.
- Render smoke and fire as overlays over their underlying terrain.
- Use point sampling at all window scales.
- Use named atlas regions rather than numeric coordinates at call sites.
- Preserve primitive rendering for locked doors, non-Orc NPCs, other items, traps, trails, diagnostics, and any missing texture.
- Preserve normal fog-of-war and F1 developer behaviour.

## Acceptance

- Content builds and the application launches with the atlas.
- All nine sprites use the agreed region.
- Transparent regions reveal the base terrain.
- The atlas is crisp at scales 1 and 2 and in fullscreen presentation.
- Existing simulation tests pass.
