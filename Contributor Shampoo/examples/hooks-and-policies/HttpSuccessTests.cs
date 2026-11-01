using System.Collections.Immutable;
using Autofac;
using Common.Shampoo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using QaaS.Framework.Providers.ObjectCreation;
using QaaS.Framework.SDK.ContextObjects;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.CommunicationDataObjects;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

public sealed class HttpSuccessTests
{
    private static HttpSuccess CreateHook() => new()
    {
        Context = new Context { Logger = NullLogger.Instance },
        Configuration = new HttpSuccessConfiguration
            { SessionName = "checkout", OutputName = "submit-order" }
    };

    private static IImmutableList<SessionData> Sessions(params int?[] statuses) =>
        ImmutableList.Create(new SessionData
        {
            Name = "checkout",
            Outputs = [new CommunicationData<object>
            {
                Name = "submit-order",
                Data = statuses.Select(status => new DetailedData<object>
                { MetaData = new MetaData { Http = new Http { StatusCode = status } } }).ToList()
            }]
        });

    [TestCase(199, false)]
    [TestCase(200, true)]
    [TestCase(201, true)]
    [TestCase(204, true)]
    [TestCase(299, true)]
    [TestCase(300, false)]
    public void Status_boundaries_are_explicit(int status, bool expected)
    {
        var hook = CreateHook();
        Assert.That(hook.Assert(Sessions(status), ImmutableList<DataSource>.Empty), Is.EqualTo(expected));
    }

    [Test] public void Empty_output_is_not_a_success() =>
        Assert.That(CreateHook().Assert(Sessions(), ImmutableList<DataSource>.Empty), Is.False);

    [Test] public void Configured_minimum_is_honored()
    {
        var hook = CreateHook();
        hook.Configuration.MinimumResponses = 2;
        Assert.That(hook.Assert(Sessions(200), ImmutableList<DataSource>.Empty), Is.False);
        Assert.That(hook.Assert(Sessions(200, 201), ImmutableList<DataSource>.Empty), Is.True);
    }

    [Test] public void Missing_metadata_is_a_broken_precondition() =>
        Assert.Throws<InvalidOperationException>(() => CreateHook().Assert(Sessions((int?)null), ImmutableList<DataSource>.Empty));

    [Test] public void Wrong_output_name_is_not_hidden()
    {
        var hook = CreateHook(); hook.Configuration.OutputName = "missing";
        Assert.Throws<ArgumentException>(() => hook.Assert(Sessions(200), ImmutableList<DataSource>.Empty));
    }

    private static IConfiguration Config(string minimum) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SessionName"] = "checkout", ["OutputName"] = "submit-order", ["MinimumResponses"] = minimum
        }).Build();

    [TestCase("0", false)] [TestCase("1", true)]
    public void Real_base_class_binds_and_validates(string value, bool valid)
    {
        var errors = CreateHook().LoadAndValidateConfiguration(Config(value));
        Assert.That(errors?.Count ?? 0, valid ? Is.EqualTo(0) : Is.GreaterThan(0));
    }

    [Test] public void Reflection_resolves_the_public_parameterless_hook()
    {
        var creator = new ByNameObjectCreator(NullLogger.Instance);
        var hook = creator.GetInstanceOfSubClassOfTByNameFromAssemblies<IAssertion>(
            "Common.Shampoo.HttpSuccess", [typeof(HttpSuccess).Assembly]);
        Assert.That(hook, Is.TypeOf<HttpSuccess>());
    }

    [Test] public void Repeated_calls_do_not_accumulate_failed_counts()
    {
        var hook = CreateHook();
        Assert.That(hook.Assert(Sessions(500), ImmutableList<DataSource>.Empty), Is.False);
        Assert.That(hook.Assert(Sessions(200), ImmutableList<DataSource>.Empty), Is.True);
        Assert.That(hook.AssertionStatus, Is.Null);
    }

    [Test] public void Context_aliases_share_only_the_registered_execution_instance()
    {
        using var root = new ContainerBuilder().Build();
        var first = new InternalContext { Logger = NullLogger.Instance };
        var second = new InternalContext { Logger = NullLogger.Instance };
        using var a = root.BeginLifetimeScope(builder => {
            builder.RegisterInstance(first).As<InternalContext>().SingleInstance();
            builder.RegisterInstance(first).As<Context>().SingleInstance();
        });
        using var b = root.BeginLifetimeScope(builder => {
            builder.RegisterInstance(second).As<InternalContext>().SingleInstance();
            builder.RegisterInstance(second).As<Context>().SingleInstance();
        });
        Assert.That(a.Resolve<Context>(), Is.SameAs(a.Resolve<InternalContext>()));
        Assert.That(a.Resolve<Context>(), Is.Not.SameAs(b.Resolve<Context>()));
    }
}
