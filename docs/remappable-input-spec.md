# Remappable input and persistent settings specification

## Goal

Separate gameplay actions from physical keys, provide laptop-friendly defaults, expose runtime remapping through a Controls screen, and persist settings safely between runs.

## Gameplay bindings

- Each gameplay action supports a primary and optional secondary key.
- Space is the primary Wait binding; period and numpad 5 remain accepted through default bindings/fallbacks.
- Directional prompts use the configured movement actions.
- Escape, Enter, menu arrows, F1, F11, and F12 remain fixed application/developer controls so remapping cannot lock the user out.
- A key already assigned to another gameplay action is rejected with a visible conflict message.
- Individual actions and the complete map can be reset to defaults.

## Controls screen

- Open Controls from Options.
- Up/down selects an action or command row.
- Tab chooses whether Enter captures the primary or secondary slot.
- Enter begins key capture.
- Escape cancels capture, then backs out normally when not capturing.
- Backspace resets the selected action.
- Delete clears the selected secondary binding; a primary binding cannot be cleared when no secondary exists.
- Reset All and Back are explicit rows.

## Persistence

- Store runtime settings and bindings as JSON in the platform per-user application-data directory.
- Missing files use defaults.
- Missing or unknown fields remain forward/backward tolerant.
- Invalid action/key names and corrupt JSON are ignored safely and never prevent startup.
- Save after a runtime option or binding actually changes.
- Tests use isolated temporary paths rather than the real user profile.

## Acceptance

- No gameplay command depends directly on hard-coded physical keys.
- Space, period, and numpad 5 can wait by default on a laptop or full keyboard.
- Remapped movement also answers directional prompts.
- Conflicts, reset, clearing, capture cancellation, and persistence are tested.
- Pause/options/control screens consume no turns and freeze real-time progress.
