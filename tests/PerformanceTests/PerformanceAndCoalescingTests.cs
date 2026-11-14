using System.Diagnostics;
using Dockyra.Application.Services;
using Dockyra.Domain.Rules;
using Dockyra.Domain.ValueObjects;
using Xunit;

namespace Dockyra.PerformanceTests;

public sealed class PerformanceAndCoalescingTests
{
    [Fact]
    public async Task EventCoalescer_CoalescesFiftyRapidSignalsIntoSingleExecution()
    {
        using var invokedEvent = new ManualResetEventSlim(false);
        using var coalescer = new EventCoalescer(
            () => invokedEvent.Set(),
            TimeSpan.FromMilliseconds(50));

        for (int i = 0; i < 50; i++)
        {
            coalescer.Signal();
        }

        bool signaled = invokedEvent.Wait(TimeSpan.FromSeconds(2));
        await Task.Delay(60);

        Assert.True(signaled);
        Assert.Equal(1, coalescer.CoalescedOutputCount);
    }

    [Fact]
    public void SmartGroupingRuleEngine_EvaluatesTenThousandClassificationsWithinBudget()
    {
        var engine = new SmartGroupingRuleEngine();
        var sample = new AppIdentity("Code", @"C:\Program Files\Microsoft VS Code\Code.exe", "Dockyra");

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10_000; i++)
        {
            string? groupId = engine.SuggestGroupId(sample);
            Assert.Equal("grp-dev", groupId);
        }
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 500, $"Rule evaluation took {sw.ElapsedMilliseconds}ms, exceeding 500ms budget.");
    }
}

