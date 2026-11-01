using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Extensions;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace Common.Shampoo;

public sealed class HttpSuccessConfiguration
{
    [Required] public string SessionName { get; set; } = string.Empty;
    [Required] public string OutputName { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int MinimumResponses { get; set; } = 1;
}

public sealed class HttpSuccess : BaseAssertion<HttpSuccessConfiguration>
{
    public override bool Assert(IImmutableList<SessionData> sessions,
        IImmutableList<DataSource> dataSources)
    {
        var output = sessions.GetSessionDataByName(Configuration.SessionName)
            .GetOutputByName(Configuration.OutputName);
        var unexpectedStatuses = 0;
        foreach (var item in output.Data)
        {
            var status = item.MetaData?.Http?.StatusCode
                ?? throw new InvalidOperationException(
                    "HTTP status metadata is missing; retain metadata for this output.");
            if (status < 200 || status > 299)
                unexpectedStatuses++;
        }

        var passed = output.Data.Count >= Configuration.MinimumResponses
            && unexpectedStatuses == 0;
        AssertionMessage = $"Received {output.Data.Count} responses; "
            + $"required at least {Configuration.MinimumResponses}; "
            + $"{unexpectedStatuses} had a non-2xx status.";
        return passed;
    }
}
