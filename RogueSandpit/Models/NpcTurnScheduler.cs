using System.Collections.Generic;
using System.Linq;

namespace RogueSandpit.Models;

public sealed class NpcTurnScheduler
{
    private int _initiativeOffset;

    public IReadOnlyList<BaseNPC> CreateTurnOrder(IEnumerable<BaseNPC> npcs)
    {
        List<BaseNPC> eligible = npcs
            .Where(npc => npc.State == NPCState.Active)
            .ToList();
        if (eligible.Count == 0) return eligible;

        int first = _initiativeOffset % eligible.Count;
        var ordered = new List<BaseNPC>(eligible.Count);
        for (int index = 0; index < eligible.Count; index++)
        {
            ordered.Add(eligible[(first + index) % eligible.Count]);
        }

        _initiativeOffset = (_initiativeOffset + 1) % eligible.Count;
        return ordered;
    }

    public int InitiativeOffset => _initiativeOffset;
}
