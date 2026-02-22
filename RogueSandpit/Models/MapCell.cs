using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;


public class MapCell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public MapCellType CellType { get; private set; }
    public BaseMapElement ParentElement { get; private set; }

    public MapCell(int x, int y, MapCellType cellType, BaseMapElement parentElement = null)
    {
        this.X = x;
        this.Y = y;
        this.CellType = cellType;
        this.ParentElement = parentElement;
    }

}