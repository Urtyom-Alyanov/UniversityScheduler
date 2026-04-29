using Xunit;
using System.Collections.Generic;
using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class TopologicalSortTests
{
    [Fact]
    public void TopologicalSort_TypicalData_ReturnsCorrectOrder()
    {
        var s1 = new Session { Id = 1, Subject = "S1" };
        var s2 = new Session { Id = 2, Subject = "S2", PrerequisiteSessionId = 1 };
        var s3 = new Session { Id = 3, Subject = "S3", PrerequisiteSessionId = 2 };
        
        var sessions = new List<Session> { s3, s2, s1 };
        var engine = new SchedulerEngine(sessions, new List<Room>());

        var (isDag, sorted) = engine.TopologicalSort();

        Assert.True(isDag);
        Assert.Equal(1, sorted[0].Id);
        Assert.Equal(2, sorted[1].Id);
        Assert.Equal(3, sorted[2].Id);
    }

    [Fact]
    public void TopologicalSort_EmptyList_ReturnsEmpty()
    {
        var engine = new SchedulerEngine(new List<Session>(), new List<Room>());
        var (isDag, sorted) = engine.TopologicalSort();
        Assert.True(isDag);
        Assert.Empty(sorted);
    }

    [Fact]
    public void TopologicalSort_Cycle_ReturnsIsDagFalse()
    {
        var s1 = new Session { Id = 1, Subject = "S1", PrerequisiteSessionId = 2 };
        var s2 = new Session { Id = 2, Subject = "S2", PrerequisiteSessionId = 1 };
        
        var sessions = new List<Session> { s1, s2 };
        var engine = new SchedulerEngine(sessions, new List<Room>());

        var (isDag, _) = engine.TopologicalSort();

        Assert.False(isDag);
    }

    [Fact]
    public void TopologicalSort_NullPrerequisites_HandlesCorrectly()
    {
        var s1 = new Session { Id = 1, Subject = "S1", PrerequisiteSessionId = null };
        var sessions = new List<Session> { s1 };
        var engine = new SchedulerEngine(sessions, new List<Room>());

        var (isDag, sorted) = engine.TopologicalSort();

        Assert.True(isDag);
        Assert.Single(sorted);
    }

    [Fact]
    public void TopologicalSort_DisconnectedGraph_ReturnsAllSessions()
    {
        var s1 = new Session { Id = 1, Subject = "S1" };
        var s2 = new Session { Id = 2, Subject = "S2" };
        var sessions = new List<Session> { s1, s2 };
        var engine = new SchedulerEngine(sessions, new List<Room>());

        var (isDag, sorted) = engine.TopologicalSort();

        Assert.True(isDag);
        Assert.Equal(2, sorted.Count);
    }

    [Fact]
    public void TopologicalSort_LargeDataset_CompletesInTime()
    {
        var sessions = new List<Session>();
        for (int i = 0; i < 200; i++)
        {
            sessions.Add(new Session
            {
                Id = i + 1,
                Subject = $"S{i + 1}",
                PrerequisiteSessionId = i > 0 ? i : (int?)null
            });
        }

        var engine = new SchedulerEngine(sessions, new List<Room>());
        var startTime = DateTime.Now;

        var (isDag, sorted) = engine.TopologicalSort();

        var elapsed = DateTime.Now - startTime;
        Assert.True(elapsed.TotalSeconds < 5, $"Took {elapsed.TotalSeconds}s");
        Assert.True(isDag);
        Assert.Equal(200, sorted.Count);
    }

    [Fact]
    public void TopologicalSort_SelfReference_DetectsCycle()
    {
        var s1 = new Session { Id = 1, Subject = "S1", PrerequisiteSessionId = 1 };
        
        var sessions = new List<Session> { s1 };
        var engine = new SchedulerEngine(sessions, new List<Room>());

        var (isDag, _) = engine.TopologicalSort();

        Assert.False(isDag);
    }
}
