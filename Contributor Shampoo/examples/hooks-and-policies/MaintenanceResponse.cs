using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Processor;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;

namespace Common.Shampoo;

public sealed class MaintenanceConfiguration
{
    [Range(1, 86400)] public int RetryAfterSeconds { get; set; } = 30;
    [Required] public string Message { get; set; } = "Planned maintenance";
}

public sealed record MaintenancePayload(string Code, string Message);

public sealed class MaintenanceResponse : BaseTransactionProcessor<MaintenanceConfiguration>
{
    public override Data<object> Process(IImmutableList<DataSource> dataSources,
        Data<object> requestData) => new()
    {
        Body = new MaintenancePayload("maintenance", Configuration.Message),
        MetaData = new MetaData
        {
            Http = new Http
            {
                StatusCode = 503,
                ResponseHeaders = new Dictionary<string, string>
                {
                    ["Retry-After"] = Configuration.RetryAfterSeconds.ToString(CultureInfo.InvariantCulture),
                    ["Content-Type"] = "application/json; charset=utf-8"
                }
            }
        }
    };
}
