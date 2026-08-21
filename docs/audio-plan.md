# Audio roadmap

## Purpose

Add sound without coupling simulation rules to playback technology, event-log text, or the availability of an audio device. The first milestone should establish reusable plumbing and prove it with a small gameplay sound set. Music, ambience, mixing polish, and persisted options follow as separate passes.

The project currently uses MonoGame `3.8.5.1`, which includes relevant `SoundEffect` and music shutdown fixes.

## Principles

- Simulation outcomes produce structured events; they do not play files directly.
- The event log, audio, animation, particles, and other presentation systems may consume the same structured event.
- Never infer audio from formatted event-log strings.
- Game rules do not depend on whether playback succeeds or audio is muted.
- Short effects, looping ambience, and music have separate responsibilities and volume controls.
- Frequently repeated actions eventually receive several subtle variants.
- Every acquired asset must have a recorded source and licence.

## Milestone 1: structured presentation events

Introduce a typed event seam suitable for several presentation systems:

```text
Simulation action
      |
      v
Structured game event
      +-- event-log message
      +-- sound cue
      +-- animation
      +-- particles or screen response
```

A structured event should identify at least:

- Event type
- Relevant world position, when one exists
- Actor or source identity where useful
- Target identity where useful
- Numeric result such as damage where useful

The initial refactor should cover only the actions required by the first audio slice. Existing log behaviour must remain intact while moving those messages onto the structured path.

## Milestone 2: audio service and representative slice

Add a central audio service responsible for:

- Loading MonoGame `SoundEffect` assets through `Content.mgcb`
- Mapping named cues to runtime assets
- Master and sound-effect volume
- Muting
- Safe overlapping playback
- Simple concurrency limits or cooldowns for repetitive cues
- Graceful operation when audio is unavailable

Initial cue vocabulary:

```csharp
public enum SoundCue
{
    DoorOpen,
    DoorClose,
    MeleeHit,
    PlayerHurt,
    ItemPickup,
    BowFire,
    ProjectileImpact,
    TrapTriggered
}
```

These eight cues exercise player and NPC actions, world positions, off-screen events, repeated actions, and several events occurring in a single turn.

Tests should verify that simulation or presentation events request the correct named cue without constructing a graphics or audio device. Runtime smoke tests should verify loading, volume, overlap, focus behaviour, and clean shutdown.

## Milestone 3: positional gameplay audio

Use event world positions and the player position to derive presentation-only audio properties:

- Distance-based volume
- Left/right stereo pan
- Maximum audible range
- Optional later muffling by distance or closed doors

The scrolling viewport makes off-screen audio valuable as player information. Positional playback is independent of the existing NPC hearing rules: NPC hearing remains simulation logic, while speaker output determines what the human player hears.

## Milestone 4: UI audio

Add restrained cues for:

- Menu navigation and confirmation
- Inventory selection
- Equip and use actions
- Invalid or blocked actions
- Opening and closing modal overlays
- Victory and defeat transitions

UI cues are non-positional and should have their own repetition and volume policy.

## Milestone 5: ambience and music

Add longer-lived playback only after short effects are stable:

- Title music
- Gameplay music
- Victory and defeat transitions
- Dungeon or room ambience
- Nearby fire or other environmental loops
- Crossfading and pause/focus behaviour

Looping sounds need explicit ownership and lifecycle management so they fade or stop when the player moves away, changes screen, restarts, or exits.

## Settings

The eventual options model should support:

- Master volume
- Sound-effect volume
- Music volume
- Mute
- Mute while the window is unfocused
- Possibly reduced repetitive sounds

The settings model remains independent of Gum. Gum or another UI layer edits the model but does not own audio policy.

## Asset convention

Before importing the first sounds, decide and document:

- Source and pipeline-ready formats
- Runtime naming convention
- Licence and attribution record format
- Expected loudness and headroom
- Overall style: realistic, retro, exaggerated, minimal, or another coherent direction
- Variant naming for repeated sounds

Suggested source/runtime structure:

```text
AudioSource/
  licences.md
  effects/
  ambience/
  music/

RogueSandpit/Content/Audio/
  Effects/
  Ambience/
  Music/
```

Do not normalize unrelated assets blindly. Audition the first set together and balance perceived loudness in context.

## Candidate expansion cues

After the representative slice, likely gameplay additions include:

- Melee miss or blocked hit
- NPC hurt and death variants
- Player death
- Potion and bandage use
- Armor equip and weapon equip
- Smoke bomb and fire bomb activation
- Fire damage
- Alarm, snare, and hunting-trap variants
- Objective pickup and return
- Locked-door failure and key use
- NPC alert, investigation, retreat, and rage cues where appropriate
- Footsteps or movement cues, if they are not too repetitive

## Deliberately deferred

- Complete sound coverage in the first milestone
- Final mixing and mastering
- Voice acting
- Dynamic music systems
- Procedural audio
- Making simulation depend on playback state
- Treating NPC hearing and player-facing positional audio as the same system

## Asset handoff

The first external input is a coherent set of eight short effects matching the representative cue list, with source/licence information. Variants are welcome but not required for the proof. Once those assets exist, begin with the structured-event seam and audio service on a focused feature branch rather than importing sounds directly into individual gameplay methods.
