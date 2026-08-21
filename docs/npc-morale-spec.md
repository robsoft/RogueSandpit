# NPC morale — brief spec

- NPCs have an immutable morale profile and an explicit live morale state.
- Crossing a health threshold after taking damage causes a deterministic archetype reaction.
- Goblins flee readily, Wretches flee when badly hurt, Orcs flee only near death, Skeletons are fearless, and wounded Trolls become enraged.
- Fleeing NPCs choose a reachable retreat destination away from their last reliable threat position; hidden player movement does not update it.
- Reaching the retreat destination changes `Fleeing` to `Shaken`; another damaging hit may trigger a fresh reaction.
- Enraged Trolls deal additional melee damage.
- F1 inspection exposes morale and retreat destination.

