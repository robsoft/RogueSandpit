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
 

## Pressing TODOs
- UI for HP, Damage etc  
- Names & class variation for NPCs  
- Implement the A* and Line-Of-Sight mechanics as per the RogueALike project  
- Player to attack NPCs
- Loot
- Graphics!  


## Controls
- Arrow keys (WASD will come) to move the player character
- F1 to toggle debug/map viewer  
- SPACE to generate a new map (effectively restart)
- ESCAPE to quit the game


## Intended Features
- Exits to be shown inside the room (it's not a bug, it's an undesired feature right now)  
- Player character with movement and combat mechanics  
- Items that can be collected and used  
- NPCs that can be fought and defeated  
- Simple UI with health and inventory displays  
- Some doors require keys to open  
- Other doors may require a 'move' to open, so you can't just walk straight through them  
- An inventory system with items that can be equipped and used, a limit on weight/count  
- Something needs to be found & retrieved on the level, make your way back to the starting point  
   
