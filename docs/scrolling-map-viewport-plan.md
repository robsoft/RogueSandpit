# Scrolling map viewport plan

1. Add a framework-light `MapViewport` owning map-panel geometry, tile size, camera origin, dead-zone following, clamping, transforms, and culling.
2. Add focused tests for initial positioning, dead-zone movement, edge clamping, world-to-screen conversion, and visibility.
3. Split normal local rendering from the existing F1 whole-map rendering.
4. Route all normal terrain, feature, effect, item, trap, trail, NPC, player, fog, and grid positions through the viewport.
5. Reserve and draw the sidebar/lower presentation space and relocate the event log in normal mode.
6. Build, run the complete test suite, and smoke-test normal movement plus the F1 transition.
