# Rogue Sandpit 

A simple rogue-like game built with Monogame and .net 9. The game features a player character that can move around a grid-based map, collect items, and fight NPCs. The game is designed to be simple and easy to understand, with a focus on the core mechanics of a rogue-like game.

## Monogame, .Net9
builds & runs on Mac and Windows

`dotnet run` opens a 2× (1600×1200) window while preserving the native 800×600 pixel canvas. Use `dotnet run -- --scale 1` for the original window size, or `--scale 3` / `--scale 4` for larger integer scaling.


## Current Status
- Player character can move around the map  
- Map is generated with rooms and corridors  
- NPCs do damage to the player if they're adjacent  
- Player can see entire map & NPCs in debug mode  
- Normal play uses persistent fog-of-war; F1 remains an omniscient developer view
- Fully turn-based, so NPCs 'pause' until the player makes a move  
- Player can register a 'move' even if they don't actually move (eg, against a wall), so NPCs will then move  
- Player attacks NPCs by moving into them; defeated NPCs stop acting and blocking cells
- Retrieve the yellow special tile and return it to the entrance to win
- HUD displays health, damage, and objective status
- NPCs pursue the player with line of sight and A* pathfinding, then attack from cardinally adjacent cells
- NPCs remember and investigate the last place they saw the player
- Orcs, Goblins, Skeletons, Trolls, and Wretches have seeded names and distinct combat profiles
- Pursuing NPCs render orange-red and investigating NPCs render yellow during development
- F1 debug mode supports hover inspection plus NPC path and line-of-sight visualization
- A compact event log shows recent combat and objective events
- Reachable potions, weapons, armor, and keys can be collected in an eight-slot inventory
- Brackets select inventory items; H uses a selected potion, E equips selected weapons or armor, and defeated NPCs may drop loot
- Equipped armor reduces incoming damage and is shown with defence in the HUD
- I opens an eight-slot inventory panel with selection, item details, and equipment state
- Closed doors take a turn to open; locked doors require a carried, reusable key
- Chasing and investigating NPCs can spend a turn opening closed doors, but not locked ones
 

## Pressing TODOs
- Extend local NPC searching with prediction, hearing, or shared awareness
- Graphics!  


## Controls
- Arrow keys (WASD will come) to move the player character
- H to use a healing potion
- E to equip the selected weapon or armor
- D to drop the selected item
- Left/right bracket to select an inventory item
- I to open or close the inventory panel; arrows select items while it is open
- Period or numpad 5 to wait one turn
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
- Expand doors with distinct keys or consumable keys
- Extend the current inventory with stacking and possibly weight
- Something needs to be found & retrieved on the level, make your way back to the starting point  
   
