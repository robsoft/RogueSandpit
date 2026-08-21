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
- NPCs hear nearby combat, opened doors, and dropped items, then investigate the sound
- NPCs alert nearby allies when they first spot the player, sharing the observed location once
- Investigation confidence decays each turn until an NPC abandons stale evidence
- NPCs predict the player's likely continuation from movement they personally observed
- Successful movement leaves a short-lived trail that nearby investigating NPCs can discover
- Alerted allies fan out across distinct nearby search cells instead of stacking on one target
- Archetypes differ in tracking skill; Skeletons cannot interpret physical trails
- Room, corridor, and doorway movement leave clues with different strength and lifetime
- Players can cautiously open or close nearby doors in place and lay directional false trails to mislead weaker trackers
- Wounded NPCs react by archetype: some flee, Skeletons remain fearless, and Trolls become enraged
- Fleeing NPCs retreat from remembered threats and call nearby allies for help once per retreat
- Selected inventory items can be thrown directionally as recoverable, noise-making distractions
- Hunting traps can be placed on adjacent cells and damage the first NPC to enter them
- Actors support timed bleeding and stunned effects with turn-based duration and damage
- Thrown weapons strike and bleed the first NPC in their path, while thrown potions shatter
- Hunting traps stun surviving victims for their next action
- Orcs, Goblins, Skeletons, Trolls, and Wretches have seeded names plus distinct combat and awareness profiles
- Pursuing NPCs render orange-red, investigating NPCs yellow, fleeing NPCs blue, and enraged NPCs red during development
- F1 debug mode supports hover inspection plus NPC path and line-of-sight visualization
- A compact event log shows recent combat and objective events
- Reachable potions, weapons, armor, and keys can be collected in an eight-slot inventory
- Brackets select inventory items; H uses a selected potion, E equips selected weapons or armor, and defeated NPCs may drop loot
- The first weapon collected is equipped automatically when the weapon slot is empty
- Equipped armor reduces incoming damage and is shown with defence in the HUD
- I opens an eight-slot inventory panel with selection, item details, and equipment state
- Closed doors take a turn to open; locked doors require a carried, reusable key
- Chasing and investigating NPCs can spend a turn opening closed doors, but not locked ones
 

## Pressing TODOs
- Remove NPC list-order effects with a fairer turn-resolution architecture
- Graphics!  


## Controls
- Arrow keys (WASD will come) to move the player character
- H to use a healing potion
- E to equip the selected weapon or armor
- D to drop the selected item
- C to open or close an adjacent unlocked door; arrows choose if more than one is available
- T followed by an arrow to lay a false trail in that direction
- F followed by an arrow to throw the selected inventory item
- P followed by an arrow to place a selected hunting trap
- Left/right bracket to select an inventory item
- I to open or close the inventory panel; arrows select items while it is open
- Period or numpad 5 to wait one turn
- F1 to toggle debug/map viewer  
- In debug mode, hover a cell to inspect it and visualize NPC decisions
- SPACE to generate a new map (effectively restart)
- ESCAPE to cancel a directional action, or quit the game otherwise


## Intended Features
- Exits to be shown inside the room (it's not a bug, it's an undesired feature right now)  
- Player character with movement and combat mechanics  
- Items that can be collected and used, with additional item types to come
- NPCs that can be fought and defeated  
- Simple UI with health and inventory displays  
- Expand doors with distinct keys or consumable keys
- Extend the current inventory with stacking and possibly weight
- Something needs to be found & retrieved on the level, make your way back to the starting point  
   
