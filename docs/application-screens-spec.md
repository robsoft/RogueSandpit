# Application screens specification

## Goal

Separate application navigation and simulation pause policy from gameplay input and presentation so future Gum screens can replace primitive views without rewriting game rules.

## Screen states

- `Playing`: gameplay input and simulation are active.
- `Paused`: gameplay remains visible but no turns or real-time progress occur.
- `Help`: binding-aware control reference opened from pause; gameplay remains frozen.
- `Options`: opened from pause; gameplay remains frozen.
- `GameOver`: terminal loss screen.
- `Victory`: terminal win screen.

The game continues to start directly in `Playing` for development convenience. A title/main-menu state is deferred.

## Escape priority

1. Cancel an active directional action.
2. Close the inventory.
3. Return from controls to options, or Help to pause.
4. Return from options to pause.
5. Resume from pause.
6. Pause active gameplay.

Escape no longer exits the application. Quit is an explicit pause-menu command.

## Pause menu

The primitive keyboard menu contains Resume, Help, Save Game, Load Game, Options, Restart This Seed, New Run, and Quit. A compact run-statistics snapshot and save/load feedback appear above it. Up/down changes selection and Enter confirms. Help reflects current bindings; Save/load and restarts are immediate during this prototype phase.

## Terminal report

Victory and Game Over present the completed run's seed, timing, combat, stealth, inventory, door, and trap statistics. Defeat also identifies a concise cause. The terminal menu offers Restart This Seed, New Run, and Quit; arrows and Enter navigate it, while Space remains a direct New Run shortcut.

## Options foundation

The primitive options page supports:

- Real-time turn interval
- Future master volume
- Future effects volume
- Future music volume
- Mute while unfocused
- Back

Up/down changes selection; left/right changes the selected value; Enter activates Back. Audio values are retained runtime settings but do not affect playback until audio is implemented. Display mode remains controlled by launch options for now.

## Invariants

- Pause, Help, controls, and options consume no turn and freeze the real-time countdown.
- Terminal states cannot return to active simulation without restarting.
- Application navigation remains independent of Gum.
- Simulation models do not reference application screens.
- Existing prompt, inventory, focus, and end-state behaviour remains deterministic.
