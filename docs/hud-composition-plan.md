# HUD composition plan

1. Define fixed native-canvas rectangles and section boundaries for the sidebar, context strip, and status bar.
2. Replace the normal-play dense HUD line with sectioned player, equipment, inventory, objective, effect, and event information.
3. Move directional prompts into the context strip and show a quiet control hint when no prompt is active.
4. Limit the bottom bar to turn and real-time/turn-based mode state.
5. Retain the current F1 diagnostic presentation as a separate rendering path.
6. Build, run the full suite, and smoke-test normal, inventory, prompt, real-time, end-state, and F1 presentation.
