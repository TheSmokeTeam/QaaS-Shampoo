using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using QaaS.Framework.Protocols.ConfigurationObjects;
using QaaS.Framework.Protocols.Protocols;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.Serialization;

namespace QaaS.Framework.Protocols.Examples;

public sealed record JsonLinesSenderConfig : ISenderConfig
{
    [Required, Description("Local file to append one compact JSON document per line.")]
    public string FilePath { get; set; } = "";
}

// Teaching extension, not a protocol shipped by QaaS.
// Use a different file per publisher instance/execution.
public sealed class JsonLinesSender : ISender
{
    private readonly string _path;
    private readonly object _gate = new();
    private bool _connected;

    public JsonLinesSender(JsonLinesSenderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrWhiteSpace(config.FilePath);
        _path = Path.GetFullPath(config.FilePath);
    }

    public SerializationType? GetSerializationType() => SerializationType.Json;

    public void Connect()
    {
        lock (_gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            _connected = true;
        }
    }

    public DetailedData<object> Send(Data<object> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        lock (_gate)
        {
            if (!_connected)
                throw new InvalidOperationException("Connect before sending.");
            if (data.Body is not byte[] bytes)
                throw new ArgumentException("Configure Serialize.Serializer: Json to produce bytes.", nameof(data));

            using var document = JsonDocument.Parse(bytes);
            var line = JsonSerializer.Serialize(document.RootElement);
            // Open/close on each send: no stream survives a failed action.
            File.AppendAllText(_path, line + "\n", new UTF8Encoding(false));
            return new DetailedData<object>
            {
                Body = bytes,
                MetaData = data.MetaData,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public void Disconnect()
    {
        lock (_gate)
            _connected = false;
    }
}
