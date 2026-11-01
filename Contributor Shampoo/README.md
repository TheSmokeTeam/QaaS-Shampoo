# QaaS Contributors Shampoo

Welcome to the QaaS contributor study pack. This is a practical route into contributing a small, reviewable change to the open-source ecosystem. It is designed for part-time, external, and community contributors who need to understand the boundaries around their change without becoming full-time maintainers of every repository.

Plan for **1 to 1.5 weeks, approximately 20–30 hours**. The standard route below takes **24 hours**. If you already know the prerequisite C# and design concepts, use a 20-hour route by shortening the standards reading by four hours while still completing its setup and exercises. Add up to six hours of optional practice for a 30-hour route. Learning C# from scratch is outside that estimate.

## Start here

You should already be comfortable reading C#, using Git, running a .NET project, and explaining a basic test's inputs and expected outputs. Prior experience using QaaS with YAML and a small hook is helpful. If you need customer-facing orientation first, read the [published QaaS documentation](https://docs.qaas.online/), then return here for implementation and contribution concerns.

Clone or download and extract the entire QaaS Shampoo repository, then open this folder's README in a Markdown viewer. All four local guide links work relative to this folder. Internet access is needed for the linked source files and learning resources. A viewer with Mermaid support displays the diagrams; surrounding prose describes their meaning if your viewer only displays the Mermaid source.

## Document index

The directory numbers identify modules; the recommended reading order starts with standards and project boundaries before the runtime deep dive.

| File | Purpose | Standard allocation |
|---|---|---:|
| [02-standards/conventions-and-guidelines.md](02-standards/conventions-and-guidelines.md) | Set up tools and the official developer plugin; learn coding, Git, design, DI, and testing expectations. | 6 hours |
| [03-architecture/solution-and-project-structure.md](03-architecture/solution-and-project-structure.md) | Find the owning repository/project and understand dependency and extension boundaries. | 4 hours |
| [01 — General flow](01-general-flow/runner-execution-flow.md) | Trace configuration, construction, runtime coordination, results, and cleanup through Runner, with worked hooks and failure-path examples. | 7 hours |
| [04-devops/cicd-and-package-publishing.md](04-devops/cicd-and-package-publishing.md) | Follow CI, local package validation, release effects, PackageMirror, and documentation synchronization. | 4 hours |
| Contributor rehearsal below | Combine the modules in one small change proposal and review. | 3 hours |
| **Total** | Reading, source tracing, exercises, and review notes are included. | **24 hours** |

```text
QaaS-Contributors-Shampoo/
├── README.md
├── 01-general-flow/
│   └── runner-execution-flow.md
├── 02-standards/
│   └── conventions-and-guidelines.md
├── 03-architecture/
│   └── solution-and-project-structure.md
├── 04-devops/
│   └── cicd-and-package-publishing.md
└── examples/
    ├── README.md
    ├── general-flow/
    ├── hooks-and-policies/
    └── protocol-extension/
```

## Code examples and extension exercises

Each guide now separates **existing source excerpts**, **new example implementations**, and **proposed integration changes**. Source links identify the exact revision; a proposed type or YAML key is never presented as an already shipped capability.

| Module | Useful feature to explore |
|---|---|
| [General flow](01-general-flow/runner-execution-flow.md) | Deterministic order generation, snapshot probes, explicit stage ordering, non-vacuous assertions, evidence summaries, and cleanup that retains multiple failures |
| [Standards](02-standards/conventions-and-guidelines.md) | A configurable HTTP-success assertion with boundary tests, reflection discovery and execution-local ownership |
| [Projects and extension boundaries](03-architecture/solution-and-project-structure.md) | A JSON-lines sender wired through Framework and Runner, a Mocker maintenance response with `Retry-After`, composed stop policies and explicit JSON serialization |
| [CI/CD](04-devops/cicd-and-package-publishing.md) | Package-only consumer smoke checks, explicit stable-tag policy, mirror selection and source-driven documentation |

The [reproducible example laboratories](examples/README.md) include the source and test projects: **18 general-flow checks, 35 hook/policy tests, and 19 protocol integration checks passed**. The protocol lab modifies disposable pinned source clones; it does not install a new feature into your existing QaaS repositories. The DevOps guide records its separate local package and tag-selection verification. Full deployment and unexecuted host-level scenarios remain explicitly identified in the relevant chapters. Choose one laboratory during the contributor rehearsal; running every lab in depth is optional practice beyond the standard schedule.

## Eight-day study schedule

Each day is a three-hour block; split it into shorter sessions if necessary. Keep a small notebook with source locations, open questions, and actual command output. Finishing a module means producing its exercise result, not merely opening every reference link.

| Day | Work | Evidence to keep |
|---|---|---|
| 1 | Standards: environment baseline, Git conventions, official Claude developer plugin setup. | SDK and build/test commands, baseline results, routed contribution idea. |
| 2 | Standards: selected patterns, DI, testing, C# and Markdown exercises. | Pattern sketches, one lifetime trace, three-case regression plan. |
| 3 | Project structure: solutions, project roles, actual references, extension ownership. | A dependency map for the area you might change. |
| 4 | Finish project structure (1 hour); start Runner initialization and configuration (2 hours). | Chosen owning project and trace from entry point to runtime construction. |
| 5 | Runner: action coordination, probes, Mocker control, data flow, assertions (3 hours). | Annotated runtime sequence with inputs, results, and failure paths. |
| 6 | Finish Runner cleanup and exercises (2 hours); begin CI overview (1 hour). | Ownership/failure table and the relevant CI job path. |
| 7 | DevOps: package testing, versioning, mirror, and documentation (3 hours). | Local package plan and downstream release/documentation map. |
| 8 | Contributor rehearsal (3 hours). | A small draft change or patch proposal with tests and a PR description. |

Optional days 9–10 add at most six hours: run a relevant integration test against local infrastructure, repeat package-consumer verification for a second consumer, or ask another contributor to review your failure-path explanation. These are optional extensions, not prerequisites for completing the 24-hour route.

## Contributor rehearsal

Choose one narrowly scoped task: clarify a misleading configuration error, add a deterministic hook behavior, fix a reproducible boundary case, or improve a documented example with a verification case. Prefer a task you can explain without changing several public contracts. If a code change needs more time than the rehearsal allows, produce a concrete implementation proposal with the failing case and exact target files; do not rush an unverified patch into a PR.

1. **Route it — 30 minutes.** Use the developer plugin and the project guide to identify the repository, owning project, public contract, and affected consumers. Explain why this belongs in a hook package or compiled core.
2. **Reproduce and change — 90 minutes.** Establish a relevant baseline, write or describe a meaningful regression case, and implement the smallest coherent change if feasible. Retain actual command outcomes. For documentation, verify the code and commands used by the example.
3. **Review — 60 minutes.** Check ownership, failure propagation, concurrency, compatibility, package effects, and docs. Draft the PR using the conventions guide. Record what remains unverified and what a maintainer should focus on.

Do not publish packages, push release tags, or trigger release workflows for this exercise. Local builds and package-consumer checks are sufficient. A proposal is useful only when another contributor can reproduce the problem and identify the intended fix from your evidence.

## Reading evidence correctly

The guides link source code to exact commits so an explanation can be checked even after the default branch changes. Those links are an evidence baseline, not a promise that a later checkout behaves identically. Before a real contribution, compare the cited code with your branch, inspect project files and workflows, and consult the [live docs](https://docs.qaas.online/).

Where a repository manual and executable code disagree, the guides identify the distinction. Similarly, a recommended contributor convention is different from a CI gate, a pattern analogy is different from a literal implementation, and a package reference is different from a project reference. Keeping those distinctions clear is part of contributing safely.

## Completion criteria

You have completed the pack when you can:

- Locate the correct project and explain one allowed dependency and one forbidden architectural direction.
- Trace a Runner execution from configuration through results and teardown, including at least one failure path.
- Explain the ownership and lifetime of a hook and the data it receives.
- Choose a unit, integration, or contract test that would detect your proposed regression.
- Identify the actual CI checks, package-consumer effects, and documentation steps for your change.
- Present a reviewable patch or implementation proposal with source references and honest validation results.

You do not need to memorize every API or implement every design pattern. The aim is to know where to look, how to verify what you find, and how to keep a contribution small enough to review accurately.
