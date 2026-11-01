using System.Collections.Immutable;
using Common.Shampoo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using QaaS.Framework.Policies;
using QaaS.Framework.Policies.ConfigurationObjects;
using QaaS.Framework.Providers.ObjectCreation;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Processor;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.Serialization;
using QaaS.Mocker.Stubs.Stubs;

public sealed class ProcessorPolicyTests
{
    [Test] public async Task Actual_http_writer_applies_status_retry_header_and_json_bytes()
    {
        var stub = new TransactionStub
        {
            Name = "maintenance", Processor = Processor(), DataSourceList = ImmutableList<DataSource>.Empty,
            ResponseBodySerializer = SerializerFactory.BuildSerializer(SerializationType.Json)
        };
        var response = stub.Exercise(new Data<object>());
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        using var body = new MemoryStream();
        context.Response.Body = body;
        await QaaS.Mocker.Servers.Extensions.HttpExtensions.HandleResponseDataAndCloseAsync(
            context.Response, response, QaaS.Mocker.Servers.ConfigurationObjects.HttpServerConfigs.HttpMethod.Get);
        Assert.That(context.Response.StatusCode, Is.EqualTo(503));
        Assert.That(context.Response.Headers["Retry-After"].ToString(), Is.EqualTo("30"));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/json; charset=utf-8"));
        Assert.That(body.ToArray(), Is.EqualTo((byte[])response.Body!));
    }

    private static MaintenanceResponse Processor() => new()
    {
        Configuration = new MaintenanceConfiguration(),
        Context = new Context { Logger = NullLogger.Instance }
    };

    [Test] public void Maintenance_returns_status_headers_and_serializable_payload()
    {
        var result = Processor().Process(ImmutableList<DataSource>.Empty, new Data<object>());
        Assert.That(result.MetaData?.Http?.StatusCode, Is.EqualTo(503));
        Assert.That(result.MetaData?.Http?.ResponseHeaders?["Retry-After"], Is.EqualTo("30"));
        Assert.That(result.Body, Is.EqualTo(new MaintenancePayload("maintenance", "Planned maintenance")));
    }

    [Test] public void Processor_does_not_share_response_headers_between_requests()
    {
        var p = Processor();
        var one = p.Process(ImmutableList<DataSource>.Empty, new Data<object>());
        var two = p.Process(ImmutableList<DataSource>.Empty, new Data<object>());
        one.MetaData!.Http!.ResponseHeaders!["Retry-After"] = "bad";
        Assert.That(two.MetaData!.Http!.ResponseHeaders!["Retry-After"], Is.EqualTo("30"));
    }

    [TestCase(0, false)] [TestCase(1, true)] [TestCase(86400, true)] [TestCase(86401, false)]
    public void Retry_delay_configuration_is_validated(int seconds, bool valid)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        { ["RetryAfterSeconds"] = seconds.ToString() }).Build();
        var results = Processor().LoadAndValidateConfiguration(configuration);
        Assert.That(results?.Count ?? 0, valid ? Is.EqualTo(0) : Is.GreaterThan(0));
    }

    [Test] public void Processor_is_discoverable_by_its_full_name()
    {
        var instance = new ByNameObjectCreator(NullLogger.Instance)
            .GetInstanceOfSubClassOfTByNameFromAssemblies<ITransactionProcessor>(
                "Common.Shampoo.MaintenanceResponse", [typeof(MaintenanceResponse).Assembly]);
        Assert.That(instance, Is.TypeOf<MaintenanceResponse>());
    }

    [Test] public void Actual_stub_source_serializes_the_response_and_preserves_metadata()
    {
        var stub = new TransactionStub
        {
            Name = "maintenance", Processor = Processor(), DataSourceList = ImmutableList<DataSource>.Empty,
            ResponseBodySerializer = SerializerFactory.BuildSerializer(SerializationType.Json)
        };
        var response = stub.Exercise(new Data<object>());
        Assert.That(response.Body, Is.TypeOf<byte[]>());
        var decoded = (MaintenancePayload?)DeserializerFactory.BuildDeserializer(SerializationType.Json)!
            .Deserialize((byte[])response.Body!, typeof(MaintenancePayload));
        Assert.That(decoded?.Code, Is.EqualTo("maintenance"));
        Assert.That(response.MetaData?.Http?.StatusCode, Is.EqualTo(503));
    }

    [Test] public void Actual_stub_source_rejects_unserialized_object()
    {
        var stub = new TransactionStub { Name = "maintenance", Processor = Processor(),
            DataSourceList = ImmutableList<DataSource>.Empty };
        Assert.Throws<ArgumentException>(() => stub.Exercise(new Data<object>()));
    }

    [Test] public void Separate_policy_builders_retain_count_and_timeout()
    {
        var chain = PolicyBuilder.BuildPolicies([
            new PolicyBuilder().Configure(new CountPolicyConfig { Count = 3 }),
            new PolicyBuilder().Configure(new TimeoutPolicyConfig { TimeoutMs = uint.MaxValue })])!;
        chain.SetupChain();
        Assert.That(chain.RunChain(), Is.True);
        Assert.That(chain.RunChain(), Is.True);
        Assert.That(chain.RunChain(), Is.False);
    }

    [Test] public void Configuring_one_builder_twice_replaces_the_first_policy()
    {
        var builder = new PolicyBuilder().Configure(new CountPolicyConfig { Count = 3 })
            .Configure(new TimeoutPolicyConfig { TimeoutMs = 0 });
        Assert.That(builder.Count, Is.Null);
        Assert.That(builder.Build(), Is.TypeOf<TimeoutPolicy>());
    }

    [Test] public void Zero_timeout_stops_without_sleeping()
    {
        var chain = PolicyBuilder.BuildPolicies([
            new PolicyBuilder().Configure(new CountPolicyConfig { Count = 3 }),
            new PolicyBuilder().Configure(new TimeoutPolicyConfig { TimeoutMs = 0 })])!;
        chain.SetupChain();
        Assert.That(chain.RunChain(), Is.False);
    }

    [Test] public void Json_roundtrip_explicitly_preserves_the_payload_type()
    {
        var serializer = SerializerFactory.BuildSerializer(SerializationType.Json)!;
        var deserializer = DeserializerFactory.BuildDeserializer(SerializationType.Json)!;
        var original = new MaintenancePayload("maintenance", "Try later");
        var bytes = serializer.Serialize(original)!;
        var restored = (MaintenancePayload?)deserializer.Deserialize(bytes, typeof(MaintenancePayload));
        Assert.That(restored, Is.EqualTo(original));
        Assert.That(deserializer.Deserialize(bytes, null), Is.InstanceOf<System.Text.Json.Nodes.JsonNode>());
        Assert.That(SerializerFactory.BuildSerializer(null), Is.Null);
        Assert.That(DeserializerFactory.BuildDeserializer(null), Is.Null);
        Assert.That(serializer.Serialize(null), Is.Null);
        Assert.That(deserializer.Deserialize(null, typeof(MaintenancePayload)), Is.Null);
    }
}
