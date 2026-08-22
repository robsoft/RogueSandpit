# Save and restore game specification

## Goal

Provide one dependable local save slot for pausing a run and restoring it later. The save is a versioned JSON document in the existing per-user RogueSandpit application-data directory, written atomically as `save-game.json`.

## Saved state

The snapshot preserves the generated world and all mutable simulation state: player, inventory and equipment, NPCs and their behaviour memory, doors, objective, loot, traps, trails, environmental effects, visibility, turn scheduling, event history, run statistics, and the random sequence required for faithful continuation.

Application-only transient state is not saved. Loading closes inventory and directional prompts, clears partial real-time countdown progress, and returns to a paused game. Runtime settings and key bindings remain owned by `settings.json`.

## Interaction

- The pause menu contains Save Game and Load Game.
- Saving is available only for an active run and leaves the game paused.
- Loading replaces the current run only after the save has been fully read and validated.
- Success or failure is shown in the pause screen without spending a turn.
- A missing, corrupt, or unsupported save never crashes or damages the current run.

## Format and safety

- The root document includes an explicit format version.
- Runtime models are not serialized directly; purpose-built snapshot records form the persistence contract.
- Writes use a temporary file followed by replacement/move so a failed write retains the previous valid save where possible.
- Restoration validates coordinates, identifiers, enum values, references, and required collections before publishing the new state.

## Acceptance

- A save/load round trip restores representative state from every simulation subsystem.
- Continuing from a restored save uses the same future random sequence as the original run.
- Saving and loading cost no turns and cannot advance real-time simulation.
- Missing, malformed, and unsupported-version files are handled safely.
- The full existing rule suite remains green.
