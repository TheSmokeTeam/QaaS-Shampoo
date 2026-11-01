# Conventions and guidelines

This guide explains how to make a QaaS contribution understandable, testable, and consistent with the repository that owns it.

[Pack index](../README.md) · [Project boundaries](../03-architecture/solution-and-project-structure.md)

Allow **6 hours** for setup, selected reading, and the exercises. This guide sets the contribution standard for this study pack. A repository's checked-in instructions and maintainer decisions take precedence. Branch prefixes, commit conventions, and the PR checklist below are the pack's required practice; they are not a claim that every QaaS repository already enforces those rules automatically.

For an extended explanation of hook contracts, configuration boundaries, and policy composition, follow the [hooks and policies implementation guide](../examples/hooks-and-policies/README.md). The code and tests below introduce those decisions where they matter in the contribution workflow.

## 1. Set up a reproducible working environment

A reproducible environment lets reviewers run the same build and diagnose the same failures as the contributor.

Install Git, a C# IDE with debugger support, and the .NET SDK required by the project you will change. Read `global.json` if present, the solution's project files, `Directory.Build.props`, `NuGet.config`, and its CI workflow before choosing SDK or tool versions. The inspected [Framework SDK project](https://github.com/TheSmokeTeam/QaaS.Framework/blob/4d4b192391418c7882990323f2690d0649a20465/QaaS.Framework.SDK/QaaS.Framework.SDK.csproj) and [Runner project](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/QaaS.Runner.csproj) target `net10.0`; do not install an old .NET Core SDK merely because a tutorial uses that name. Nullable reference checking is enabled in both. Check those files again on your contribution branch.

From the Framework repository root, establish a baseline before editing:

```sh
git status --short
dotnet --info
dotnet restore QaaS.Framework.sln
dotnet build QaaS.Framework.sln --no-restore --nologo
dotnet test QaaS.Framework.sln --no-build --nologo
```

Use the corresponding real solution name in another repository. Record SDK version, command, exit status, and any failing test names. An existing failure should be investigated and identified separately from a regression introduced by your change. Restore may require access to configured feeds; investigate the repository's `NuGet.config` before replacing sources. These are commands for the contributor to run, not a claim that this documentation pack executed every solution's tests.

Use local tool manifests where provided. The Framework contributor instructions call for `csharpier format <changed-files>`; install the maintainer-approved CSharpier version if your checkout has no tool manifest, and format only your changed files. Do not reformat an entire solution as a side effect of a small fix. See the [CSharpier installation guide](https://csharpier.com/docs/Installation) and [format command](https://csharpier.com/docs/CLI).

## 2. Required AI-assisted tooling

Use the QaaS developer tooling to locate ownership and check assumptions before changing a contract.

Install and use the official QaaS developer plugin from [TheSmokeTeam/qaas-dev-claude-plugins](https://github.com/TheSmokeTeam/qaas-dev-claude-plugins). Run these commands **inside Claude Code**, not in a shell:

```text
/plugin marketplace add TheSmokeTeam/qaas-dev-claude-plugins
/plugin install qaas-platform-dev@qaas-dev-claude-plugins
```

The commands and available skills are documented in the [plugin README](https://github.com/TheSmokeTeam/qaas-dev-claude-plugins/blob/cc498ff1751df7a341d1802cf2a340a0f3e2597e/README.md). Start a contribution with:

```text
/qaas-where Add a generator hook that supplies a deterministic sequence of records
```

Then ask the agent to read the target repository's instructions, identify the extension boundary, cite the source it will change, and propose a focused regression test. Use `qaas-platform-overview` for routing, `add-framework-hook` for reflection-discovered hooks, `extend-framework-core` for compiled protocols/serializers/policies, and `release-and-mirror` when evaluating downstream release effects. `/qaas-release <repo> <version>` is a release workflow command; onboarding does not require publishing a package.

For review, ask Claude to use the plugin's `qaas-verifier` and report exact commands and outcomes. An AI explanation is not evidence that a build ran. Inspect generated diffs, verify APIs against the installed package/source version, and compare claims with [published QaaS documentation](https://docs.qaas.online/). A useful prompt is:

> Review this diff for the chosen hook contract, configuration validation, ownership of state, failure propagation, and downstream compatibility. Cite source locations. Separate checks you ran from checks you could not run. Do not publish or change package versions.

Contributors using another agent can load the plugin's portable `AGENTS.md`; that is an alternative integration, not a reason to skip the same source-verification discipline. Keep project secrets and production payloads out of prompts and examples.

## 3. Git and pull-request conventions

Create one branch per coherent change. Use lowercase, hyphen-separated descriptions after the prefix:

| Prefix | Purpose | Example |
|---|---|---|
| `feat/` | A new supported behavior | `feat/generator-record-filter` |
| `fix/` | Correct existing behavior | `fix/session-timeout-reporting` |
| `docs/` | Documentation-only change | `docs/hook-lifecycle-example` |

Use [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/): `type(scope): short description`. For example, `fix(runner): preserve action failure in session result`. Use `feat` for a feature and `docs` for documentation. Describe a breaking contract change with `!` and a `BREAKING CHANGE:` footer explaining the migration. A commit label does not itself bump a package or prove compatibility. Keep unrelated cleanup in a separate change.

Before opening a PR, inspect `.github/PULL_REQUEST_TEMPLATE.md`, `.github/pull_request_template.md`, or `.github/PULL_REQUEST_TEMPLATE/` if present. Use that repository's template. No PR template was found in the inspected Framework, Runner, and Mocker roots, so use this fallback unless your branch adds one:

```markdown
## Problem and behavior
Explain the user-visible trigger, old result, and new result.

## Change and boundaries
List the owning project, affected contracts/configuration, and downstream users.

## Validation
Give exact build/test commands and outcomes, including unresolved failures.
Explain the regression case and why it fails without the change.

## Compatibility and documentation
State YAML/API/package effects, migration needs, and docs/examples updated.

## Review focus
Identify the ownership, concurrency, or release decision needing attention.
```

Prefer a reproducible failing example over a large narrative. For a public change, test a consumer as well as the producing project. Never describe a skipped integration test as passing. Keep generated output and source changes distinguishable in the PR, following the repository's generator workflow.

## 4. Clean Code and SOLID

The objective is code another occasional contributor can safely change. Name operations by their observable purpose, keep validation near the boundary that understands the input, and make mutation, I/O, and failure paths explicit. Extract a method when it reveals a meaningful responsibility; avoid layers whose only purpose is to hide a single line. Comments should explain constraints and tradeoffs rather than repeat syntax. Read Microsoft's [architectural principles](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles) for separation of concerns and dependency inversion.

Apply all five SOLID principles during review:

| Principle | Contribution question |
|---|---|
| Single Responsibility | Does this hook evaluate one domain rule, or also own transport creation and report publishing? |
| Open/Closed | Can the new domain behavior be an external hook instead of another core conditional? |
| Liskov Substitution | Does the implementation honor the interface's expected inputs, outputs, and failure behavior? |
| Interface Segregation | Is the consumer asking only for the capability it uses, rather than an all-purpose service? |
| Dependency Inversion | Does application logic depend on the contract it needs, with concrete selection at composition time? |

These are design questions, not a demand to refactor every older abstraction. A small change should preserve existing semantics unless changing them is the purpose of the PR. Discuss ambiguous ownership before introducing a new public abstraction. [The project-boundary guide](../03-architecture/solution-and-project-structure.md) maps these questions to actual QaaS projects.

## 5. Required design-pattern vocabulary

Design patterns name recurring responsibilities and boundaries; use them to explain why a QaaS extension belongs where it does. An HTTP response assertion makes these ideas concrete: its domain rule can change while Runner keeps control of discovery, execution, and reporting.

### HTTP success assertions and meaningful evidence

An endpoint may return `200`, `201`, or `204` legitimately. A useful assertion checks that **all retained responses are 2xx and at least a configured number arrived**. The count condition prevents an empty output from passing vacuously. This is a domain rule, so implement an external assertion hook; do not add HTTP-specific conditionals to Runner's assertion loop or change Mocker's server implementation.

Declare hook configuration types as **records**, and keep the hook implementations as classes. Use property-based records with defaults and validation attributes, as below. A record does not make mutable properties immutable or replace configuration validation.

The following is the complete proposed hook, not an existing built-in class. Put it in a `net10.0` class library whose QaaS SDK version matches the host. Its concrete signature comes from [BaseAssertion](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.SDK/Hooks/Assertion/BaseAssertion.cs); data selection uses the existing [session/output extensions](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.SDK/Extensions/SessionDataExtensions.cs), and status lives in [HTTP metadata](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.SDK/Session/MetaDataObjects/Http.cs).

```csharp
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace Common.Shampoo;

public sealed record HttpSuccessConfiguration
{
    [Required] public string SessionName { get; set; } = string.Empty;
    [Required] public string OutputName { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int MinimumResponses { get; set; } = 1;
}

public sealed class HttpSuccess : BaseAssertion<HttpSuccessConfiguration>
{
    public override bool Assert(IImmutableList<SessionData> sessions,
        IImmutableList<DataSource> dataSources)
    {
        var output = sessions.GetSessionDataByName(Configuration.SessionName)
            .GetOutputByName(Configuration.OutputName);
        var unexpectedStatuses = 0;
        foreach (var item in output.Data)
        {
            var status = item.MetaData?.Http?.StatusCode
                ?? throw new InvalidOperationException(
                    "HTTP status metadata is missing; retain metadata for this output.");
            if (status < 200 || status > 299)
                unexpectedStatuses++;
        }

        var passed = output.Data.Count >= Configuration.MinimumResponses
            && unexpectedStatuses == 0;
        AssertionMessage = $"Received {output.Data.Count} responses; "
            + $"required at least {Configuration.MinimumResponses}; "
            + $"{unexpectedStatuses} had a non-2xx status.";
        return passed;
    }
}
```

The class has one responsibility: evaluate retained session data. It does not call the endpoint again, open a socket, mutate the output, or publish a report. It uses local counters so repeated calls do not accumulate previous failures. The inherited diagnostic properties are still mutable: do not share one hook instance across concurrent invocations merely because its counter is local.

Reference the hook library/package from the Runner consumer project so its assembly and dependencies reach the application output. Then add this **assertion-section fragment** to a plan that already defines session `checkout` and an HTTP output action named `submit-order`, with metadata retained:

```yaml
Assertions:
  - Name: checkout-http-success
    Assertion: Common.Shampoo.HttpSuccess
    SessionNames: [checkout]
    AssertionConfiguration:
      SessionName: checkout
      OutputName: submit-order
      MinimumResponses: 1
```

`SessionNames` filters the data supplied by Runner; `AssertionConfiguration.SessionName` selects within that filtered data. They are deliberately consistent. The exact outer properties are defined by [AssertionBuilder](https://github.com/TheSmokeTeam/QaaS.Runner/blob/7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba/QaaS.Runner.Assertions/ConfigurationObjects/AssertionBuilder.cs). The fragment is not a complete runnable plan: it requires the existing session, action, and installed hook.

`false` means the observed behavior violated the rule. Missing status metadata or a missing output name is instead a broken precondition and throws. [Assertion.Execute](https://github.com/TheSmokeTeam/QaaS.Runner/blob/7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba/QaaS.Runner.Assertions/AssertionObjects/Assertion.cs) maps thrown exceptions to `Broken`; otherwise it maps the Boolean to `Passed`/`Failed` unless the hook explicitly overrides `AssertionStatus`. This hook leaves that override unset. Configuration binding and DataAnnotations validation remain in the base class; at this source revision successful validation returns an **empty list**, so consumers must not assume that success is exclusively `null`.

To extend the feature to accept a configurable status range, add annotated lower/upper bounds to `HttpSuccessConfiguration`, validate that lower does not exceed upper, replace the two constants, and add boundary tests. Do not silently accept missing metadata as success. No Runner or Mocker API change is needed for this variant; publish/install the new hook package and update its configuration/documentation. Actual protocol changes follow the separate compiled-core route in the project-boundary guide.

Read each linked pattern's intent and structure, then use the question in the final column to reason about QaaS. These are conceptual lenses: a class ending in `Factory` is not automatically the GoF Factory Method, and the pack does not claim every pattern is implemented literally in QaaS.

| Pattern | Concise overview | Application or review question |
|---|---|---|
| [Factory Method](https://refactoring.guru/design-patterns/factory-method) | Defer creation through an overridable creation operation. A static selection factory is a different implementation style. | Who chooses the concrete protocol, and is the creation point actually virtual? |
| [Autofac keyed services / `IIndex`](https://docs.autofac.org/en/latest/advanced/keyed-services.html) | Resolve a registered implementation by a typed key through an injected index. | Is the key registered, and does selection preserve the intended lifetime? |
| [Autofac delegate factories](https://docs.autofac.org/en/latest/advanced/delegate-factories.html) | Inject a typed creation delegate for runtime parameters while the container supplies other dependencies. Parameter matching and scope still matter. | Who owns the objects created by this delegate? |
| [Builder](https://refactoring.guru/design-patterns/builder) | Accumulate construction choices and produce a usable object at a defined boundary. | Which Runner settings are mutable before `Build`, and what is shared afterward? |
| [Singleton](https://refactoring.guru/design-patterns/singleton) | Limit instance creation to one shared object in a defined scope. | Is the shared object container-wide or process-wide, and is its mutable state safe? |
| [Adapter](https://refactoring.guru/design-patterns/adapter) | Translate an existing API into the contract a consumer expects. | Where should transport-specific exceptions and payloads become QaaS behavior? |
| [Bridge](https://refactoring.guru/design-patterns/bridge) | Separate two independently varying dimensions through composition. | Can protocol and payload representation vary without a subclass for every combination? |
| [Composite](https://refactoring.guru/design-patterns/composite) | Treat a group and its individual elements through a compatible operation. | How should a group aggregate startup, shutdown, and child failures? |
| [Facade](https://refactoring.guru/design-patterns/facade) | Offer a focused entry point over collaborating subsystems. | What does the Runner entry point simplify, and what errors must remain visible? |
| [Decorator](https://refactoring.guru/design-patterns/decorator) | Wrap a contract to add behavior while preserving substitutability. | Could timing or logging wrap an operation without swallowing its failure? |
| [Chain of Responsibility](https://refactoring.guru/design-patterns/chain-of-responsibility) | Order handlers so each can act or stop further work. | How does policy order change whether another iteration is allowed? |
| [Strategy](https://refactoring.guru/design-patterns/strategy) | Make an algorithm replaceable behind a stable contract. | Which part varies: domain assertion logic, transport, or serialization? |

`IIndex` and delegate factories are Autofac capabilities to learn, not an assertion that both occur in the current Runner composition. Find actual registrations and consumers before proposing their use. For concrete QaaS construction and policy examples, follow the source links in [Runner execution flow](../01-general-flow/runner-execution-flow.md).

Exercise: choose Builder, Strategy, and Chain of Responsibility. Draw the participating QaaS types, label who calls whom, and identify one limitation of each analogy. Then explain why using a Singleton merely to avoid passing dependencies would be a poor reason to introduce shared state.

For the HTTP success assertion, **Strategy** describes replacing the assertion algorithm while Runner keeps the execution contract. **Adapter** describes the existing transport code translating an HTTP response into QaaS metadata; the new assertion consumes that representation rather than adapting HTTP again. **Builder** belongs to `AssertionBuilder`, which selects the hook and builds the runtime assertion; the hook itself is not a builder. A proposed timing decorator would have to forward `Context`, configuration validation, diagnostics, attachments, and explicit status as well as `Assert`, so wrapping only the Boolean method would not preserve the complete `IAssertion` contract. Runner already times assertions in `Assertion.Execute`; use that measurement unless the new requirement specifically needs a different timing boundary.

## 6. Dependency injection and ownership

DI gives an object its collaborators from outside, usually through its constructor. The composition root decides implementations; business behavior should not need to query a global service locator. Learn the [Microsoft DI model](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview) and [Autofac registration model](https://docs.autofac.org/en/latest/register/registration.html), then inspect the container actually used in the target project.

| Lifetime | Microsoft DI | Autofac analogue | Main review risk |
|---|---|---|---|
| Transient | `AddTransient` | `InstancePerDependency` | Many instances or disposable objects retained too long |
| Scoped | `AddScoped` | `InstancePerLifetimeScope` | A long-lived service captures a shorter-lived dependency |
| Singleton | `AddSingleton` | `SingleInstance` | Concurrent access and accidental state leakage |

The analogues are not a guarantee of identical scope hierarchies. A scope is a deliberate lifetime boundary, not automatically one session or one test. Read [Microsoft service lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/service-lifetimes) and [Autofac instance scopes](https://docs.autofac.org/en/latest/lifetime/instance-scope.html).

The inspected [Runner registrations](https://github.com/TheSmokeTeam/QaaS.Runner/blob/8ccd6f53f9d23645ee117eef48ea3d56f24acfd0/QaaS.Runner/ExecutionBuilder.cs#L730) register context instances as `SingleInstance` and hook collections per lifetime scope. That does not mean all QaaS executions in a process share one context. Trace which builder creates each container and which object disposes it. Registering a type as singleton also does not make it thread-safe.

The same context aliasing is present in the [updated composition root](https://github.com/TheSmokeTeam/QaaS.Runner/blob/7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba/QaaS.Runner/ExecutionBuilder.cs#L729). These are actual source statements inside its child-scope registration callback:

```csharp
containerBuilder.RegisterInstance(Context).As<InternalContext>().SingleInstance();
containerBuilder.RegisterInstance(Context).As<Context>().SingleInstance();
```

Both registrations refer to the **same supplied object**, not two independently constructed contexts. A second execution can supply a different context instance. If a future feature needs execution-local statistics, keep them in an execution-owned object and register it in the corresponding scope; do not move the hook or its counters to a process-wide static field. Add a two-execution isolation test before introducing that state.

Do not assume Autofac constructor injection automatically applies to reflection hooks. The actual [ByNameObjectCreator](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.Providers/ObjectCreation/ByNameObjectCreator.cs) uses `Activator.CreateInstance`; [HookProvider](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.Providers/Providers/HookProvider.cs) discovers the assembly and assigns context. The worked hook deliberately has a public parameterless constructor. Adding a required `HttpClient` or custom service constructor parameter would require checking and changing the creation path, not just adding an unrelated Autofac registration. An assertion over saved responses needs neither dependency.

Exercise: for one hook, record constructor dependencies, context injection, configuration loading, scope creation, and disposal owner. Explain what would happen if two concurrent sessions mutated the same referenced collection. Distinguish immutable collections from mutable objects contained inside them.

## 7. Testing: unit, integration, and contract

Choose the smallest test that proves the behavior, then cover real component boundaries where the change crosses them.

Use the repository's existing test framework and test-double library. The inspected [Framework SDK test project](https://github.com/TheSmokeTeam/QaaS.Framework/blob/4d4b192391418c7882990323f2690d0649a20465/QaaS.Framework.SDK.Tests/QaaS.Framework.SDK.Tests.csproj) uses NUnit and its analyzer; that does not imply every QaaS test project uses the same dependencies. Do not introduce a second mocking library just for an onboarding exercise.

| Technique | Boundary and example | What it cannot establish alone |
|---|---|---|
| Unit | Exercise one policy decision or configuration rule with deterministic inputs and controlled collaborators. | Real broker behavior or package compatibility. |
| Integration | Exercise a real adapter against a local service, or load a real assembly/configuration through the application boundary. | All production failure modes and deployment environments. |
| Contract | Verify the promised interaction shape and semantics between a consumer and provider, such as a serialized command or public hook contract. | Internal correctness of either side beyond that contract. |

Read [Microsoft's unit-test guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices), [The Practical Test Pyramid](https://martinfowler.com/articles/practical-test-pyramid.html), and [Contract Test](https://martinfowler.com/bliki/ContractTest.html). Prefer observable results over assertions about incidental implementation order. An interaction assertion is useful when that interaction itself is the requirement, such as disconnecting an owned connection.

Learn both [Moq](https://github.com/devlooped/moq/wiki/Quickstart) and [NSubstitute](https://nsubstitute.github.io/help/getting-started/). Both can replace interface collaborators; neither turns an unrealistic fake into proof that a production integration works. The following tiny examples are **standalone learning snippets**, not QaaS APIs or a request to add both packages:

```csharp
// In a test project already using Moq:
var clock = new Moq.Mock<IClock>();
clock.SetupGet(x => x.UtcNow).Returns(DateTimeOffset.UnixEpoch);
var observed = clock.Object.UtcNow;
clock.VerifyGet(x => x.UtcNow, Moq.Times.Once());

public interface IClock { DateTimeOffset UtcNow { get; } }
```

```csharp
// In an alternative test project already using NSubstitute:
using NSubstitute;
var clock = Substitute.For<IClock>();
clock.UtcNow.Returns(DateTimeOffset.UnixEpoch);
var observed = clock.UtcNow;
_ = clock.Received(1).UtcNow;

public interface IClock { DateTimeOffset UtcNow { get; } }
```

Exercise: write a three-row test plan for a configuration change: a valid value, a boundary value, and a rejected value. Add an integration or contract case only where a real boundary is affected. State what would fail without the fix. Avoid sleeps to coordinate tests; use controllable signals or bounded waits with clear diagnostics.

### Testing the assertion boundary

The assertion boundary needs tests for both status handling and the amount of evidence required to pass. The complete NUnit fixture below constructs real SDK data records, so the cases exercise the proposed hook against the SDK data shape shown in the source links.

```csharp
using System.Collections.Immutable;
using Common.Shampoo;
using NUnit.Framework;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

public sealed class HttpSuccessBoundaryTests
{
    [TestCase(199, false)]
    [TestCase(200, true)]
    [TestCase(201, true)]
    [TestCase(204, true)]
    [TestCase(299, true)]
    [TestCase(300, false)]
    public void Status_boundaries_are_explicit(int status, bool expected)
    {
        var hook = new HttpSuccess
        {
            Configuration = new HttpSuccessConfiguration
                { SessionName = "checkout", OutputName = "submit-order" }
        };
        var sessions = ImmutableList.Create(new SessionData
        {
            Name = "checkout",
            Outputs = [new CommunicationData<object>
            {
                Name = "submit-order",
                Data = [new DetailedData<object>
                {
                    MetaData = new MetaData { Http = new Http { StatusCode = status } }
                }]
            }]
        });
        Assert.That(hook.Assert(sessions, ImmutableList<DataSource>.Empty),
            Is.EqualTo(expected));
    }
}
```

This unit test assigns configuration directly to isolate the predicate. A separate configuration test must invoke the real `LoadAndValidateConfiguration` with `Context.Logger` initialized; otherwise it does not establish YAML/configuration binding. A separate discovery check must instantiate the public hook through `ByNameObjectCreator`, rather than relying solely on `new HttpSuccess()`. The assertion rule also needs cases for no responses, too few responses, missing metadata, a nonexistent output, and a failed call followed by a successful call on the same instance.

For this guide, a separate scratch project referenced the actual Framework Providers project at `11f3ec6` (and its SDK dependency), targeted .NET 10, and ran **21 NUnit cases: 21 passed, 0 failed, 0 skipped**. Fifteen checks covered the six status boundaries above, empty output, minimum count, missing metadata, wrong output name, two real configuration-binding/validation cases, reflection creation, repeated-call state, and two execution-context scopes. The exact displayed test fixture was then extracted from this Markdown and contributed six additional boundary cases to verify that the published snippet itself compiles and runs. This proves compilation and those behaviors; it does not claim a live HTTP Runner/Mocker scenario or a published package-consumer test. Restore reported the existing transitive MessagePack `3.1.7` NU1902 advisory; the focused tests still completed successfully.

## 8. Modern C# and .NET

Know nullable reference types, generics and constraints, interfaces, delegates, LINQ's deferred execution, records, pattern matching, extension methods, and `IDisposable`. A record's value-oriented syntax does not make every referenced object immutable. Re-enumerating `IEnumerable<T>` may repeat work or consume a one-shot data source. Start with the [C# language reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/) and [record semantics](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record).

For asynchronous code, distinguish starting work from awaiting its completion. `async` does not guarantee a new thread, and cancellation requires cooperation. Account for exceptions in every task you create; a forgotten task can outlive the state it uses. Study [async/await](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/), then compare it with Runner's actual task coordination in the lifecycle guide rather than rewriting synchronous contracts reflexively.

For performance, establish a measurement before optimizing. Look for repeated enumeration, unbounded buffering, excessive allocations, blocking I/O, and unnecessary serialization. The [garbage-collection overview](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/) explains managed memory; [diagnostic tools](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/) help determine the actual bottleneck. Garbage collection does not replace timely cleanup of sockets, files, or container-owned resources. Pooling and shared caches add ownership obligations and should follow evidence, not guesswork.

The worked hook reads `CommunicationData<object>.Data`, an `IList<DetailedData<object>>`, so `Count` is available without re-enumerating a deferred source. It examines only metadata and does not deserialize or clone response bodies. A feature that inspects JSON bodies would need a separate declared body contract and cases for null, malformed, and differently deserialized bodies; casting every `Body` to `string` is not justified by `Data<object>`.

The [SDK data record](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.SDK/Session/DataObjects/Data.cs) exposes `Body` and `MetaData` as `init` properties. That prevents reassigning them after construction but does not freeze a dictionary stored inside the body. If a new feature transforms output for comparison, create a separate transformed value; mutating the shared payload would affect later assertions. Use the existing synchronous assertion contract here: marking a helper `async` does not make `IAssertion.Assert` asynchronous, and unawaited work could finish after Runner records the result.

Exercise: inspect one data pipeline and mark the points that materialize a sequence, allocate a payload copy, wait for a task, and release an external resource. Explain which change would alter semantics, even if it appears faster.

## 9. C# style, `.editorconfig`, and Markdown

Follow the nearest applicable `.editorconfig`, repository formatter configuration, and the surrounding code. Check parent directories as well as the project directory because EditorConfig settings can be inherited. The inspected Framework, Runner, and Mocker repositories had no tracked `.editorconfig`; do not claim this pack's preferences are existing enforced rules. If one appears on your branch, it governs the files in its scope. Read [code-analysis configuration](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files) before changing diagnostic severity.

Use descriptive PascalCase type/member names and camelCase parameters/locals, `I`-prefixed interface names, and `Async` for new task-returning asynchronous operations unless an existing contract dictates otherwise. Keep public API names stable unless a breaking change is intentional. Respect the project's nullable annotations and do not silence warnings with `!` without proving the invariant. Microsoft's [C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) provide a fallback, not permission to override a repository's established formatter.

Write Markdown with one descriptive H1, ordered heading levels, blank lines around lists and fences, language tags on code blocks, and meaningful link labels. Use relative links for files within this pack and published `docs.qaas.online` URLs for user documentation. Name the working directory for shell commands. Label conceptual snippets and avoid example credentials. Check rendered tables and diagrams in the intended viewer; Mermaid support varies, so each diagram should also have a textual explanation. [CommonMark](https://spec.commonmark.org/) defines the baseline syntax; GitHub adds tables and Mermaid rendering.

For the worked feature, `[Required]` and `[Range]` are executable configuration rules, whereas prose such as “requires one response” is documentation. Keep both synchronized. Add XML documentation to the new public hook/configuration members before contributing the example as a production package, following neighboring hooks; the compact teaching snippet omits those comments. A useful PR description is: “Accept every 2xx response from the configured output while failing when fewer than `MinimumResponses` arrive; distinguish missing HTTP metadata from an observed non-2xx response.” Follow it with the exact tests run and remaining host integration checks. This describes the behavior a reviewer can verify, rather than merely saying that the hook was added.

## Completion check

You are ready for the architecture modules when you can route a proposed change with the developer plugin, explain its contract and lifetime, select a meaningful regression test, and draft a PR with reproducible evidence. Keep your baseline command output and the short exercises as personal review notes. Continue with [solution and project structure](../03-architecture/solution-and-project-structure.md), then [Runner execution flow](../01-general-flow/runner-execution-flow.md).
