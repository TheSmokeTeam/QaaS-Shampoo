# JSON-lines sender laboratory

This is a **proposed teaching extension**, not an existing QaaS protocol. It adds a local, append-only JSON-lines sender to Framework and registers it in Runner. The executable checks the real modified factory and Runner builder, not substitute interfaces.

Prerequisites: Git, Python 3, .NET 10 SDK, NuGet access. Run from this folder. Choose a new laboratory directory outside your working repositories:

```powershell
$lab = Join-Path $env:TEMP ("qaas-protocol-lab-" + [guid]::NewGuid().ToString("N"))
git clone https://github.com/TheSmokeTeam/QaaS.Framework.git "$lab/QaaS.Framework"
git -C "$lab/QaaS.Framework" checkout --detach 11f3ec66126cf878dc34c4fab8c152ced521f3ea
git clone https://github.com/TheSmokeTeam/QaaS.Runner.git "$lab/QaaS.Runner"
git -C "$lab/QaaS.Runner" checkout --detach 7c3af33b093b500b0dc7fdcd2cffe2bb7f60c8ba
python prepare.py "$lab"
dotnet run --project ProtocolExample.csproj "--property:ExampleSources=$lab"
```

[prepare.py](prepare.py) rejects incorrect revisions and dirty clones before making edits. It changes the disposable copies only: adds [JsonLinesSender.cs](JsonLinesSender.cs), registers the factory, adds all Runner selection/reset/chunk paths, and replaces Runner.Infrastructure's Framework.Executions package reference with a source reference **for this laboratory only**. Run preparation once per fresh laboratory. A real release uses coordinated package versions; do not copy the laboratory project-reference substitution into a product release.

[Program.cs](Program.cs) verifies 19 behaviors: actual factory selection and chunk rejection; empty path; send-before-connect and send-after-disconnect; timestamp/body; malformed and wrong-type input; 25 concurrent sends plus one initial send; config selection, cloning, updates and reset in both directions; chunk validation; actual Publisher construction; conflicting configurations; and binding the internal-set configuration property. Successful output ends with `19 checks passed`.

Verified with .NET SDK 10.0.203 against the two pinned source revisions. The source build emitted pre-existing compiler and NuGet vulnerability warnings. A successful laboratory run does not resolve those upstream dependency warnings. This is not a full Runner session/E2E test or a package-consumer release test; no Mocker network server or external broker is involved.

The sender owns no persistent stream: each send opens and closes the file. Its per-instance lock prevents interleaving within one publisher. Different instances/executions must use different files. This intentionally omits rotation, quotas, cross-process locking, durable fsync, retry deduplication and crash-atomic records. It is suitable for a bounded local diagnostic fixture, not a claim of production audit durability.

Read the [project and extension guide](../../03-architecture/solution-and-project-structure.md) for the rationale and publication path.
