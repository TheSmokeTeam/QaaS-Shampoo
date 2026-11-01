# Protocol extensions: from data to a completed operation

A protocol extension must fit both the transport contract and the host's construction rules. The proposed JSON-lines sender makes those responsibilities concrete: it captures outgoing JSON in a local file while preserving Runner's normal publishing lifecycle.

Read the complete implementation and integration snippets in [solution and project structure](../../03-architecture/solution-and-project-structure.md). This walkthrough explains the reasoning behind that code. `JsonLinesSender` and its configuration are proposed additions, not names of a shipped QaaS protocol.

## Place the behavior at the owning boundary

The sender owns the file operation; Runner owns selecting and executing it. This division lets the transport remain reusable without teaching it about sessions, assertions or the whole Runner host.

An ordinary generator produces input data, and an assertion evaluates evidence. Neither is the appropriate owner of an alternative delivery mechanism. Implementing the file write inside a generator would couple data creation to a side effect before the publisher owns the operation. Implementing it inside an assertion would delay delivery until evaluation and confuse expected behavior with observation. A sender implements the operation that publishing actually requires.

This is a compiled Framework extension. Merely placing a DLL beside Runner does not add a new configuration alternative to its publisher builder or a new arm to the Framework sender factory. The chapter links both owners so the change can be traced across them. The deployment implication is equally concrete: consumers need the Framework version containing the new sender and the Runner version containing its selection path.

## Keep representation separate from conversion

Declaring JSON as a protocol's serialization type does not turn an arbitrary object into JSON bytes. The configured serialization step and the sender's accepted input must agree.

The implementation accepts a byte array, parses it as JSON, and serializes the parsed document into one compact line. Parsing catches malformed JSON before appending a misleading record. Compact output ensures that pretty-printed input cannot accidentally become several physical records. This transformation is about file framing; it does not promise byte-for-byte preservation of whitespace in the file.

The publisher configuration therefore explicitly selects the JSON serializer. If a caller bypasses that pipeline and passes a record object directly, the sender rejects the input. Silent conversion inside the sender would hide a misconfigured pipeline and make serializer choices ambiguous. Runner publishes the serialized member of the data pair while retaining the original item for its evidence; the chapter's `Publisher.Publish` reference explains why a stored object and the physical file representation need not look identical.

## Give lifecycle methods a precise meaning

Connection state controls whether sends are allowed, even though this implementation does not keep an open network connection or file stream.

The constructor rejects a blank path so direct C# construction has a basic invariant even when configuration validation is bypassed. `Connect` creates the containing directory and enables sending. `Send` checks that state, validates the body, appends a record and returns evidence only after the write succeeds. `Disconnect` disables subsequent sends. These choices make misuse visible instead of producing an apparently successful empty operation.

Opening and closing the file for each send costs more than retaining a buffered writer, but it avoids leaving a stream alive when a later action fails. A retained stream would require additional ownership rules for flushing, partial initialization, concurrent disconnect and cleanup exceptions. Those are valid optimization concerns, but they need an explicit contract and regression cases before replacing the simpler lifecycle.

## State exactly what concurrency guarantees

The instance lock serializes operations on one sender object. It does not coordinate every writer that happens to use the same path.

Holding the same lock around state checks and writes prevents a disconnect from changing the sender's state halfway through one of its protected operations. It also keeps two calls through that instance from interleaving the implementation's record writes. Separate publisher instances must use separate files: their independent locks cannot establish a shared-file ordering guarantee, and another process is outside either lock entirely.

The timestamp is assigned after a successful append, and input metadata is preserved in the returned result. A filesystem exception propagates instead of producing a successful `DetailedData` record. None of this establishes crash-atomic writes, forced persistence to physical storage, quotas or log rotation. A production retention feature would need to specify ownership of rotation, behavior when a rename fails, and how readers distinguish completed records from a truncated last record.

## Follow configuration through every builder path

The new option must survive binding, selection, updates, cloning, validation and construction. A single happy-path factory call covers only part of that journey.

The property makes the proposed option representable in Runner's publisher configuration. `Configure` selects it from the typed configuration input. `GetConfiguration` allows later operations to retrieve the active alternative. `Reset` removes a previous choice so changing protocols does not leave two conflicting settings. Inclusion in `BuildWithTimeZone`'s alternatives allows the existing selection checks to see it. The chunk-support registry states that the proposed implementation supports single sends, avoiding an incompatible execution mode before the factory is reached.

The Framework factory then turns the configuration record into the sender. The chapter includes all these insertion points because a sender can compile and still be unreachable from YAML, or work once and fail after a builder update. The scalar configuration follows the existing clone path; clone-isolation verification checks that changing the copied configuration leaves the original intact. More complex mutable configuration would require renewed scrutiny of that assumption.

## Decide whether Mocker belongs in the change

A file sender has no listening endpoint, so this proposal requires no Mocker server implementation. Adding a receiving transport would be a different capability with its own lifecycle.

For a new listener, trace Mocker's server contract, configuration, server selection, factory, routing and startup/shutdown handling. A Runner sender cannot substitute for those responsibilities. Conversely, changing a response body or status on an existing HTTP server can belong in a processor hook, as the maintenance-response example in the chapter demonstrates. That distinction avoids expanding transport code for ordinary response behavior.

## Interpret the verification evidence

Verification should challenge each integration boundary, not merely demonstrate that one record can be written.

The implementation was compiled against the real patched Framework factory and Runner builder in a disposable validation workspace. The chapter records the executed checks and their limits. Useful review questions include what happens before connection, after disconnection, with malformed bytes, with conflicting protocol choices, and when a copied builder is changed. Filesystem access failures and multi-process writers require their own evidence before making broader claims. Return to the [main chapter](../../03-architecture/solution-and-project-structure.md) for the exact snippets and linked source, or to the [walkthrough index](../README.md) for another boundary.
