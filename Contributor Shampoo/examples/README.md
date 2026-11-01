# In-depth explanations

These walkthroughs explain why the examples in the main chapters work, where their guarantees end, and how to reason about extending them. Each category starts with the central idea, then traces the decisions and failure paths in detail.

## Choose a walkthrough

Follow the topic you are currently studying. The implementation snippets remain beside their explanations in the main chapters; these pages provide a slower investigation of the same behavior.

| Walkthrough | What it explains | Main chapter |
|---|---|---|
| [General flow](general-flow/README.md) | Configuration, generated data, ordering, assertions and cleanup as one execution story | [Runner execution flow](../01-general-flow/runner-execution-flow.md) |
| [Hooks and policies](hooks-and-policies/README.md) | Discovery, configurable assertions, response processing and stop conditions | [Conventions and guidelines](../02-standards/conventions-and-guidelines.md) |
| [Protocol extensions](protocol-extension/README.md) | The complete ownership and lifecycle reasoning behind a JSON-lines sender | [Solution and project structure](../03-architecture/solution-and-project-structure.md) |
| [Delivery](delivery/README.md) | Package-consumer evidence, release selection, mirror inputs and generated documentation | [CI/CD and package publishing](../04-devops/cicd-and-package-publishing.md) |

## Read from behavior to evidence

Begin with the main chapter's behavior and inline code, then use its companion to examine the tradeoffs. Finish by returning to the linked implementation and checking which assertion or failure case supports each guarantee.

The folder contains explanatory Markdown only. Proposed features are identified in the main chapters, alongside the current source they would extend; the walkthroughs do not imply that those features are already shipped. Return to the [study plan](../README.md) to continue the full sequence.
