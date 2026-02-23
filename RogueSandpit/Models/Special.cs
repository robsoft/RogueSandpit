using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;
public class Special
{
     public Guid Id { get; private set; } = Guid.NewGuid();

    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public BaseContainingElement CurrentRoom { get; set; } = null;

    public Special(int x, int y, BaseContainingElement currentRoom)
    {
        this.X = x;
        this.Y = y;
        this.CurrentRoom = currentRoom;
    }

}