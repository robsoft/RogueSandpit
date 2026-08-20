# NPC door opening — brief spec

- Pursuing and investigating NPCs treat closed doors as viable path cells.
- Reaching a closed door opens it and consumes that NPC's turn; movement happens later.
- Locked doors remain impassable to NPCs and force pathfinding to seek another route.
- Unaware wandering NPCs do not open doors.
- NPC door openings appear in the event log and existing door rendering updates immediately.

