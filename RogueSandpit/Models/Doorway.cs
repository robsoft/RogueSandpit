using System;
namespace RogueSandpit.Models;

public class Doorway
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int X1 { get; set; }
    public int Y1 { get; set; }

    public DoorState State { get; set; }
    public bool CanTraverse => State == DoorState.Open;

    public Doorway(int x1, int y1, DoorState state = DoorState.Closed)
    {
        X1 = x1;
        Y1 = y1;
        State = state;
    }

}
