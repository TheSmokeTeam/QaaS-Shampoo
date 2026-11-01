using System.Collections.Immutable;
using Common.Shampoo;
using NUnit.Framework;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

public sealed class HttpSuccessBoundaryTests
{
    [TestCase(199, false)]
    [TestCase(200, true)]
    [TestCase(201, true)]
    [TestCase(204, true)]
    [TestCase(299, true)]
    [TestCase(300, false)]
    public void Status_boundaries_are_explicit(int status, bool expected)
    {
        var hook = new HttpSuccess
        {
            Configuration = new HttpSuccessConfiguration
                { SessionName = "checkout", OutputName = "submit-order" }
        };
        var sessions = ImmutableList.Create(new SessionData
        {
            Name = "checkout",
            Outputs = [new CommunicationData<object>
            {
                Name = "submit-order",
                Data = [new DetailedData<object>
                {
                    MetaData = new MetaData { Http = new Http { StatusCode = status } }
                }]
            }]
        });
        Assert.That(hook.Assert(sessions, ImmutableList<DataSource>.Empty),
            Is.EqualTo(expected));
    }
}