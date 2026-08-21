# NPC awareness confidence — brief spec

- Every investigation has a positive confidence value derived from its evidence source and the NPC's persistence.
- Direct sight begins strongest, an ally report is next, and noise begins weakest.
- Confidence falls once per investigating turn, including travel toward the reported location.
- An NPC whose confidence reaches zero abandons the investigation and returns to wandering.
- Fresh evidence of equal or greater priority replaces the target and refreshes confidence; weaker evidence cannot distract an NPC.
- Reacquiring direct sight restores pursuit and full direct-sighting confidence.
- Debug inspection exposes current confidence alongside the evidence source.

