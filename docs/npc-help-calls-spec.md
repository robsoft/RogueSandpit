# NPC calls for help — brief spec

- A fleeing NPC may call for help once before reaching safety.
- The call creates noise at the caller and shares only its last reliable player position with eligible nearby allies.
- Allies receive coordinated search assignments using existing evidence priority and confidence rules.
- Pursuing allies are not distracted and dead or inactive NPCs do not respond.
- The event log identifies the caller and number of allies alerted.
- A new damage-triggered flight after reaching safety permits a new call; a single retreat cannot loop calls.

