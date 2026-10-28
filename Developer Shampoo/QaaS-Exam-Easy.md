## Q1. The core ecosystem

What roles do Framework, Runner, Mocker, and the shared hooks play? Show how they relate in a small diagram.

**Code hint:** [QaaS.Framework: QaaS.Framework.sln](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.sln); [QaaS.Runner: QaaS.Runner.sln](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.sln); [QaaS.Mocker: QaaS.Mocker.sln](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker.sln).

**Graph hint:** [Roadmap stage 1](qaas-ecosystem/qaas-ecosystem.html#learn=orient). Start with the [ecosystem view](qaas-ecosystem/qaas-ecosystem.html#view=ecosystem) and select the components you used as a customer. Read their connection explanations, then use the [project view](qaas-ecosystem/qaas-ecosystem.html#view=projects) to place shared contracts and hooks in your diagram.

## Q2. The units of a test

How do cases, sessions, actions, and assertions differ? Explain how names and categories select work.

**Code hint:** [QaaS.Runner: ConfigurationExtensions.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Extensions/ConfigurationExtensions.cs); [QaaS.Runner: SessionBuilderProperties.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Builders/SessionBuilderProperties.cs).

**Graph hint:** [Roadmap stage 1](qaas-ecosystem/qaas-ecosystem.html#learn=orient). Open the [Runner flow](qaas-ecosystem/qaas-ecosystem.html#view=runner) and locate the units represented in a configuration you have run. Inspect [ConfigurationExtensions](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FExtensions%2FConfigurationExtensions.cs%23370&tab=behavior&stage=orient) in **Behavior & members**, then follow **Uses / used by** to connect selection rules with the work they select.

## Q3. Hook contracts

When would you use a generator, assertion, probe, or processor? Illustrate one contract with a small generator.

**Code hint:** [QaaS.Framework: IHook.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Hooks/IHook.cs); [QaaS.Framework: BaseGenerator.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Hooks/Generator/BaseGenerator.cs); [QaaS.Framework: ITransactionProcessor.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Hooks/Processor/ITransactionProcessor.cs).

**Graph hint:** [Roadmap stage 1](qaas-ecosystem/qaas-ecosystem.html#learn=orient). Begin with your own small hook and compare it with [BaseGenerator](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FHooks%2FGenerator%2FBaseGenerator.cs%23708&tab=overview&stage=orient). Read **Overview**, then inspect **Behavior & members** and the neighboring hook contracts; use **Documentation** to check which guarantees belong to the contract rather than your implementation.

## Q4. Action families

Compare the six Runner action families. What does each receive, do, and produce?

**Code hint:** [QaaS.Runner: SessionBuilderLogic.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Session/Builders/SessionBuilderLogic.cs); [QaaS.Runner: Transaction.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Actions/Transactions/Transaction.cs); [QaaS.Runner: Collector.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner.Sessions/Actions/Collectors/Collector.cs).

**Graph hint:** [Roadmap stage 1](qaas-ecosystem/qaas-ecosystem.html#learn=orient). Locate action construction in the [Runner flow](qaas-ecosystem/qaas-ecosystem.html#view=runner). Open [Session](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner.Sessions%2FSession%2FSession.cs%23711&tab=behavior&stage=orient) in **Behavior & members**, then follow action relationships in **Uses / used by** and compare the input and output signatures of each family.

## Q5. Communication data

Follow one request and response through QaaS's data model. Distinguish payloads, metadata, live buffers, and completed evidence.

**Code hint:** [QaaS.Framework: Data.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Session/DataObjects/Data.cs); [QaaS.Framework: CommunicationData.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Session/CommunicationDataObjects/CommunicationData.cs); [QaaS.Framework: RunningSessionData.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/Session/SessionDataObjects/RunningSessionData.cs).

**Graph hint:** [Roadmap stage 1](qaas-ecosystem/qaas-ecosystem.html#learn=orient). Open [CommunicationData](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FSession%2FCommunicationDataObjects%2FCommunicationData.cs%23198&tab=overview&stage=orient) and match its terms to a request and response you observed. Follow its data and session relationships through **Uses / used by**, then inspect the relevant members in **Source & signature** to distinguish the containers involved.

## Q6. Starting QaaS

How do Runner and Mocker start from the CLI or an embedded host? Compare their commands and process-exit behavior.

**Code hint:** [QaaS.Runner: Bootstrap.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Bootstrap.cs); [QaaS.Runner: Runner.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Runner.cs); [QaaS.Mocker: Bootstrap.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/Bootstrap.cs).

**Graph hint:** [Roadmap stage 1](qaas-ecosystem/qaas-ecosystem.html#learn=orient). Compare [Runner Bootstrap](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Runner%7CQaaS.Runner%2FBootstrap.cs%23530&tab=behavior&stage=orient) and [Mocker Bootstrap](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Mocker%7CQaaS.Mocker%2FBootstrap.cs%23310&tab=behavior&stage=orient) in **Behavior & members**. Follow their entry-point relationships through **Uses / used by**, then check **Source & signature** around return paths and process termination.

## Q7. From YAML to runtime

Trace YAML into a Runner or Mocker execution. Where do cases, overlays, placeholders, references, and environment values enter?

**Code hint:** [QaaS.Framework: ContextBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/ContextObjects/ContextBuilder.cs); [QaaS.Runner: RunLoader.cs](https://github.com/TheSmokeTeam/QaaS.Runner/blob/0b184ee3c9f6111bc6aca070f0c6f9c9bfd8283f/QaaS.Runner/Loaders/RunLoader.cs); [QaaS.Mocker: ExecutionBuilder.cs](https://github.com/TheSmokeTeam/QaaS.Mocker/blob/c3aa787b2db6d4c796b0ec6625af16799150e5ec/QaaS.Mocker/ExecutionBuilder.cs).

**Graph hint:** [Roadmap stage 2](qaas-ecosystem/qaas-ecosystem.html#learn=configuration). Use the [Runner flow](qaas-ecosystem/qaas-ecosystem.html#view=runner) to place the YAML you already know at the start of a run. Open [ContextBuilder](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FContextObjects%2FContextBuilder.cs%23497&tab=behavior&stage=configuration) in **Behavior & members**, then follow its configuration relationships through **Uses / used by** and compare the Mocker path.

## Q8. Binding and validation

How does configuration become typed C# objects? Distinguish defaults, binding, conversion errors, unknown keys, and validation.

**Code hint:** [QaaS.Framework: ConfigurationUtils.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Configurations/ConfigurationUtils.cs); [QaaS.Framework: Bind.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/ContextObjects/Bind.cs); [QaaS.Framework: ValidationUtils.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Configurations/ValidationUtils.cs).

**Graph hint:** [Roadmap stage 2](qaas-ecosystem/qaas-ecosystem.html#learn=configuration). Open [Bind](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FContextObjects%2FBind.cs%23241&tab=behavior&stage=configuration) and [ConfigurationUtils](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Configurations%2FConfigurationUtils.cs%23646&tab=behavior&stage=configuration) in **Behavior & members**. Follow conversion and validation relationships through **Uses / used by**, then check **Source & signature** against one valid value, one unknown key, and one invalid value from a small configuration.

## Q9. Contexts and shared state

Compare QaaS's context types. What does each expose, and how do execution identity, probe identity, and shared state differ?

**Code hint:** [QaaS.Framework: Context.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/ContextObjects/Context.cs); [QaaS.Framework: InternalContext.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/ContextObjects/InternalContext.cs); [QaaS.Framework: ProbeExecutionContext.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.SDK/ContextObjects/ProbeExecutionContext.cs).

**Graph hint:** [Roadmap stage 2](qaas-ecosystem/qaas-ecosystem.html#learn=configuration). Start at [Context](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.SDK%2FContextObjects%2FContext.cs%23286&tab=overview&stage=configuration) and relate the visible members to those your hook used. Follow neighboring context types in **Uses / used by**, then compare **Source & signature** for identity, state, and visibility boundaries.

## Q10. Protocol contracts

Explain every interface in QaaS.Framework.Protocols. How do their capabilities and lifecycle contracts support Runner actions?

**Code hint:** [QaaS.Framework: IReader.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Protocols/Protocols/IReader.cs); [QaaS.Framework: ITransactor.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Protocols/Protocols/ITransactor.cs); [QaaS.Framework: IFetcher.cs](https://github.com/TheSmokeTeam/QaaS.Framework/blob/daa83a2b7ddcba5142760fc42833486219efd1ba/QaaS.Framework.Protocols/Protocols/IFetcher.cs).

**Graph hint:** [Roadmap stage 3](qaas-ecosystem/qaas-ecosystem.html#learn=contracts). Start at [IReader](qaas-ecosystem/qaas-ecosystem.html#entity=QaaS.Framework%7CQaaS.Framework.Protocols%2FProtocols%2FIReader.cs%23135&tab=overview&stage=contracts) and expand the Framework Protocols project in the explorer tree to locate its other interfaces. Compare **Source & signature**, then use **Uses / used by** to trace each capability to a concrete implementation and a Runner consumer.
