## Q11. Serialization boundaries

Trace serialization through configuration, messages, responses, and stored evidence. Explain format selection, nulls, type hints, and casting.

**Code hint:** [QaaS.Framework: SerializerFactory.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Serialization/SerializerFactory.cs); [QaaS.Framework: DeserializerFactory.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Serialization/DeserializerFactory.cs); [QaaS.Framework: DataSource.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/DataSourceObjects/DataSource.cs).

**Graph hint:** [Roadmap stage 3](qaas-ecosystem/qaas-ecosystem.html#learn=contracts). Open [SerializerFactory](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Serialization%2FSerializerFactory.cs%23188&tab=behavior&stage=contracts) in **Behavior & members** and follow format selection to its implementations. Use **Uses / used by** to move outward toward data sources and runtime consumers, checking **Source & signature** at each conversion boundary.

## Q12. Extension methods and lookups

Map Framework extension methods to their consumers. Demonstrate named session and communication lookups, and locate assertion selection.

**Code hint:** [QaaS.Framework: SessionDataExtensions.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Extensions/SessionDataExtensions.cs); [QaaS.Framework: ContextExtensions.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Extensions/ContextExtensions.cs); [QaaS.Runner: ConfigurationExtensions.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Extensions/ConfigurationExtensions.cs).

**Graph hint:** [Roadmap stage 3](qaas-ecosystem/qaas-ecosystem.html#learn=contracts). Inspect [SessionDataExtensions](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FExtensions%2FSessionDataExtensions.cs%23257&tab=behavior&stage=contracts) in **Behavior & members**, then use **Uses / used by** on individual lookup methods to find consumers. Compare [ConfigurationExtensions](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FExtensions%2FConfigurationExtensions.cs%23370&tab=source&stage=contracts) in **Source & signature** before deciding which lookup or selection API belongs to each use case.

## Q13. Hook discovery

Trace discovery, selection, creation, and configuration of a custom hook. Diagnose duplicate names, unloadable assemblies, and invalid constructors.

**Code hint:** [QaaS.Framework: HookProvider.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Providers/Providers/HookProvider.cs); [QaaS.Framework: ByNameObjectCreator.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Providers/ObjectCreation/ByNameObjectCreator.cs); [QaaS.Framework: HooksFromProvidersLoader.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Providers/HooksFromProvidersLoader.cs).

**Graph hint:** [Roadmap stage 3](qaas-ecosystem/qaas-ecosystem.html#learn=contracts). Start with the selector name of a hook you wrote and open [HookProvider](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Providers%2FProviders%2FHookProvider.cs%23257&tab=behavior&stage=contracts). Follow discovery and creation relationships in **Uses / used by**, then read **Source & signature** for the selection and construction methods to evaluate the three failure scenarios.

## Q14. Builder architecture

Draw the Framework and Runner builder relationships. Explain each role and distinguish registration, building, fluent mutation, and cloning.

**Code hint:** [QaaS.Framework: BaseExecutionBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Executions/BaseExecutionBuilder.cs); [QaaS.Framework: DataSourceBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/DataSourceObjects/DataSourceBuilder.cs); [QaaS.Runner: SessionBuilderLogic.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Builders/SessionBuilderLogic.cs).

**Graph hint:** [Roadmap stage 4](qaas-ecosystem/qaas-ecosystem.html#learn=execution). Begin at [BaseExecutionBuilder](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Executions%2FBaseExecutionBuilder.cs%23349&tab=overview&stage=execution) and follow inheritance and construction relationships in **Uses / used by**. Compare [DataSourceBuilder](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FDataSourceObjects%2FDataSourceBuilder.cs%23905&tab=behavior&stage=execution) with the Runner builders in **Behavior & members**, marking each operation by what it changes or returns.

## Q15. Execution identity and lifetime

What is an execution, and where does its ID come from? Compare Runner and Mocker plans, scheduling, exit codes, and disposal.

**Code hint:** [QaaS.Runner: Execution.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Execution.cs); [QaaS.Mocker: Execution.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/Execution.cs); [QaaS.Framework: ContextBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/ContextObjects/ContextBuilder.cs).

**Graph hint:** [Roadmap stage 4](qaas-ecosystem/qaas-ecosystem.html#learn=execution). Compare [Runner Execution](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FExecution.cs%231269&tab=overview&stage=execution) and [Mocker Execution](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Mocker%7CQaaS.Mocker%2FExecution.cs%23278&tab=overview&stage=execution) in **Overview**. Inspect their members and follow creation and disposal relationships in **Uses / used by**, checking identity assignments in **Source & signature** rather than inferring them from names.

## Q16. Composing an execution

Walk through ExecutionBuilder.Build. How are contexts, hooks, data sources, runtime objects, and command-specific logic assembled?

**Code hint:** [QaaS.Runner: ExecutionBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/ExecutionBuilder.cs); [QaaS.Mocker: ExecutionBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/ExecutionBuilder.cs).

**Graph hint:** [Roadmap stage 4](qaas-ecosystem/qaas-ecosystem.html#learn=execution). Open [Runner ExecutionBuilder](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FExecutionBuilder.cs%231985&tab=behavior&stage=execution) and select its Build member. Follow construction relationships in **Uses / used by**, then compare [Mocker ExecutionBuilder](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Mocker%7CQaaS.Mocker%2FExecutionBuilder.cs%231427&tab=behavior&stage=execution) in **Behavior & members**, recording the order and ownership of assembled objects.

## Q17. Session scheduling and completion

Trace a session from its builder to completed SessionData. Identify launch order, overlap, waits, collectors, and cleanup.

**Code hint:** [QaaS.Runner: SessionBuilderLogic.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Builders/SessionBuilderLogic.cs); [QaaS.Runner: Session.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Session.cs); [QaaS.Runner: Stage.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Stage.cs).

**Graph hint:** [Roadmap stage 4](qaas-ecosystem/qaas-ecosystem.html#learn=execution). Start at [Session](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner.Sessions%2FSession%2FSession.cs%23711&tab=behavior&stage=execution) and inspect the run and completion members in **Behavior & members**. Follow its Stage and action relationships through **Uses / used by**, then check **Source & signature** wherever a task is started, awaited, or cleaned up.

## Q18. Capturing and restoring results

Follow action results into storage and an assertion-only run. How do identity, serialization, and filtering affect a round trip?

**Code hint:** [QaaS.Runner: Session.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Session.cs); [QaaS.Runner: StorageLogic.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Logics/StorageLogic.cs); [QaaS.Runner: BaseStorage.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Storage/BaseStorage.cs).

**Graph hint:** [Roadmap stage 4](qaas-ecosystem/qaas-ecosystem.html#learn=execution). Open [StorageLogic](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FLogics%2FStorageLogic.cs%23397&tab=behavior&stage=execution) and trace saving and loading in **Behavior & members**. Follow storage and session relationships through **Uses / used by**, then inspect **Source & signature** at naming, serialization, and filtering points to sketch a round trip.

## Q19. Assertions, reports, and exit codes

How does an assertion become a status, report, and exit code? Distinguish hook results, exceptions, session failures, and reporting failures.

**Code hint:** [QaaS.Runner: Assertion.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Assertions/AssertionObjects/Assertion.cs); [QaaS.Runner: ReportLogic.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Logics/ReportLogic.cs); [QaaS.Runner: Runner.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Runner.cs).

**Graph hint:** [Roadmap stage 4](qaas-ecosystem/qaas-ecosystem.html#learn=execution). Start at [Assertion](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner.Assertions%2FAssertionObjects%2FAssertion.cs%23543&tab=behavior&stage=execution) and compare the paths for a returned result and an exception in **Behavior & members**. Follow the result toward reporting and execution in **Uses / used by**, then verify the status-to-exit-code boundary in **Source & signature**.

## Q20. The Mocker request lifecycle

Trace a request through server routing, a stub, a processor, and the response. Explain hosting, concurrency, failure, and shutdown boundaries.

**Code hint:** [QaaS.Mocker: ServerFactory.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker.Servers/ServerFactory.cs); [QaaS.Mocker: TransactionStub.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker.Stubs/Stubs/TransactionStub.cs); [QaaS.Mocker: CompositeServer.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker.Servers/Servers/CompositeServer.cs).

**Graph hint:** [Roadmap stage 5](qaas-ecosystem/qaas-ecosystem.html#learn=coordination). Use the [Mocker flow](qaas-ecosystem/qaas-ecosystem.html#view=mocker) to locate the request you want to trace. Open [TransactionStub](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Mocker%7CQaaS.Mocker.Stubs%2FStubs%2FTransactionStub.cs%23467&tab=behavior&stage=coordination) in **Behavior & members**, then follow server, processor, and serialization relationships in **Uses / used by**, inspecting source at failure and shutdown boundaries.
