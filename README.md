# Rogue Sandpit 

A simple rogue-like game built with Monogame and .net 9. The game features a player character that can move around a grid-based map, collect items, and fight NPCs. The game is designed to be simple and easy to understand, with a focus on the core mechanics of a rogue-like game.

## Monogame, .Net9
builds & runs on Mac and Windows


## Current Status
- Player character can move around the map  
- Map is generated with rooms and corridors  
- NPCs do damage to the player if they're adjacent  
- Player can see entire map & NPCs in debug mode  
- Fully turn-based, so NPCs 'pause' until the player makes a move  
- Player can register a 'move' even if they don't actually move (eg, against a wall), so NPCs will then move  
- Player attacks NPCs by moving into them; defeated NPCs stop acting and blocking cells
- Retrieve the yellow special tile and return it to the entrance to win
- HUD displays health, damage, and objective status
- NPCs pursue the player with line of sight and A* pathfinding, then attack from cardinally adjacent cells
- NPCs remember and investigate the last place they saw the player
- Pursuing NPCs render orange-red and investigating NPCs render yellow during development
- F1 debug mode supports hover inspection plus NPC path and line-of-sight visualization
- A compact event log shows recent combat and objective events
- Reachable potions, weapons, and keys can be collected in an eight-slot inventory
- H uses a healing potion; E equips a carried weapon; defeated NPCs may drop loot
- Closed doors take a turn to open; locked doors require a carried, reusable key
 

## Pressing TODOs
- investigate better placement of the 'special' tile
- continue consolidating initial placement and live actor occupancy
- refactor Player class so it handles it's own moves, knows about Map etc  
- suspect the IsWalkable implementation allows creatures to teleport through walls when rooms are hard-adjacent  
- Names & class variation for NPCs - Markov-chain generation of names? 
- Add richer search behaviour after an NPC reaches the last-known player position
- Graphics!  


## Controls
- Arrow keys (WASD will come) to move the player character
- H to use a healing potion
- E to equip the next carried weapon
- F1 to toggle debug/map viewer  
- In debug mode, hover a cell to inspect it and visualize NPC decisions
- SPACE to generate a new map (effectively restart)
- ESCAPE to quit the game


## Intended Features
- Exits to be shown inside the room (it's not a bug, it's an undesired feature right now)  
- Player character with movement and combat mechanics  
- Items that can be collected and used, with additional item types to come
- NPCs that can be fought and defeated  
- Simple UI with health and inventory displays  
- Expand doors with distinct keys, consumable keys, or NPC door-opening behaviours
- Extend the current count-limited inventory with menus, dropping, and possibly weight
- Something needs to be found & retrieved on the level, make your way back to the starting point  
   
