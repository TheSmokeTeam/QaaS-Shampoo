# Delivery: following evidence across release boundaries

Delivery turns source changes into packages, mirrored inputs and published documentation. Each transition has a different owner and needs different evidence; a successful compilation cannot stand in for all of them.

The commands, workflow excerpts and implementation links are integrated into [CI/CD and package publishing](../../04-devops/cicd-and-package-publishing.md). This companion explains why those checks are shaped that way and how to diagnose gaps between them.

## Start with the artifact a consumer receives

A package-consumer check asks whether another application can use the built NuGet artifact. That is a different question from whether projects in the producer's solution compile together.

A project reference gives the compiler direct access to another project's outputs. It can conceal problems in package contents, dependency declarations or version selection. A consumer that instead restores a `PackageReference` must pass through the distribution boundary. If the package omits something necessary or declares the wrong dependency, that route is more likely to expose the discrepancy the eventual user would encounter.

The chapter's smoke check deliberately exercises a small real behavior after restoration. Merely confirming that a package archive exists would establish that packaging produced a file, not that the package resolves correctly or behaves as expected. Keep the behavioral assertion focused on the changed boundary: a small package check complements the owning project's tests; it need not reproduce every internal regression test in a second application.

## Isolate version and cache to establish provenance

A passing consumer is meaningful only when it consumed the artifact you intended to test. Unique versions and an isolated restore cache make that identity inspectable.

Reusing a version that already exists locally can let a previous package satisfy the restore. The executable may then pass against yesterday's build while the new artifact is broken. A unique temporary version reduces that ambiguity, and an isolated package cache prevents an unrelated cached copy from silently becoming the source of truth. The local feed and restore settings must be explicit enough to make the intended candidate discoverable.

After restore, inspect the resolved package identity and location rather than treating the command's exit code as sufficient provenance. Then execute the consumer's assertions. These steps support a narrow conclusion: this consumer restored this local artifact and observed the tested behavior. They do not establish that remote credentials, publication permissions or every downstream application's dependency graph are correct. Use a consistent build configuration for packing and consumption so configuration differences do not confound the result.

## Read release predicates as executable policy

A workflow label describes an intention; the actual condition determines which runs can publish. Inspect the predicate before using a tag as a supposedly harmless experiment.

The chapter compares existing tag handling with a proposed stricter stable-version rule. The proposal should be reviewed as a behavior change, because rejecting prerelease tags alters which artifacts are eligible. Test both accepted and rejected forms and check the resulting output variables, not just whether the shell script exits successfully. Quoting and workflow interpolation are also part of the implementation; a correct standalone expression can still be wired incorrectly into a job.

Local classifier tests are useful precisely because they can explore these cases without publishing a release. Their limit is equally important: they do not execute the whole hosted Actions environment or prove that all later jobs use the variable correctly. A reviewer must trace each consumer of the predicate and assess how it interacts with branch, event and credential conditions. The chapter keeps the proposed job distinct from existing repository behavior.

## Treat mirror selection as an independent decision

PackageMirror selects and processes distribution inputs. Changing its selection rules does not automatically change the application's own package references or erase previously retained inputs.

This distinction matters when proposing stable-only selection. A source repository may publish several kinds of tags, while the mirror applies its own acceptance logic to candidates. Rejecting a new prerelease candidate is not equivalent to purging prerelease packages already present in saved state. The fallback and retention paths must be inspected before describing that stronger guarantee.

Artifact availability introduces another failure mode. A suitable tag can exist while its corresponding build artifact is missing or expired. A successful tag-classification test cannot prove that a complete synchronization will obtain everything it needs. Useful integration scenarios include no acceptable new artifact, an expired selected artifact and retained saved state. Observe what the implementation does in each case before promising that the mirror always advances to a particular source revision.

## Preserve the relationship between packages and docs

Generated documentation must describe the selected inputs, rather than whichever source branch happens to be newest when generation runs.

The delivery chapter traces schema contracts, captured CLI snapshots and source XML documentation into generation. A method's XML summary and placement annotation help the renderer locate API documentation, but they do not change YAML validation. A configuration change also needs the relevant configuration descriptions, validation and regenerated schema inputs. This separation prevents a polished page from becoming evidence for behavior that the package does not accept.

Explicit roots and selected source references make that relationship inspectable. The generator revision is another input: changing the renderer can change output even when the package selection stays fixed. When debugging a surprising diff, compare both the data being documented and the tool interpreting it. Repair a generated page at its owning input where appropriate; otherwise a subsequent generation pass can overwrite the apparent fix.

## Distinguish checking, publishing and republishing

A drift check, a site build and a release upload establish different results. Follow the command flags and job conditions to see which operation actually occurred.

Generation in check mode can detect that committed output is stale without creating or merging a documentation PR. Strict site validation can detect navigation or rendering failures without deploying the result. Publication belongs to its own workflow conditions and credentials. The published reader-facing reference is [docs.qaas.online](https://docs.qaas.online/), while the chapter's workflow links provide implementation evidence for contributors.

Offline documentation adds a further ownership distinction: docs CI builds the ZIM artifact, and the mirror republishes the existing bytes. A missing or ambiguous ZIM input can therefore block a mirror release even when package validation passed. Diagnosing that failure as a NuGet problem would send the investigation to the wrong owner. Trace the missing artifact back to its producer before changing the consumer.

## Build a reviewer handoff from bounded claims

A useful handoff states what was executed, what passed and which boundary still needs verification. It does not stretch a local result into a claim about deployment.

The chapter records the successful local package consumer, tag-detector cases, YAML parsing and existing mirror classifier checks. It also states that no complete mirror sync, release publication or full documentation regeneration was performed for those proposals. Preserve those distinctions when adapting the examples: attach the package identity, relevant dependency evidence, behavioral result and expected generated diff to the contribution. Then identify the remaining integration operation by name. Return to the [delivery chapter](../../04-devops/cicd-and-package-publishing.md) for the exact implementation, or the [walkthrough index](../README.md) to explore runtime behavior.
