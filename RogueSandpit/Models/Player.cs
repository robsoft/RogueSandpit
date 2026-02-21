using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Models;

public class Player
{
    public Guid Id { get; private set; } = Guid.NewGuid();

}
