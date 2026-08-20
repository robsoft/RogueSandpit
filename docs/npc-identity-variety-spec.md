# NPC identity and variety — brief spec

- NPCs have a stable archetype and a generated fantasy name instead of numbered labels.
- Names are deterministic for a generated map seed.
- Orc, Goblin, Skeleton, Troll, and Wretch all generate naturally on maps.
- Each archetype has a distinct health/damage profile:
  - Orc: sturdy all-rounder.
  - Goblin: fragile, moderate damage.
  - Skeleton: balanced but less sturdy than an orc.
  - Troll: toughest and hardest-hitting.
  - Wretch: weakest and most fragile.
- Unaware NPCs have archetype colours; pursuit/investigation colours continue to override them.
- Debug inspection shows archetype, name, health, damage, and awareness.

