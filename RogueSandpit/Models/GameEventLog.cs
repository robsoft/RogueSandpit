using System.Collections.Generic;

namespace RogueSandpit.Models;

public class GameEventLog
{
    private readonly List<string> _entries = [];

    public int Capacity { get; }
    public IReadOnlyList<string> Entries => _entries;

    public GameEventLog(int capacity = 6)
    {
        Capacity = capacity;
    }

    public void Add(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || Capacity <= 0) return;

        _entries.Add(message);
        while (_entries.Count > Capacity)
        {
            _entries.RemoveAt(0);
        }
    }

    internal void Restore(IEnumerable<string> entries)
    {
        _entries.Clear();
        foreach (string entry in entries) Add(entry);
    }
}
