# Runner execution flow

This module follows a configuration from CLI bootstrap to disposed execution scopes. Allow **7 hours**, including the exercises. You should already have run a small QaaS scenario and written a small hook. The contributor's next job is to understand which component owns a behavior, when its inputs become available, and how failure changes the remaining work.

The source links pin the workspace revisions used for this module: Runner `8ccd6f53f9d23645ee117eef48ea3d56f24acfd0`, Framework `4d4b192391418c7882990323f2690d0649a20465`, and Mocker `c3aa787b2db6d4c796b0ec6625af16799150e5ec`. Read the published [QaaS documentation](https://docs.qaas.online/) for customer-facing configuration and the [SDK overview](https://docs.qaas.online/framework/projects/sdk/) for hook and data contracts. When contributing against another revision, check these methods again; the links describe specific implementations, not promises about every release.

## Study route

| Work | Time | Deliverable |
|---|---:|---|
| Bootstrap, configuration, and composition | 75 minutes | Annotated configuration-to-execution trace |
| Probes, mock control, and data ownership | 60 minutes | Hook ownership and data-flow sketch |
| Session and action scheduling | 90 minutes | Predicted and observed event timeline |
| Assertions, collectors, and reports | 60 minutes | Failure/outcome matrix |
| Cleanup and failure-path investigation | 75 minutes | Resource-ownership table and regression proposal |
| Explain the entire lifecycle to another contributor | 60 minutes | Short walkthrough with source evidence |

## 1. Initialization: three different lifetimes

A **Runner invocation** owns a list of execution builders and a runner-level scope. An **Execution** owns a chosen execution plan and an execution scope. A **Session** owns a group of staged actions and optional collectors. A session is neither an operating-system process nor the whole test run. Several sessions may overlap within one execution, while the top-level Runner starts its executions sequentially.

Start at [`Bootstrap.GetRunner`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Bootstrap.cs#L53). It parses arguments and chooses loaders for `run`, `act`, `assert`, `template`, or an execution sequence. Help, version, and parse-only outcomes can return a Runner with a bootstrap-handled exit code. Receiving a Runner object does not establish that any execution was built.

[`RunAndGetExitCode`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Runner.cs#L96) is the useful entry point for a host that needs the result without asking Runner to apply process-exit policy. `Run()` calls it and then handles process completion. An exception can still escape `RunAndGetExitCode`; its name does not mean every failure becomes an integer.

```mermaid
flowchart TD
    CLI[CLI arguments] --> B[Bootstrap selects loader]
    B --> H[Help/version/parse-only result]
    B --> C[Load contexts and execution builders]
    C --> R[Runner.RunAndGetExitCode]
    R --> S[Runner Setup]
    S --> BUILD[Build all executions]
    BUILD --> RP[Validate ReportPortal access]
    RP --> E[Start executions sequentially]
    E --> CLEAN[Ordered cleanup pipeline]
    S -. captured failure .-> CLEAN
    BUILD -. captured failure .-> CLEAN
    RP -. captured failure .-> CLEAN
    E -. captured failure .-> CLEAN
    CLEAN --> RESULT[Return exit code or throw failure]
```

The phase order is explicit in [`ExecuteLifecycle`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Runner.cs#L400). `Setup` optionally cleans the results directory. In this revision, ReportPortal access validation happens **after all executions have been built and before any execution starts**. Terminal-output capture, when enabled by reporter configuration, starts earlier in `RunAndGetExitCode`.

`Setup` and `Teardown` are protected virtual Runner extension points. They are distinct from YAML-configured SDK probes. Changing one changes runner-level orchestration; adding a probe adds a domain action to a session.

## 2. Configuration loading: build a runtime model before running it

[`RunLoader.BuildContext`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Loaders/RunLoader.cs#L51) supplies the logger, execution identity, configuration file, running-session registry, overwrite inputs, case, reference options, and environment-resolution setting to `ContextBuilder`. The resulting `InternalContext` carries runtime-owned state as well as the effective configuration.

Follow [`ContextBuilder.GetConfiguration`](https://github.com/TheSmokeTeam/QaaS.Framework/blob/4d4b192391418c7882990323f2690d0649a20465/QaaS.Framework.SDK/ContextObjects/ContextBuilder.cs#L249) rather than treating YAML as a single deserialization call:

1. Add the base YAML, configured overwrite files, and files from overwrite folders.
2. In the normal branch, add the case before command-line overrides, reference resolution, and the enriched build.
3. With `ResolveCaseLast`, build and resolve the earlier configuration first, then add the case before the enriched build.
4. Construct the internal context with configuration, identity, logger, and running-session state.

The case-last option changes precedence and resolution timing. A change to a merge helper can therefore alter runtime behavior even when every individual YAML document remains valid. Do not describe environment resolution as an unconditional last overwrite layer: it participates in the configured resolution operations.

Next read [`ExecutionBuilder.Build`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/ExecutionBuilder.cs#L1106). It initializes context, establishes scope dependencies, filters and validates configured sections, and materializes runtime components. Its construction order is data sources, sessions, storages, assertions, then reporters. It resolves the logic objects and transfers the scope into the returned `Execution`.

Construction order and execution order are separate contracts. Building a storage object does not mean every CLI verb uses storage. Constructing a data source does not necessarily enumerate its generator. `DataSourceLogic` registers sources in execution data; [`DataSource.GetData`](https://github.com/TheSmokeTeam/QaaS.Framework/blob/4d4b192391418c7882990323f2690d0649a20465/QaaS.Framework.SDK/DataSourceObjects/DataSource.cs#L102) determines generation, lazy enumeration, and cached materialization behavior when data is requested.

The builder disposes its scope when construction inside its protected `try` fails. Context initialization and scope creation precede that `try`; do not expand this into a guarantee covering every possible allocation failure.

**Exercise — configuration trace.** Pick one existing working scenario. Find where one overridden value enters `RootConfiguration`, where its builder property is populated, and where the runtime object consumes it. Repeat with a case override and `ResolveCasesLast`. Produce a four-column table: input, resolved value, owning builder, runtime reader. Explain which validation belongs to generic configuration processing, the execution builder, and the hook itself. Do not change shared configuration-resolution code merely to make one scenario pass.

## 3. Probe and Mocker integration: discovery, composition, then actions

Hooks enter through framework providers. [`HookProvider`](https://github.com/TheSmokeTeam/QaaS.Framework/blob/4d4b192391418c7882990323f2690d0649a20465/QaaS.Framework.Providers/Providers/HookProvider.cs#L24) discovers supported hook types from assemblies, resolves names, and creates instances. [`HooksFromProvidersLoader.LoadAndValidate`](https://github.com/TheSmokeTeam/QaaS.Framework/blob/4d4b192391418c7882990323f2690d0649a20465/QaaS.Framework.Providers/HooksFromProvidersLoader.cs#L20) then calls each selected hook's configuration loader/validator and collects validation results. A configuration instance name and a CLR hook type selector have different jobs; follow both through `HookData` when diagnosing an unresolved hook.

The execution builder composes these instances into runtime objects. For a probe, the runtime action invokes the SDK hook's `Run` method with snapshots of completed session data and selected data sources. [`Probe.Act`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner.Sessions/Actions/Probes/Probe.cs#L37) enters a probe execution scope and returns empty communication data. This is why a probe can perform domain setup or inspection without behaving like a transaction that records a request/response pair.

A probe runs at its configured action stage. It is not an automatic global “before test” callback, and placing it in an earlier action stage does not by itself prove it finishes before later-stage actions start. The scheduling details in the next section matter for setup probes.

Mocker control is a different boundary. [`MockerCommand.Command`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner.Sessions/Actions/MockerCommands/MockerCommand.cs#L105) discovers running Mocker instances through Redis and sends control requests. No matching instance and unsuccessful command responses have explicit failure paths. This code is not an implicit Mocker-process launcher. Arrange the required Mocker service and its control infrastructure before using the action; inspect its configured target selection and response handling when a command appears ineffective.

```mermaid
flowchart LR
    CFG[Hook type and hook configuration] --> PROVIDER[Provider resolves instance]
    PROVIDER --> VALIDATE[Hook configuration validation]
    VALIDATE --> ACTION[Probe action in session]
    ACTION --> SDK[SDK IProbe.Run]
    CMD[MockerCommand action] --> REDIS[Redis control messages]
    REDIS --> MOCK[Running Mocker controller]
    CLIENT[Protocol action] --> SERVER[Mock server endpoint]
    SERVER --> RESPONSE[Stub and processor response]
```

The Mocker side owns its own server task and optional controller task; see [`Mocker.Execution.Run`](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/Execution.cs#L71). Its execution `Dispose` is empty at this revision, another reason to avoid inferring remote shutdown from a disposal call.

The lower paths describe control traffic and application traffic separately. A healthy mock endpoint does not prove Redis control works. A successful control request does not prove the application sends requests to the intended mock endpoint.

**Exercise — hook ownership.** Trace one small custom probe from its YAML type selector to `ProbeHook.Run`. Record which object owns its configuration, which context API it receives, and what data is available at invocation. Then trace a Mocker command without assuming the Runner owns the remote server lifetime. Explain where you would add a new domain hook versus where a transport change belongs.

## 4. Execution loop: distinguish three scheduling levels

[`Execution.ExecutionPlans`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Execution.cs#L33) is the authoritative pipeline table:

| Execution type | Logic order | Normal result calculation |
|---|---|---|
| Run | Data sources → sessions → assertions → reports | All assertion results must be Passed |
| Act | Data sources → sessions → storage | Zero after successful pipeline completion |
| Assert | Data sources → storage → assertions → reports | All assertion results must be Passed |
| Template | Template logic | Zero after successful pipeline completion |

In `Act`, storage persists session data; in `Assert`, storage retrieves it. `Run` does not include `StorageLogic`. Keep that distinction when investigating a missing persisted file. A zero `Act` code does not mean assertions passed: assertions were not executed by that plan.

At the outer level, [`Runner.StartExecutions`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Runner.cs#L227) enumerates `execution.Start()` sequentially and sums the integer results. A normal failure result allows enumeration to continue; a thrown exception interrupts it. Builders share one per-run mutable global dictionary, explicitly assigned in `BuildExecutions`; it is not an independent dictionary per execution.

At the session level, [`SessionLogic.Run`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Logics/SessionLogic.cs#L79) orders numeric session-stage buckets. Before launching a bucket, it materializes deferred sessions whose target stage has arrived. It then launches that bucket's sessions and materializes its immediate results. Remaining deferred sessions are drained at the end. `RunUntilStage` postpones waiting and publication of a result; it does not postpone the initial launch until that stage. Completed-session visibility therefore differs from live running-session visibility.

Inside a session, [`Session.RunAsync`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner.Sessions/Session/Session.cs#L80) initializes running-session state and prepares actions, launches action stages in numeric order, then awaits the combined action-task set. The easily missed detail is [`Stage.RunAsync`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner.Sessions/Session/Stage.cs#L84): it returns launched tasks after its configured delays. Its `WhenAll` is used for a completion-log continuation; it is not awaited as a barrier before returning.

```text
One session, with two action stages:

stage 0 launch:  action A [-------------------------]
                action B [--------]
stage 1 launch:       action C [-----------]
session wait:        await all A, B, C --------------|
collectors:                                           [------]
SessionData:                                                  publish

Horizontal lengths are illustrative, not timing guarantees.
Stage numbers order launch; this implementation permits overlap.
```

[`CreateTaskFromAction`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner.Sessions/Extensions/SessionExtensions.cs#L210) wraps `action.Act()` in `Task.Run`. Its catch records action failure and cancels corresponding live communication state rather than simply propagating every action exception as a faulted task. Consequently, “the aggregate await completed” is not equivalent to “every action succeeded.” Consult both recorded failures and assertion results.

**Exercise — scheduling experiment.** Instrument two harmless actions with start/end timestamps and use different action stages. Predict whether they overlap before running them. Then compare two sessions with immediate versus deferred result publication. Draw separate timelines for task lifetime, running registry visibility, and completed `SessionData` visibility. Propose a meaningful regression test for an ordering requirement; avoid using arbitrary sleeps as proof of a synchronization contract.

## 5. Assertion evaluation: outcomes are computed from selected evidence

[`AssertionLogic.Run`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Logics/AssertionLogic.cs#L25) snapshots the session/source collections and evaluates assertions through `Parallel.ForEach`. Results are collected concurrently. Do not rely on configuration order as completion order. Immutable collection membership also does not make every payload and referenced object deeply immutable.

[`Assertion.Execute`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner.Assertions/AssertionObjects/Assertion.cs#L62) removes absent session results and applies configured selection. The hook evaluates the selected sessions and data sources. An explicit hook `AssertionStatus` takes precedence over the returned Boolean; otherwise true maps to Passed and false to Failed. An exception inside the hook invocation becomes Broken. Selection and preparation occur before that protected invocation, so this is not a catch-all guarantee for every error in assertion processing.

The result also records flakiness from selected sessions' action failures. That metadata is independent of the hook's Boolean result. A selected action failure can coexist with a Passed assertion if the hook's rule still passes. Similarly, `TestDurationMs` adds assertion elapsed time to selected session durations; overlapping sessions mean it is not necessarily the suite's wall-clock duration.

[`ResolveAssertionExitCode`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Execution.cs#L119) asks whether every recorded result is Passed. An empty result collection satisfies `All`, so configuration that unintentionally selects no assertions deserves explicit coverage. Do not infer useful test coverage from exit zero alone.

**Exercise — outcome matrix.** Explain these separately: Boolean false; hook exception; explicit status with Boolean true; action failure with an assertion that still passes; no assertions. For each, identify the recorded status, metadata, and execution result from source. Extend a focused test around one meaningful boundary if making a contribution.

## 6. Metric and log harvesting: several mechanisms at different times

There is no universal post-assertion “collect all metrics” phase in `ExecutionPlans`. Collection and reporting have different owners:

| Evidence | Owner and timing |
|---|---|
| Action input/output and failures | Produced during session work; materialized into session data |
| Live communication | Running-session registry, available while actions run |
| Collector output | Collected after ordinary session actions complete |
| Assertion result and links | Constructed during assertion evaluation |
| Reporter entries | Written by ReportLogic after assertions |
| Terminal capture / queued ReportPortal publication | Runner-level capture and cleanup publication |

[`RunPostSessionTasksAsync`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner.Sessions/Session/Session.cs#L190) passes the session's measured start/end timestamps to collectors, launches their tasks, and awaits completion. The measured end is captured after normal action completion and before collector work. This gives collectors the action window to query rather than automatically including their own collection latency. The actual metrics/log backend and filtering depend on the configured collector protocol.

`CreateSessionData` combines action results into public session evidence. Public communication objects represent input/output directions; internal communication results support runtime materialization. `SaveData=false` returns no session result even though the session work ran. That directly affects what later assertions can select.

[`ReportLogic.Run`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Logics/ReportLogic.cs#L29) filters by configured reporter type and assertion status before writing results. A missing report entry can therefore be a reporting-selection issue rather than a missing assertion execution. ReportPortal's queued results are published later, before execution scope disposal. Dashboard links are references to evidence; they are not proof that a collector fetched that evidence into `SessionData`.

```mermaid
flowchart LR
    GEN[Data source / generator] --> ACT[Action inputs]
    ACT --> LIVE[Running communication registry]
    ACT --> RESULT[Internal action result / failure]
    WINDOW[Measured action time window] --> COL[Collectors]
    RESULT --> SESSION[Completed SessionData]
    COL --> SESSION
    SESSION --> ASSERT[Selected assertion inputs]
    ASSERT --> AR[AssertionResult]
    AR --> REPORT[Configured reporters]
    SESSION --> STORE[Storage in Act plan]
    STORE --> RESTORE[Restored sessions in Assert plan]
    RESTORE --> ASSERT
```

## 7. Cleanup and teardown: inspect the precise failure boundary

Session cleanup and Runner cleanup are separate. `Session.RunAsync` uses `finally` to dispose its task collection and configured actions/collectors and remove its running-session entry. Its optional after-session delay and returned result follow the `finally`; they are not executed after an exception that propagates out of the protected session work. The disposal helper attempts individual items and logs failures, so its behavior differs from the Runner's execution-disposal loop.

[`RunCleanupSteps`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Runner.cs#L482) establishes this order:

1. Stop terminal capture and publish queued ReportPortal results; dispose capture in the publication helper's `finally`.
2. Dispose the built executions, releasing their owned scopes.
3. Call Runner `Teardown`, which can dispose the Serilog logger and serve results when configured.
4. Dispose Runner-level resources.

Each cleanup **step** is caught separately and remaining steps are attempted. However, [`DisposeExecutions`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Runner.cs#L266) loops without a separate catch around each execution. One throwing disposal can skip later executions in that loop while teardown and runner disposal still proceed. “Cleanup continues” must name the level at which continuation is guaranteed.

[`CompleteRun`](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/Runner.cs#L463) combines lifecycle outcomes with cleanup failures. `InvalidConfigurationsException` has a dedicated failure-code path. Other captured lifecycle exceptions are rethrown when cleanup succeeds; combined failures can become `AggregateException`. Cleanup failures can therefore replace a normal integer-return path with an exception.

Two ownership caveats deserve attention when reviewing changes:

- The top-level capture starts after bootstrap/loading and initial Runner logging/capture setup. The explicit cleanup pipeline is not a universal `finally` around everything that occurred since process launch.
- `BuildExecutions` materializes a LINQ projection into a list before assigning that list into lifecycle state. If a later builder fails, earlier successful executions are not automatically preserved in that outcome list. The failing builder's local cleanup does not establish that every earlier built scope was released. Verify ownership explicitly before claiming complete partial-build cleanup.

These are code-reading boundaries, not claims that a failure has been reproduced in a deployment. They identify where a contributor should add evidence before changing resource handling.

**Exercise — failure-path review.** Create an ownership table for Runner scope, execution scope, action, terminal capture, and remote Mocker service. For each, record creation, normal release, failure release, and whether the owner can outlive the current method. Select one failure injection—second builder fails, one execution disposal throws, or a publication step throws—and specify the expected remaining operations. Write the regression-test design before the fix. Do not assume disposing a local Mocker command stops the remote service.

## Contributor completion check

Explain one scenario from configuration to report in ten minutes with the source open. Your explanation should distinguish build order from execution order, session stages from action stages, live data from completed evidence, assertion status from action failure, and session cleanup from Runner cleanup. Then propose one small contribution with an explicit owner, input/output contract, failure behavior, and regression case. If any claim relies only on a class name or an architecture diagram, follow its actual call site before including it in a PR.
