using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QaaS.Framework.Configurations;
using QaaS.Framework.Protocols.ConfigurationObjects.Socket;
using QaaS.Framework.Protocols.Examples;
using QaaS.Framework.Protocols.Protocols.Factories;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.Session;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;
using QaaS.Framework.Serialization;
using QaaS.Runner.Sessions.Actions;
using QaaS.Runner.Sessions.Actions.Publishers;
using QaaS.Runner.Sessions.Actions.Publishers.Builders;
using QaaS.Runner.Sessions.ConfigurationObjects;

var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    Console.WriteLine("PASS " + name);
    passed++;
}
void Throws<T>(System.Action action, string name) where T : Exception
{
    try { action(); } catch (T) { Check(true, name); return; }
    throw new Exception("Expected " + typeof(T).Name + ": " + name);
}

var directory = Path.Combine(Path.GetTempPath(), "qaas-jsonlines-" + Guid.NewGuid().ToString("N"));
var file = Path.Combine(directory, "events.ndjson");
var config = new JsonLinesSenderConfig { FilePath = file };
var (sender, chunk) = SenderFactory.CreateSender(false, config, NullLogger.Instance, new DataFilter());
Check(sender is JsonLinesSender && chunk is null, "factory selects singular sender");
Throws<InvalidOperationException>(() => SenderFactory.CreateSender(true, config, NullLogger.Instance, null), "factory rejects chunk mode");
Throws<ArgumentException>(() => new JsonLinesSender(new()), "empty path rejected");
var payload = new Data<object> { Body = Encoding.UTF8.GetBytes("{\n\"orderId\":42\n}") };
Throws<InvalidOperationException>(() => sender!.Send(payload), "send before connect rejected");
sender!.Connect();
var sent = sender.Send(payload);
Check(sent.Timestamp?.Kind == DateTimeKind.Utc && ReferenceEquals(sent.Body, payload.Body), "sent timestamp and body retained");
System.Threading.Tasks.Parallel.For(0, 25, i => sender.Send(new Data<object> { Body = JsonSerializer.SerializeToUtf8Bytes(new { id = i }) }));
Throws<ArgumentException>(() => sender.Send(new Data<object> { Body = "not bytes" }), "wrong body type rejected");
Throws<JsonException>(() => sender.Send(new Data<object> { Body = Encoding.UTF8.GetBytes("not JSON") }), "invalid JSON rejected");
sender.Disconnect();
sender.Disconnect();
Throws<InvalidOperationException>(() => sender.Send(payload), "send after disconnect rejected");
var lines = File.ReadAllLines(file);
Check(lines.Length == 26 && lines.All(line => JsonDocument.Parse(line).RootElement.ValueKind == JsonValueKind.Object), "parallel sends produce 26 complete JSON lines");

var builder = new PublisherBuilder().Named("audit").AddDataSource("orders").Configure(config)
    .WithSerializer(new SerializeConfig { Serializer = SerializationType.Json });
Check(builder.JsonLines == config, "Configure selects new config");
Check(ProtocolChunkSupport.ResolveSenderMode(config) == ProtocolChunkMode.SingleOnly, "chunk registry selects single mode");
var copy = builder.Clone();
copy.JsonLines!.FilePath = "copy.ndjson";
Check(builder.JsonLines!.FilePath == file, "clone isolates mutable config");
builder.UpdateConfiguration(new { FilePath = file + ".updated" });
Check(builder.JsonLines!.FilePath == file + ".updated", "object configuration update finds new type");
builder.Configure(new SocketSenderConfig());
Check(builder.JsonLines is null && builder.Socket is not null, "reconfiguration resets new field");
builder.Configure(config);
Check(builder.Socket is null && builder.JsonLines is not null, "reconfiguration resets previous protocol");
builder.WithChunks(new Chunks { ChunkSize = 2 });
Check(builder.Validate(new ValidationContext(builder)).Any(), "builder validation rejects chunked JsonLines");
builder.Chunk = null;
var failures = new List<ActionFailure>();
var action = builder.Build(new InternalContext { Logger = NullLogger.Instance }, failures, "session");
Check(action is Publisher && failures.Count == 0, "real Runner builder constructs publisher through patched factory");
builder.Socket = new SocketSenderConfig();
Check(builder.Build(new InternalContext { Logger = NullLogger.Instance }, failures, "session") is null && failures.Count == 1,
    "builder rejects conflicting protocol configurations");
var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Name"] = "bound", ["DataSourceNames:0"] = "orders",
    ["JsonLines:FilePath"] = file, ["Serialize:Serializer"] = "Json"
}).Build();
var bound = configuration.BindToObject<PublisherBuilder>(new BinderOptions { BindNonPublicProperties = true }, NullLogger.Instance);
Check(bound.JsonLines?.FilePath == file && bound.Serialize?.Serializer == SerializationType.Json, "configuration binding populates internal-set property");
File.Delete(file);
Directory.Delete(directory);
Console.WriteLine($"{passed} checks passed. No upstream source repository was changed.");
