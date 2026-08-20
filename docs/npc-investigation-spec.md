# NPC Investigation: Brief Spec

## Goal

Make a chase persist after line of sight breaks by having NPCs investigate the last position where they saw the player.

## Behaviour

- NPC awareness has three current states: `Unaware`, `Pursuing`, and `Investigating`.
- While the player is visible, the NPC updates its last-known player position and pursues normally.
- When sight is lost, the NPC paths toward that last-known position rather than immediately wandering.
- Reacquiring sight updates the target and returns the NPC to direct pursuit.
- On reaching the last-known position without reacquiring the player, the NPC clears the target and resumes wandering.
- A temporarily blocked route does not make the NPC forget its target; it waits and retries next turn.
- Direct pursuit renders orange-red, investigation renders yellow, and unaware NPCs retain their existing colours.

## Notes

- Perception occurs at turn boundaries. NPCs remember the last cell where the player was actually visible.
- `HasSeenPlayer` remains historical; current awareness and last-known position are separate properties.

## Out of scope

- Searching multiple nearby cells, predicting which exit the player chose, shared awareness, hearing, or timed memory decay.

