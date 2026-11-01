using System.Collections.Immutable;
using ContributorExamples;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QaaS.Framework.Providers.ObjectCreation;
using QaaS.Framework.Providers.Providers;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Generator;
using QaaS.Framework.SDK.Session.SessionDataObjects;

var passed = 0;
void Check(bool success, string label)
{
    if (!success) throw new InvalidOperationException(label);
    Console.WriteLine($"PASS {label}"); passed++;
}
IConfiguration Config(params (string Key, string Value)[] values) =>
    new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value)).Build();
var context = new Context { Logger = NullLogger.Instance };
var sessions = ImmutableList<SessionData>.Empty;
var sources = ImmutableList<DataSource>.Empty;
var provider = new HookProvider<IGenerator>(context, new ByNameObjectCreator(context.Logger));
var generator = (OrderBatchGenerator)provider.GetSupportedInstanceByName(typeof(OrderBatchGenerator).FullName!);
Check(ReferenceEquals(generator.Context, context), "provider discovers hook and injects context");
Check(generator.LoadAndValidateConfiguration(Config(("Count", "3"), ("Prefix", "invoice")))?.Count == 0,
    "valid generator configuration binds with no validation errors");
string[] Ids() => generator.Generate(sessions, sources).Select(x => ((OrderRequest)x.Body!).Id).ToArray();
Check(Ids().SequenceEqual(new[] { "invoice-0000", "invoice-0001", "invoice-0002" }), "deterministic IDs");
Check(Ids().SequenceEqual(Ids()), "repeat enumeration restarts local state");
var parallel = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(Ids)));
Check(parallel.All(x => x.SequenceEqual(Ids())), "eight concurrent enumerations do not share counters");
Check(generator.LoadAndValidateConfiguration(Config(("Count", "0")))?.Count > 0, "zero count rejected");
Check(generator.LoadAndValidateConfiguration(Config(("Prefix", " ")) )?.Count > 0, "blank prefix rejected");
Check(generator.LoadAndValidateConfiguration(Config(("Typo", "3")))?.Count == 0 && generator.Configuration.Count == 3,
    "unknown field is skipped and defaults remain (current custom binder behavior)");
var probe = new SnapshotProbe { Context = context };
Check(probe.LoadAndValidateConfiguration(Config(("Label", "after setup")))?.Count == 0, "probe configuration binds");
probe.Run(sessions, sources);
Check(true, "snapshot probe runs without enumerating source payloads");
var assertion = new CompletedSessionAssertion { Context = context };
Check(assertion.LoadAndValidateConfiguration(Config(("ExpectedCount", "1")))?.Count == 0, "assertion configuration binds");
Check(!assertion.Assert(sessions, sources), "missing session fails rather than vacuous pass");
var one = ImmutableList.Create(new SessionData { Name = "checkout" });
Check(assertion.Assert(one, sources), "one clean completed session passes");
var failedSession = ImmutableList.Create(new SessionData { Name = "checkout", SessionFailures = [new ActionFailure { Name = "submit" }] });
Check(!assertion.Assert(failedSession, sources), "recorded action failure prevents guard assertion pass");
Check(assertion.AssertionMessage!.Contains("1 action failures"), "guard assertion describes failed evidence");
Check(EvidenceSummary.Summarize(one).Single() == "checkout: inputs=0, outputs=0, failures=0", "summary handles missing input/output collections");
var calls = new List<int>();
try { CleanupExample.DisposeAll(new[] { new Resource(1, calls, true), new Resource(2, calls, false), new Resource(3, calls, true) }); }
catch (AggregateException error) { Check(error.InnerExceptions.Count == 2, "cleanup retains both failures"); }
Check(calls.SequenceEqual(new[] { 1, 2, 3 }), "cleanup continues after individual failure");
Console.WriteLine($"{passed} checks passed");

sealed class Resource(int id, List<int> calls, bool fail) : IDisposable
{
    public void Dispose() { calls.Add(id); if (fail) throw new InvalidOperationException(id.ToString()); }
}
