using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;


public abstract class BaseNPC
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Description { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;

    public Direction Direction { get; set; } = Direction.Up;
    public int X { get; set; }
    public int Y { get; set; }
    public int Speed { get; set; } 
    public int HP { get; set; }
    public int Damage { get; set; }

    public int AssetID { get; set; }
    public int AnimFrame {  get; set; }
    
    public NPCState State { get; set; } = NPCState.InActive;
    public Visibility Visibility { get; set; } = Visibility.Hidden;

    // we set these to the postion of something we want to follow, when we start following it
    // at each turn, if we can still see the target, we update these.
    // if the target 'disappears' from view, we continue to travel to the location where we last saw it 
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public int TargetId { get; set; } // id of the player, item or other NPC we're following

    // this is where the NPC starts out from, and is where it will 'home' back to when it can't find a target   
    // (the target is dead or disappeared etc)
    public int HomeX { get; set; }
    public int HomeY { get; set; }

}
