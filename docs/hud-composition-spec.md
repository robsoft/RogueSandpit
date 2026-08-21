# HUD composition specification

## Goal

Settle the information hierarchy and permanent regions of the 800×600 native canvas before adopting Gum or producing final UI art.

## Normal-play layout

- Keep the 512×512 scrolling map unchanged.
- Divide the 288×580 right sidebar into player, equipment, inventory, objective/effects, and event-log sections.
- Use the 512×68 strip below the map for contextual actions and restrained control hints.
- Reduce the 800×20 bottom bar to turn and mode-level information.
- Prefer labelled sections, whitespace, and a small number of semantic colours over the previous single dense status line.

## Developer view

- Preserve the compact F1 map, inspection panel, existing diagnostic HUD information, and current prompt placement.
- The production layout may not obscure or remove development information needed for simulation work.

## Acceptance

- Health, damage, defence, equipment, inventory selection, objective state, effects, and recent events remain visible in normal play.
- Contextual directional prompts occupy the lower map strip and clearly show cancellation.
- The bottom bar shows turn count and current turn mode without duplicating the full player state.
- The layout remains readable at default 2× scale and functional at 1×.
- Inventory and end-state overlays remain usable.
