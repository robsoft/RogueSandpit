using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public enum CharacterTypes { Orc, Goblin, Skeleton, Troll, Wretch };


public enum Direction { Up, Down, Left, Right };
public enum NPCState { Active, InActive, Targeting, Homing, Dead };
public enum Visibility { Hidden, Visible, Cloaked };
public enum CharacterMood { Attacker, Defender, Neutral, Helpful };
public enum RenderMode { Rooms, Cells };
public enum MapCellType { Wall, Floor, Door, Special };
public enum GameOutcome { Playing, Won, Lost };
public enum PlayerCommand { None, MoveUp, MoveDown, MoveLeft, MoveRight, Wait, SelectPreviousItem, SelectNextItem, UsePotion, EquipItem, DropItem };
public enum NPCAwareness { Unaware, Pursuing, Investigating };
public enum NPCInvestigationSource { None, Noise, AllyAlert, LastSeen };
public enum ItemType { HealingPotion, Weapon, Key, Armor };
public enum DoorState { Closed, Locked, Open };
public enum PlayerItemActionResult { Success, NoSelection, WrongItemType, NoEffect, Blocked };

