using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class BaseContainingElement
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int X1 { get; set; }
    public int X2 { get; set; }
    public int Y1 { get; set; }
    public int Y2 { get; set; }

    public bool HasVisited { get; set; } = false;

    public BaseContainingElement(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

}