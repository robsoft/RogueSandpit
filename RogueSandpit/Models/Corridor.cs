using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Corridor : BaseMapElement
{
    public Microsoft.Xna.Framework.Color Color { get; set; } = Microsoft.Xna.Framework.Color.LightBlue;

    public bool BeenTraversed { get; set; } = false;
    public bool Locked { get; set; } = false;

    public Corridor(int X1, int Y1, int X2, int Y2) : base(X1, Y1, X2, Y2)
    {
    }
}
