using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Obstacle
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int X1 { get; set; }
    public int X2 { get; set; }
    public int Y1 { get; set; }
    public int Y2 { get; set; }
    public Microsoft.Xna.Framework.Color Color { get; set; } = Microsoft.Xna.Framework.Color.Yellow;

    public Visibility Visibility { get; set; } = Visibility.Visible;
    public bool BeenRemoved { get; set; } = false;

    public Obstacle(int X1, int Y1, int X2, int Y2, Microsoft.Xna.Framework.Color Colour)
    {
        this.X1 = X1;
        this.X2 = X2;
        this.Y1 = Y1;
        this.Y2 = Y2;
        this.Color = Colour;
    }
}