using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Obstacle : BaseMapElement
{

    public Microsoft.Xna.Framework.Color Color { get; set; } = Microsoft.Xna.Framework.Color.Yellow;

    public Visibility Visibility { get; set; } = Visibility.Visible;
    public bool BeenRemoved { get; set; } = false;

    public Obstacle(int X1, int Y1, int X2, int Y2, Microsoft.Xna.Framework.Color Colour)
        : base(X1, Y1, X2, Y2)
    {
        this.Color = Colour;
    }
}