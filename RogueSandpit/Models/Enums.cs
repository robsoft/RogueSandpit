using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public enum CharacterTypes {Orc, Goblin, Skeleton};


public enum Direction { Up, Down, Left, Right, None };
public enum NPCState { Active, InActive, Targetting, Homing, Dead };
public enum Visibility { Hidden, Visible, Cloaked };
public enum CharacterMood { Attacker, Defender, Neutral, Helpful };
public enum RenderMode { Rooms, Cells };
public enum MapCellType { Wall, Floor, Door };

