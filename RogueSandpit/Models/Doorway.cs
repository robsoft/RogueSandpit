using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Doorway
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int X1 { get; set; }
    public int Y1 { get; set; }

    public bool CanTraverse { get; set; } = false;

    public Doorway(int x1, int y1)
    {
        X1 = x1;
        Y1 = y1;
    }

}