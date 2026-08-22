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
public enum NPCMoraleState { Steady, Shaken, Fleeing, Enraged, Fearless };
public enum StatusEffectType { Stunned, Bleeding };
public enum RenderMode { Rooms, Cells };
public enum GenerationDepthBand { Shallow, Middle, Deep };
public enum MapCellType { Wall, Floor, Door, Special };
public enum GameOutcome { Playing, Won, Lost };
public enum PlayerCommand { None, MoveUp, MoveDown, MoveLeft, MoveRight, Wait, SelectPreviousItem, SelectNextItem, UsePotion, UseBandage, EquipItem, DropItem, ToggleDoorUp, ToggleDoorDown, ToggleDoorLeft, ToggleDoorRight, LayFalseTrailUp, LayFalseTrailDown, LayFalseTrailLeft, LayFalseTrailRight, ThrowItemUp, ThrowItemDown, ThrowItemLeft, ThrowItemRight, PlaceTrapUp, PlaceTrapDown, PlaceTrapLeft, PlaceTrapRight, FireRangedUp, FireRangedDown, FireRangedLeft, FireRangedRight };
public enum NPCAwareness { Unaware, Pursuing, Investigating };
public enum NPCInvestigationSource { None, Noise, AllyAlert, Casualty, Trail, LastSeen };
public enum ItemType { HealingPotion, Weapon, Key, Armor, Trap, RangedWeapon, Bandage, SmokeBomb, FireBomb };
public enum TrapKind { Hunting, Snare, Alarm };
public enum DoorState { Closed, Locked, Open };
public enum PlayerItemActionResult { Success, NoSelection, WrongItemType, NoEffect, Blocked };

