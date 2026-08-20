# NPC shared awareness — brief spec

- When an NPC newly sees the player, it alerts active allies within 8 Manhattan cells.
- An alert shares only the position observed at that moment.
- Alerted allies investigate that reported position; they do not receive subsequent live player coordinates.
- Repeated pursuit turns do not repeatedly broadcast alerts.
- Direct sight overrides an ally report, while an ally report overrides ordinary noise.
- The event log reports the number of allies receiving an alert.

