# Presentation and UI roadmap

## Purpose

This document records the agreed next phase after the playable systems-prototype milestone. Broad simulation expansion is paused while the project establishes its visual language, asset workflow, application screens, and production UI approach.

The work begins with a deliberately small hand-drawn tile experiment. It then introduces a reproducible MonoGame asset pipeline, a screen/overlay architecture, and Gum through one representative UI slice before committing to a full presentation implementation.

## Agreed principles

- Begin with crude, coherent hand-drawn tiles rather than waiting for final art.
- Use a limited indexed palette and integer-scaled pixel artwork.
- Judge artwork inside the game at its real presentation size as early as possible.
- Keep editable art sources separate from exported runtime assets.
- Use atlases at runtime, but organise editable actor files for future animation.
- Keep simulation rules independent of the presentation and UI libraries.
- Treat full screens, modal overlays, and the non-modal HUD as different UI responsibilities.
- Prove Gum with one demanding existing interface before rebuilding every screen.
- Preserve current turn-cost, modal-pause, focus-pause, and fog-of-war invariants.

## Milestone 1: nine-tile visual slice

### Artist handoff

The initial source is one Aseprite document with:

- A 96×96 pixel canvas
- A 32×32 pixel grid arranged as three columns by three rows
- Indexed colour mode
- A deliberately limited palette, approximately 24–32 colours
- No antialiasing
- A transparent background for every sprite or overlay that does not replace terrain

Suggested atlas layout:

| Atlas position | Tile | Rendering role |
|---|---|---|
| Row 0, column 0 | Floor | Opaque base terrain |
| Row 0, column 1 | Wall | Opaque base terrain |
| Row 0, column 2 | Player | Transparent actor sprite |
| Row 1, column 0 | Orc | Transparent actor sprite |
| Row 1, column 1 | Healing potion | Transparent ground item |
| Row 1, column 2 | Smoke | Transparent environmental overlay |
| Row 2, column 0 | Open door | Transparent terrain feature |
| Row 2, column 1 | Closed door | Transparent terrain feature |
| Row 2, column 2 | Fire | Transparent environmental overlay |

The layout may change before integration, but the final positions must be recorded and remain stable once referenced by code.

The editable file should be retained as `.aseprite`. Export a PNG at exactly 96×96 pixels without smoothing or resizing. A temporary floor-preview layer may be used while drawing actors and overlays, but must be hidden during export.

### Palette guidance

Choose colours by purpose rather than filling an arbitrary quota:

- Shared dark outline and shadow colours
- Stone and neutral values
- Warm browns
- Greens
- Reds and oranges
- Blues
- Yellow or gold accents
- Pale highlights

Individual tiles should normally use only a subset of the palette. Reserve the brightest accents for important information, especially if the retrieval objective remains yellow.

### Repository asset convention

Use separate source and runtime locations:

```text
ArtSource/
  palette.aseprite
  prototype-slice.aseprite

RogueSandpit/Content/Sprites/
  prototype-slice.png
```

`ArtSource` is version-controlled working material. `Content/Sprites` contains exported files consumed by MonoGame. The source file is authoritative for editing; the PNG is authoritative for a particular game build.

### Engineering work

Once the atlas is supplied:

1. Add the exported PNG to `Content.mgcb` as a texture asset.
2. Load it once through MonoGame's `ContentManager` as a `Texture2D`.
3. Introduce named sprite identifiers or atlas-region definitions rather than scattering numeric row and column values through rendering code.
4. Draw a source region using `Rectangle(column * 32, row * 32, 32, 32)`.
5. Render terrain first, then trails/effects/items/traps, then actors, then fog and UI as appropriate.
6. Use point sampling so scaling does not blur pixel artwork.
7. Replace only the nine corresponding primitives. Retain primitive fallbacks for content not yet represented by artwork.
8. Verify normal fog-of-war and F1 diagnostics still expose the intended information.

The current logical map cell is smaller than the 32×32 source artwork. Source art will be scaled into the existing destination cell, so one-pixel details may disappear. The first integration is intended to test whether the current native canvas and cell size remain suitable.

### Acceptance criteria

- The atlas builds through the MonoGame content pipeline on supported development platforms.
- All nine regions render at the correct coordinates with no colour bleeding or smoothing.
- Transparent sprites reveal the underlying floor correctly.
- Smoke and fire remain overlays rather than replacing terrain.
- The game remains usable in normal and F1 modes at window scale 1, default scale 2, and fullscreen.
- Existing simulation tests continue to pass.
- We explicitly decide whether 32×32 sources and the 800×600 native canvas are a good foundation before drawing the remaining asset set.

## Milestone 2: production asset conventions

After approving the visual slice:

1. Keep the MonoGame packages and project-local content tools pinned to the tested stable version `3.8.5.1`.
2. Document palette, transparency, grid, naming, export, and point-sampling conventions.
3. Decide whether door art is orientation-neutral or needs horizontal and vertical variants.
4. Decide how actor awareness, morale, bleeding, and stun are communicated: sprite variants, icons, tinting, animation, or debug-only overlays.
5. Decide whether the entrance needs its own base tile or transparent marker.
6. Expand from the prototype sheet into a small number of runtime atlases such as terrain, actors, items, and effects.
7. Keep actor source documents separate when animation starts, even if their exported frames are combined into an actor atlas.

Likely long-term source structure:

```text
ArtSource/
  palette.aseprite
  terrain.aseprite
  doors.aseprite
  items.aseprite
  effects.aseprite
  player.aseprite
  orc.aseprite
  goblin.aseprite
  skeleton.aseprite
  troll.aseprite
  wretch.aseprite
```

Animation is explicitly deferred. The conventions should make animation possible without requiring it now.

## Milestone 3: application screen architecture

Before introducing a UI framework widely, separate application navigation from `GameWrapper`.

The application should recognise full-screen states similar to:

```text
Main menu
Playing
Paused
Options
Game over
Victory
```

The exact names are implementation details. The important boundary is that application code—not Gum—owns navigation, active simulation state, and pause policy.

### UI categories

Full screens replace gameplay presentation:

- Intro/title and main menu
- Options and controls
- Game-over and victory screens
- Credits, if later required

Modal overlays sit above gameplay and block normal game input:

- Pause menu
- Inventory
- Directional action prompts
- Confirmation dialogs
- Item details or help panels
- Key-binding capture

The non-modal HUD remains visible during gameplay:

- Health and equipment
- Objective status
- Active effects
- Event log
- Turn or real-time mode indication where appropriate

### Architectural constraints

- The screen coordinator decides which screen or overlay is active.
- Simulation models do not reference Gum or MonoGame UI controls.
- UI events produce application commands; they do not directly mutate unrelated simulation internals.
- Opening a modal overlay preserves the current rule that timed turns pause.
- Lost focus, terminal outcomes, and directional prompts retain their existing pause semantics.
- Mouse coordinates continue through the aspect-ratio-aware viewport mapping.
- The initial Gum layout uses the same 800×600 native coordinate space unless Milestone 1 demonstrates that the native resolution must change.

## Milestone 4: Gum proof of concept

Adopt `Gum.MonoGame` with Gum Forms, the current visuals version, and the recommended MonoGame + Forms code-generation target. Use the Gum editor rather than a code-only layout for production screens, while retaining normal C# view models and navigation.

The proof of concept will replace the existing inventory overlay because it exercises:

- Repeated item rows or slots
- Selection and focus
- Equipped state
- Item details
- Keyboard navigation
- Mouse interaction
- Modal simulation pausing
- Data binding
- Theme and reusable component styling

Use view models through Gum's binding support. View models may inherit Gum's `ViewModel` helper or implement `INotifyPropertyChanged`, but they must expose presentation-ready state rather than become a second game simulation.

### Proof acceptance criteria

- Inventory behaviour remains functionally equivalent to the current implementation.
- Keyboard selection works and does not leak movement commands into gameplay.
- Mouse mapping works at every supported window mode.
- Opening inventory pauses real-time turns; closing it resumes the correct mode.
- The UI remains legible at the native canvas and all integer scales.
- Styling can be changed without rewriting the inventory rules.
- The integration does not force Gum dependencies into framework-independent tests or models.

If the proof is unpleasant to maintain, difficult to style, or unreliable across input and scaling modes, stop and reassess before converting other interfaces.

## Milestone 5: full presentation shell

If Gum passes the proof, implement in small reviewed slices:

1. Shared visual theme and reusable controls
2. Pause overlay
3. Intro/title and main menu
4. Options and controls screens
5. Directional prompts and confirmation dialogs
6. Game-over and victory screens
7. Production HUD and active-status presentation
8. Event-log redesign and any tutorial/help presentation

Options should eventually cover display mode, integer scale where applicable, real-time mode and interval, audio once present, input bindings, and accessibility choices. Controller support and rebindable controls remain separate features rather than implicit requirements of the first Gum milestone.

## Deferred asset inventory

After the nine-tile slice is approved, the remaining current simulation requires artwork for:

- Entrance/return point and yellow objective
- Goblin, Skeleton, Troll, and Wretch
- Locked door and any chosen door orientations
- Iron sword, steel axe, war hammer
- Short bow, hunter bow, war bow
- Leather armor, chain mail, plate armor
- Brass key, bandage, smoke bomb, and fire bomb as ground/inventory items
- Hunting trap, snare, and alarm trap, including placed forms if visually distinct
- Genuine and false directional trail overlays
- Bleeding and stunned indicators
- Pursuing, investigating, fleeing, and enraged communication if player-facing
- Inventory slots, selection/equipment markers, HUD icons, and interaction indicators

Fog-of-war shading, debug paths, line-of-sight lines, valid/invalid targeting, and selection rectangles may remain code-rendered unless the chosen theme benefits from artwork.

### Minor presentation follow-ups

The earlier player-hover diagnostic follow-up is complete: F1 inspection identifies the player and exposes health, equipment, inventory, objective, and status-effect state.

## Deliberately out of scope

- Player or NPC movement animation
- Attack, damage, death, smoke, or fire animation
- Tiled integration for map authoring
- Hand-authored map replacement for procedural generation
- Complete asset production before validating the representative slice
- Simultaneous NPC intent resolution or unrelated simulation expansion

Tiled may later be useful for prefab rooms, special encounters, tileset testing, or tutorial maps. It is not needed for the current procedurally generated level or the initial sprite workflow.

## Resume point

The prototype atlas, 18×16 scrolling viewport, structured normal-play HUD, and application-screen boundary are now implemented. The next visual input is expanded artwork following the approved 32×32 conventions. The next UI engineering milestone is the Gum inventory proof after the desired visual theme is clearer.

Audio is planned separately in [the audio roadmap](audio-plan.md), beginning with structured presentation events and an eight-cue gameplay slice once suitable licensed assets are available.
