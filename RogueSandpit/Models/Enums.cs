using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public enum Direction { Up, Down, Left, Right, None };
public enum NPCState { Active, InActive, Targetting, Homing };
public enum Visibility { Hidden, Visible, Cloaked };
public enum Character { Attacker, Defender, Neutral, Helpful };
