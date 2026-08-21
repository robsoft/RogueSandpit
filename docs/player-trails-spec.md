# Recent player trails — brief spec

- Each successful player move leaves a clue at the departed cell pointing to the entered cell.
- Clues last for twelve completed turns and the map retains at most twenty-four.
- Trails are invisible in normal play and rendered only in F1 developer mode.
- An investigating NPC can discover the newest clue on its cell or a cardinally adjacent cell.
- Each NPC reacts to a particular clue only once; multiple NPCs may independently discover the same clue.
- A discovered clue becomes fresh physical evidence, redirects the investigation, and refreshes confidence.
- Pursuing and unaware NPCs do not inspect trails.

