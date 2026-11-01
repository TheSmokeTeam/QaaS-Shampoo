# Reproduce the examples

The examples use **real, pinned QaaS source**. Install Git and .NET 10; the protocol laboratory also needs Python 3. Keep these files inside the downloaded Shampoo directory structure. No credentials, package publication, or running broker is required. NuGet access is needed for dependencies.

From this `examples` directory, create fresh source clones outside your working repositories:

```powershell
$exampleSources = Join-Path $env:TEMP ("qaas-hook-examples-" + [guid]::NewGuid().ToString("N"))
git clone https://github.com/TheSmokeTeam/QaaS.Framework.git "$exampleSources/QaaS.Framework"
git -C "$exampleSources/QaaS.Framework" checkout --detach 11f3ec66126cf878dc34c4fab8c152ced521f3ea
git clone https://github.com/TheSmokeTeam/QaaS.Mocker.git "$exampleSources/QaaS.Mocker"
git -C "$exampleSources/QaaS.Mocker" checkout --detach 0407e6b038b3ada1f48ed837bc6fd7b3022e0299
dotnet run --project general-flow/FlowExamples.csproj "--property:ExampleSources=$exampleSources"
dotnet test hooks-and-policies/StandardsExample.csproj "--property:ExampleSources=$exampleSources"
```

Stop if a clone, checkout or build fails. Do not claim verification from a command you have not run. The property points to the **parent directory** of the source repositories, not an individual project.

| Laboratory | Behavior covered | Verified result |
|---|---|---|
| [General flow](general-flow/Program.cs) and [implementations](general-flow/Examples.cs) | Real generator discovery/context injection, binding, deterministic/concurrent enumeration, snapshot probe, completed-session assertion, payload-free summary, cleanup aggregation | 18 checks passed |
| [Hooks and policies](hooks-and-policies/StandardsExample.csproj) | HTTP-success assertion, real binding and reflection, context aliasing/isolation, maintenance processor, real stub serialization and HTTP response writer, policy composition, JSON codec | 35 NUnit cases passed |
| [New protocol](protocol-extension/README.md) | Modified Framework sender factory and real Runner builder integration, including chunk validation, cloning, update/reset and parallel file writes | 19 checks passed |

The hooks project links the pinned Mocker `TransactionStub`, HTTP extension and HTTP-method sources and supplies the ASP.NET framework reference; it does not fake those implementations. Its writer test uses `DefaultHttpContext`, not a listening network server. The protocol laboratory has separate preparation commands because it intentionally modifies **disposable clones** to demonstrate the new protocol registration.

These results were obtained with SDK 10.0.203. The restored upstream dependency graph produced existing NuGet advisory warnings; those warnings are not fixed by the examples. Focused passing tests do not establish whole-solution correctness, live Runner/Mocker deployment, package release compatibility, or production readiness. Each chapter states which host-level checks remain necessary.

The [DevOps guide](../04-devops/cicd-and-package-publishing.md) additionally includes the complete locally executed package-consumer smoke script, tested release-tag selection, and the focused PackageMirror command. Those examples do not deploy GitHub Actions or publish packages.

Return to the [study route](../README.md).
