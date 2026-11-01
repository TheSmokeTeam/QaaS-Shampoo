# Understanding hooks, policies, and payload boundaries

This page explains the extension decisions behind the HTTP-success assertion, maintenance-response processor, and policy examples. Read the inline assertion in [conventions and guidelines](../../02-standards/conventions-and-guidelines.md), then the processor, serialization, and policy implementations in [solution and project structure](../../03-architecture/solution-and-project-structure.md). The aim is to understand what each layer promises and which failures belong to another layer.

## Start with the extension boundary

Domain behavior and infrastructure behavior enter QaaS through different mechanisms. Choose the boundary before choosing a class name.

An assertion changes how retained evidence is judged. A processor changes the response produced by an existing Mocker stub. Both can use the SDK's reflection-discovered hook contracts. They do not need a new transport merely because the business behavior is new. By contrast, a new wire protocol, serialization format, or policy type requires changes to the framework's compiled selection paths.

This distinction limits the change's consequences. Returning a maintenance response over existing HTTP should remain a processor feature. Adding a different response DTO should remain a payload-contract change. Editing protocol factories for either requirement introduces unnecessary downstream responsibilities. When the existing contract genuinely cannot express the feature, identify exactly which promise must change before widening the implementation.

## Make an assertion's evidence contract explicit

`HttpSuccess` judges one named output from one named session. Its minimum count prevents “nothing failed” from becoming “the scenario passed.”

Runner's `SessionNames` selection controls which session data reaches the assertion. The hook's `SessionName` and `OutputName` then select within that evidence. Keeping them consistent matters: a perfectly valid output elsewhere cannot satisfy a hook that was intentionally given a narrower snapshot. The SDK name helpers keep the lookup explicit rather than silently evaluating an arbitrary first session.

The predicate requires at least `MinimumResponses` and accepts statuses 200 through 299. A non-2xx status is observed behavior that fails the rule. Missing HTTP metadata is different: the assertion cannot evaluate its declared contract, so it throws. [Assertion.Execute](https://github.com/TheSmokeTeam/QaaS.Runner/blob/7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba/QaaS.Runner.Assertions/AssertionObjects/Assertion.cs) distinguishes that broken outcome from a normal false result. The hook leaves its explicit status override unset, allowing Runner's Boolean mapping to apply.

Configurable status bounds would be a reasonable extension, but two valid integers are not sufficient validation: the lower bound must also be no greater than the upper bound. Preserve the missing-evidence distinction when extending the predicate.

## Keep state and construction understandable

Local counters help repeated calls remain independent, but they do not automatically make an entire hook thread-safe.

`HttpSuccess` counts unexpected statuses locally and writes a diagnostic message for a completed evaluation. It does not accumulate a static failure count or modify retained responses. Its inherited diagnostics remain mutable, so sharing one instance across concurrent evaluations still needs an ownership decision. Similarly, immutable session collections do not freeze every object referenced by their elements.

Reflection creation is also distinct from ordinary constructor injection. [ByNameObjectCreator](https://github.com/TheSmokeTeam/QaaS.Framework/blob/11f3ec66126cf878dc34c4fab8c152ced521f3ea/QaaS.Framework.Providers/ObjectCreation/ByNameObjectCreator.cs) uses `Activator.CreateInstance`, which explains the public parameterless constructor. Registering a new constructor dependency in Autofac elsewhere does not change that path. The composition root's `Context` and `InternalContext` registrations alias the same supplied context object; another execution can supply another object. Container lifetime labels alone establish neither process-wide sharing nor safe concurrent mutation.

## Trace a maintenance response across its layers

The maintenance processor describes the response; serialization and the HTTP server turn it into a transport response.

`MaintenanceResponse` creates a new envelope containing a `MaintenancePayload` record and HTTP metadata. The status is 503, while `Retry-After` expresses the configured delay in an invariant string format. That header requests client behavior; the processor does not sleep. Each call creates a fresh headers dictionary, preventing one response's mutation from leaking into another.

The record itself is not yet a writable HTTP body. [TransactionStub.Exercise](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/0407e6b038b3ada1f48ed837bc6fd7b3022e0299/QaaS.Mocker.Stubs/Stubs/TransactionStub.cs) uses the configured response serializer to produce bytes while preserving metadata. Without that serializer, this non-byte body is rejected. The [HTTP writer](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/0407e6b038b3ada1f48ed837bc6fd7b3022e0299/QaaS.Mocker.Servers/Extensions/HttpExtensions.cs) applies status and headers separately from the body. Putting `503` inside JSON would not set the HTTP status.

Calling the mock directly verifies its response contract. Proving an application's retry behavior requires the application to call that mock and evidence of the application's subsequent behavior. Runner's transport retries should not be assumed to retry arbitrary 503 responses.

## Treat serialization as a declared representation

A DTO defines a logical shape; the serializer defines its wire representation and the deserializer defines the resulting runtime shape.

Selecting JSON for `MaintenancePayload` reuses an existing format. Supplying `typeof(MaintenancePayload)` during deserialization requests the typed record, whereas a null target type requests the JSON implementation's `JsonNode` representation. A consumer cannot safely cast every resulting body to a string or a Newtonsoft token.

The factories return null for a null format, and the JSON implementation preserves null payloads. Those outcomes need meaning at the caller: absence may be valid for one operation and an invalid precondition for another. A new encoding needs both encode and decode behavior, compatibility checks, and downstream adoption; a new JSON business schema does not automatically need a new encoding.

## Read policies at the actual checkpoint

Policy composition controls continuation at checkpoints; it does not retroactively cancel all work already started.

Count and timeout use separate `PolicyBuilder` instances because `Configure` resets the previous selection. Configuring both on one builder leaves only the later policy. Building the chain and calling `SetupChain` establishes runtime policy state before use.

The count policy stops at its Nth checkpoint. Runner transactions reach that checkpoint after the operation and result logging, so moving the call into a loop condition before sending changes the count boundary. Concurrent operations may already be in flight when continuation is denied. An elapsed-time policy is consequently not a hard deadline for current I/O; transport timeouts remain a separate concern. Trace policy state, checkpoint location, and operation ownership together before adding a new stop condition.
