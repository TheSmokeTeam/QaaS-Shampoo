# Understanding the general flow through one order scenario

This page explains why the order generator, snapshot probe, completion assertion, and cleanup example belong at different points in QaaS's lifecycle. Read it alongside the inline implementations in [Runner execution flow](../../01-general-flow/runner-execution-flow.md); use [solution and project structure](../../03-architecture/solution-and-project-structure.md) when deciding which component should own a change.

## Separate the invocation from its work

Runner, execution, and session are different ownership boundaries. Following those boundaries makes both normal operation and failure easier to explain.

A Runner invocation selects and builds executions, starts them sequentially, and owns overall cleanup. An execution owns a configured plan and its scope; a session groups actions and collectors inside that plan. Consequently, “time the test” is ambiguous. Timing an assertion measures evaluation of retained evidence. Timing a session measures another boundary. The host's stopwatch around bootstrap and `RunAndGetExitCode` measures the invocation, including initialization and cleanup.

The host example uses `finally` because elapsed time remains useful when bootstrap or execution throws. It preserves the distinction between an exit code and an exception instead of silently treating every abnormal outcome as an integer. `RunAndGetExitCode` also leaves process-exit ownership with the embedding host. Trace those decisions in [Runner](https://github.com/TheSmokeTeam/QaaS.Runner/blob/7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba/QaaS.Runner/Runner.cs) before adding service-level instrumentation.

## Follow configuration into a generator

Configuration chooses a source instance, selects its hook implementation, and supplies values to that hook. These are three separate naming and validation problems.

In the order scenario, `orders` identifies the data source; `ContributorExamples.OrderBatchGenerator` identifies the CLR type; `Count` and `Prefix` configure its behavior. Discovery cannot fix a misspelled configuration property, and a valid configuration cannot make an unavailable assembly discoverable. The providers create the hook and assign context; configuration loading and validation then establish its input contract before generation.

The annotated count bounds prevent an actually bound value outside 1–1000. They do not establish that every supplied key was recognized. The [custom binder](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.Configurations/ConfigurationUtils.cs) currently logs and skips unknown properties. A misspelled count can therefore leave the default of three, which still validates. Diagnose configuration failures by comparing supplied keys, effective values, and validation results, rather than assuming successful validation proves the user's intent was understood.

## Understand when generation happens

The generator describes a sequence; the data source decides how consumers retrieve it. Construction and enumeration are not interchangeable events.

`OrderBatchGenerator.Generate` yields one fresh envelope and record per item. Its counter belongs to each enumeration, so a second enumeration produces the same identifiers instead of continuing a global sequence. This is useful for repeatable regression input. It is unsuitable as a universal uniqueness guarantee: separate runs using the same prefix can collide in persistent application state.

Configuration values are copied into local variables when enumeration begins. This keeps one enumeration internally consistent after those reads, but does not authorize concurrent mutation of configuration. Finish configuration before using the hook. Follow [DataSource.Retrieve](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.SDK/DataSourceObjects/DataSource.cs) to distinguish lazy retrieval from cached materialization. A diagnostic helper that enumerates a source merely to count it can accidentally trigger work or consume a one-shot sequence.

## Inspect available evidence with a probe

The snapshot probe deliberately reports descriptors and completed-session snapshots without consuming generated payloads.

Its session count describes the completed data passed to that invocation, not every session currently running. Its source count describes selected sources, not the number of messages they will yield. These small distinctions make the log useful without changing the observed pipeline. Counting messages would require an explicit decision about retrieval, caching, and consumption.

A probe is a configured session action. Its name does not make it a global setup callback or a synchronization barrier. [Probe.Act](https://github.com/TheSmokeTeam/QaaS.Runner/blob/7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba/QaaS.Runner.Sessions/Actions/Probes/Probe.cs) invokes its hook and returns empty communication data; custom logging is therefore different from a transaction's retained request/response evidence.

## Distinguish launch order, completion, and success

Scheduling gives order to work, but different boundaries provide different guarantees.

Inside a session, an earlier action stage does not by itself guarantee that its actions finish before later-stage actions start. The session ultimately awaits its collected action tasks. For setup that must complete before dependent work, inspect the session-stage coordination described in the main guide: separate session stages, without deferred `RunUntilStage`, provide the relevant completion boundary.

Even that boundary proves completion, not success. An action can finish after recording a failure, and later work can still start. A useful investigation therefore records three events separately: action launched, action completed, and outcome recorded. A log line saying “setup finished” must not stand in for a verified setup result.

## Evaluate retained evidence explicitly

The completion assertion and evidence summary answer different questions about the same session data.

`CompletedSessionAssertion` requires the expected number of sessions and zero recorded session failures. Requiring evidence prevents an empty input from passing accidentally. It does not prove that an order has the right business value, was durably stored, or produced a correct downstream event. Those requirements need their own assertions against the appropriate outputs.

`EvidenceSummary` counts retained input/output items and failures without logging bodies. These totals describe saved evidence, not attempted operations or wire bytes. Null input/output collections contribute zero. Before explaining a mismatch as missing traffic, check which actions retained data and which selections reached the assertion. Runner maps the Boolean result to passed/failed unless explicitly overridden; thrown assertion errors are a separate broken outcome.

## Preserve cleanup ownership under failure

Cleanup must release owned resources while retaining enough failure information to diagnose incomplete disposal.

The proposed helper attempts every supplied resource, collects exceptions, and throws an aggregate afterward. This improves a loop that otherwise stops at its first disposal exception. It does not discover resources that were never added to that collection, repair partially constructed ownership, or shut down an independently running Mocker. Those are separate lifecycle obligations. Treat the helper as an explained improvement, not an already integrated Runner change; follow construction, ownership transfer, and disposal together before applying it.
