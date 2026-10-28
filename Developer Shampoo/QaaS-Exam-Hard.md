## Q21. Policy chains

Explain policy ordering, pacing, and stopping. How does the RunChain call site affect counts, timeouts, and work already running?

**Code hint:** [QaaS.Framework: Policy.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Policies/Policy.cs); [QaaS.Framework: PolicyBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Policies/PolicyBuilder.cs); [QaaS.Runner: Publisher.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Actions/Publishers/Publisher.cs).

**Graph hint:** [Roadmap stage 5](qaas-ecosystem/qaas-ecosystem.html#learn=coordination). Inspect [Policy](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Policies%2FPolicy.cs%2383&tab=behavior&stage=coordination) in **Behavior & members**, then select RunChain and follow its incoming relationships in **Uses / used by**. Compare at least two call sites in **Source & signature** before reasoning about counts, elapsed time, and work already launched.

## Q22. Configuration ownership and mutation

Why restrict configuration visibility? Compare typed patches, object patches, and clones when nulls, defaults, collections, and shared references are involved.

**Code hint:** [QaaS.Framework: ConfigurationUtils.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Configurations/ConfigurationUtils.cs); [QaaS.Runner: SessionBuilderProperties.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Builders/SessionBuilderProperties.cs); [QaaS.Runner: ExecutionBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/ExecutionBuilder.cs).

**Graph hint:** [Roadmap stage 5](qaas-ecosystem/qaas-ecosystem.html#learn=coordination). Open [ConfigurationUtils](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Configurations%2FConfigurationUtils.cs%23646&tab=behavior&stage=coordination) and compare its mutation helpers in **Behavior & members**. Follow their consumers in **Uses / used by**, then inspect builder members in **Source & signature** to track which values and references can survive a patch or clone.

## Q23. Dependency injection and ownership

Audit registrations, resolution, scopes, sharing, and disposal in both runtimes. Which isolation guarantees can you actually prove?

**Code hint:** [QaaS.Runner: ExecutionBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/ExecutionBuilder.cs); [QaaS.Runner: Execution.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Execution.cs); [QaaS.Mocker: Execution.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/Execution.cs).

**Graph hint:** [Roadmap stage 5](qaas-ecosystem/qaas-ecosystem.html#learn=coordination). Start at [Runner ExecutionBuilder](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FExecutionBuilder.cs%231985&tab=behavior&stage=coordination) and locate registration and resolution in **Behavior & members**. Follow execution ownership through **Uses / used by**, then compare [Mocker Execution](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Mocker%7CQaaS.Mocker%2FExecution.cs%23278&tab=source&stage=coordination) in **Source & signature** when checking scope and disposal claims.

## Q24. Runner-to-Mocker control

Design a Runner-to-Mocker control exchange. Explain discovery, commands, correlation, evidence retrieval, timeouts, and acknowledgements.

**Code hint:** [QaaS.Runner: MockerCommand.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Actions/MockerCommands/MockerCommand.cs); [QaaS.Mocker: CommandHandler.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker.Controller/Handlers/CommandHandler.cs); [Qaas.Mocker.CommunicationObjects: CommunicationMethods.cs](https://github.com/TheSmokeTeam/Qaas.Mocker.CommunicationObjects/blob/3c059afcab05642f6efaf60261a845a65b703631/Qaas.Mocker.CommunicationObjects/CommunicationMethods.cs).

**Graph hint:** [Roadmap stage 5](qaas-ecosystem/qaas-ecosystem.html#learn=coordination). Open [MockerCommand](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner.Sessions%2FActions%2FMockerCommands%2FMockerCommand.cs%23937&tab=behavior&stage=coordination) in **Behavior & members** and follow its communication relationships in **Uses / used by**. Switch to the [Mocker flow](qaas-ecosystem/qaas-ecosystem.html#view=mocker) to continue on the receiving side, then inspect command and response source for correlation and timeout boundaries.

## Q25. Tasks and failure propagation

Trace Task creation, waiting, cancellation, and cleanup across both runtimes. Which failures become evidence, which escape, and how would you verify this?

**Code hint:** [QaaS.Runner: SessionExtensions.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Extensions/SessionExtensions.cs); [QaaS.Runner: SessionLogic.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Logics/SessionLogic.cs); [QaaS.Mocker: Execution.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/Execution.cs).

**Graph hint:** [Roadmap stage 5](qaas-ecosystem/qaas-ecosystem.html#learn=coordination). Inspect [SessionExtensions](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner.Sessions%2FExtensions%2FSessionExtensions.cs%23446&tab=behavior&stage=coordination) and follow task-related consumers in **Uses / used by**. Compare [Mocker Execution](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Mocker%7CQaaS.Mocker%2FExecution.cs%23278&tab=source&stage=coordination) in **Source & signature**, recording the point where each failure is observed and the cleanup that can still execute.

## Q26. Parallelism and shared-state invariants

Analyze out-of-order responses and shared state across QaaS's concurrency levels. What happens when a policy stops a partially running batch?

**Code hint:** [QaaS.Runner: IterableSerializableDataIterator.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Extensions/IterableSerializableDataIterator.cs); [QaaS.Runner: Transaction.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Actions/Transactions/Transaction.cs); [QaaS.Mocker: MockerRunner.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/MockerRunner.cs).

**Graph hint:** [Roadmap stage 5](qaas-ecosystem/qaas-ecosystem.html#learn=coordination). Start at [IterableSerializableDataIterator](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner.Sessions%2FExtensions%2FIterableSerializableDataIterator.cs%23292&tab=behavior&stage=coordination) and inspect the scheduling members in **Behavior & members**. Follow the transaction and policy relationships in **Uses / used by**, then compare shared fields and completion paths in **Source & signature** using one deliberately out-of-order scenario.

## Q27. Choosing an extension boundary

Design a configurable generator and assertion, then add a new transport. Defend the extension points, design patterns, and verification strategy.

**Code hint:** [QaaS.Framework: BaseGenerator.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Hooks/Generator/BaseGenerator.cs); [QaaS.Framework: BaseAssertion.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Hooks/Assertion/BaseAssertion.cs); [QaaS.Framework: TransactorFactory.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Protocols/Protocols/Factories/TransactorFactory.cs).

**Graph hint:** [Roadmap stage 6](qaas-ecosystem/qaas-ecosystem.html#learn=change). Compare [BaseGenerator](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FHooks%2FGenerator%2FBaseGenerator.cs%23708&tab=overview&stage=change) with [TransactorFactory](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Protocols%2FProtocols%2FFactories%2FTransactorFactory.cs%23486&tab=behavior&stage=change), using **Overview** for contracts and **Behavior & members** for construction. Follow their consumers in **Uses / used by**, then use **Documentation** to check the extension boundaries your proposal depends on.

## Q28. Releasing a cross-project change

Trace hook and SDK changes through consumers, packages, schemas, and documentation. How would you prove the resulting artifacts are compatible and current?

**Code hint:** [QaaS.PackageMirror: sync-packages.yml](https://github.com/TheSmokeTeam/QaaS.PackageMirror/blob/903007e48765875931609236ccb3599daf93cf96/.github/workflows/sync-packages.yml); [QaaS.Docs.Generator: Program.cs](https://github.com/TheSmokeTeam/QaaS.Docs.Generator/blob/8fe81cf340e63302252bce1e487cc121d5be1e92/Program.cs); [QaaS.Configuration: QaaS.Configuration.targets](https://github.com/TheSmokeTeam/QaaS.Configuration/blob/aecb042d9ae0a827c1b314a4e4ff0c051d549191/QaaS.Configuration/buildTransitive/QaaS.Configuration.targets).

**Graph hint:** [Roadmap stage 6](qaas-ecosystem/qaas-ecosystem.html#learn=change). Start with the [release and tooling flow](qaas-ecosystem/qaas-ecosystem.html#view=release) and trace the artifacts affected by your proposed SDK change. Open [documentation generator](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Docs.Generator%7CProgram.cs%23214&tab=behavior&stage=change) in **Behavior & members**, then inspect connection explanations and **Source & signature** for the package, schema, and documentation handoffs.

## Q29. Diagnosing misleading success

Investigate missing replies in a passing run and unexpected evidence in an assertion-only run. Separate configuration, discovery, concurrency, storage, and reporting causes.

**Code hint:** [QaaS.Framework: ConfigurationUtils.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Configurations/ConfigurationUtils.cs); [QaaS.Framework: ByNameObjectCreator.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Providers/ObjectCreation/ByNameObjectCreator.cs); [QaaS.Runner: StorageLogic.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Logics/StorageLogic.cs).

**Graph hint:** [Roadmap stage 6](qaas-ecosystem/qaas-ecosystem.html#learn=change). Use the [Runner flow](qaas-ecosystem/qaas-ecosystem.html#view=runner) to mark where you first have evidence for each symptom. Follow [StorageLogic](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FLogics%2FStorageLogic.cs%23397&tab=connections&stage=change) through **Uses / used by** and inspect relevant discovery, action, and reporting members in **Source & signature**; keep competing explanations until a source-backed check rules each out.

## Q30. Defending an end-to-end design

Design a suite with messaging, a changing HTTP mock, eventual results, and later assertion-only analysis. Defend ownership, synchronization, evidence, failure handling, and release compatibility.

**Code hint:** [QaaS.Runner: ExecutionBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/ExecutionBuilder.cs); [QaaS.Mocker: TransactionStub.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker.Stubs/Stubs/TransactionStub.cs); [QaaS.Runner: MockerCommand.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Actions/MockerCommands/MockerCommand.cs).

**Graph hint:** [Roadmap stage 6](qaas-ecosystem/qaas-ecosystem.html#learn=change). Place your proposed flow across the [Runner](qaas-ecosystem/qaas-ecosystem.html#view=runner) and [Mocker](qaas-ecosystem/qaas-ecosystem.html#view=mocker) views, then trace critical ownership and communication edges in **Uses / used by**. Check **Source & signature** and **Documentation** for each guarantee your design relies on, and revisit the release flow for compatibility assumptions.
