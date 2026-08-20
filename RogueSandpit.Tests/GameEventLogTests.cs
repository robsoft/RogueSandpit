using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class GameEventLogTests
{
    [Fact]
    public void RetainsOnlyNewestEntriesUpToCapacity()
    {
        var log = new GameEventLog(3);

        log.Add("ONE");
        log.Add("TWO");
        log.Add("THREE");
        log.Add("FOUR");

        Assert.Equal(["TWO", "THREE", "FOUR"], log.Entries);
    }

    [Fact]
    public void IgnoresBlankMessages()
    {
        var log = new GameEventLog();

        log.Add("");
        log.Add("   ");

        Assert.Empty(log.Entries);
    }
}
