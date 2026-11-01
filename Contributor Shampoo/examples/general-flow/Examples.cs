using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Generator;
using QaaS.Framework.SDK.Hooks.Probe;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace ContributorExamples;

public sealed class OrderBatchConfig
{
    [Range(1, 1000)] public int Count { get; set; } = 3;
    [Required] public string Prefix { get; set; } = "order";
}

public sealed record OrderRequest(string Id, int Quantity);

public sealed class OrderBatchGenerator : BaseGenerator<OrderBatchConfig>
{
    public override IEnumerable<Data<object>> Generate(
        IImmutableList<SessionData> sessions, IImmutableList<DataSource> sources)
    {
        var count = Configuration.Count;
        var prefix = Configuration.Prefix;
        for (var i = 0; i < count; i++)
            yield return new Data<object> { Body = new OrderRequest($"{prefix}-{i:D4}", i + 1) };
    }
}

public sealed class SnapshotProbeConfig
{
    [Required] public string Label { get; set; } = "checkpoint";
}

public sealed class SnapshotProbe : BaseProbe<SnapshotProbeConfig>
{
    public override void Run(IImmutableList<SessionData> sessions,
        IImmutableList<DataSource> sources)
    {
        Context.Logger.LogInformation(
            "{Label}: {CompletedSessions} completed sessions; {Sources} selected sources",
            Configuration.Label, sessions.Count, sources.Count);
    }
}

public sealed class CompletedSessionConfig
{
    [Range(1, 1000)] public int ExpectedCount { get; set; } = 1;
}

public sealed class CompletedSessionAssertion : BaseAssertion<CompletedSessionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessions,
        IImmutableList<DataSource> sources)
    {
        var failureCount = sessions.Sum(session => session.SessionFailures.Count);
        var passed = sessions.Count == Configuration.ExpectedCount && failureCount == 0;
        AssertionMessage = $"Expected {Configuration.ExpectedCount} completed sessions; " +
            $"received {sessions.Count}, with {failureCount} action failures.";
        return passed;
    }
}

public static class EvidenceSummary
{
    public static string[] Summarize(IImmutableList<SessionData> sessions) =>
        sessions.Select(session =>
            $"{session.Name}: inputs={session.Inputs?.Sum(x => x.Data.Count) ?? 0}, " +
            $"outputs={session.Outputs?.Sum(x => x.Data.Count) ?? 0}, " +
            $"failures={session.SessionFailures.Count}").ToArray();
}

public static class CleanupExample
{
    public static void DisposeAll(IEnumerable<IDisposable> resources)
    {
        var failures = new List<Exception>();
        foreach (var resource in resources)
        {
            try { resource.Dispose(); }
            catch (Exception error) { failures.Add(error); }
        }
        if (failures.Count != 0)
            throw new AggregateException("Execution disposal failed", failures);
    }
}
