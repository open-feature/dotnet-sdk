using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenFeature.Hosting.Internal;
using OpenFeature.Model;

namespace OpenFeature.Hosting.Tests;

public partial class OpenFeatureBuilderExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly OpenFeatureBuilder _systemUnderTest;

    public OpenFeatureBuilderExtensionsTests()
    {
        _services = new ServiceCollection();
        _systemUnderTest = new OpenFeatureBuilder(_services);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddContext_Delegate_ShouldAddServiceToCollection(bool useServiceProviderDelegate)
    {
        // Act
        var featureBuilder = useServiceProviderDelegate ?
            _systemUnderTest.AddContext(_ => { }) :
            _systemUnderTest.AddContext((_, _) => { });

        // Assert
        Assert.Equal(_systemUnderTest, featureBuilder);
        Assert.True(_systemUnderTest.IsContextConfigured, "The context should be configured.");
        Assert.Single(_services, serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(EvaluationContext) &&
            serviceDescriptor.Lifetime == ServiceLifetime.Transient);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddContext_Delegate_ShouldCorrectlyHandles(bool useServiceProviderDelegate)
    {
        // Arrange
        var delegateCalled = false;

        _ = useServiceProviderDelegate ?
            _systemUnderTest.AddContext(_ => delegateCalled = true) :
            _systemUnderTest.AddContext((_, _) => delegateCalled = true);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var context = serviceProvider.GetService<EvaluationContext>();

        // Assert
        Assert.True(_systemUnderTest.IsContextConfigured, "The context should be configured.");
        Assert.NotNull(context);
        Assert.True(delegateCalled, "The delegate should be invoked.");
    }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
    [Theory]
    [InlineData(1, true, 0)]
    [InlineData(2, false, 1)]
    [InlineData(3, true, 0)]
    [InlineData(4, false, 1)]
    public void AddProvider_ShouldAddProviderToCollection(int providerRegistrationType, bool expectsDefaultProvider, int expectsDomainBoundProvider)
    {
        // Act
        var featureBuilder = providerRegistrationType switch
        {
            1 => _systemUnderTest.AddProvider(_ => new NoOpFeatureProvider()),
            2 => _systemUnderTest.AddProvider("test", (_, _) => new NoOpFeatureProvider()),
            3 => _systemUnderTest.AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { }),
            4 => _systemUnderTest.AddProvider<TestOptions>("test", (_, _) => new NoOpFeatureProvider(), o => { }),
            _ => throw new InvalidOperationException("Invalid mode.")
        };

        // Assert
        Assert.False(_systemUnderTest.IsContextConfigured, "The context should not be configured.");
        Assert.Equal(expectsDefaultProvider, _systemUnderTest.HasDefaultProvider);
        Assert.False(_systemUnderTest.IsPolicyConfigured, "The policy should not be configured.");
        Assert.Equal(expectsDomainBoundProvider, _systemUnderTest.DomainBoundProviderRegistrationCount);
        Assert.Equal(_systemUnderTest, featureBuilder);
        Assert.Single(_services, serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(FeatureProvider) &&
            serviceDescriptor.Lifetime == ServiceLifetime.Transient);
    }

    class TestOptions : OpenFeatureOptions { }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void AddProvider_ShouldResolveCorrectProvider(int providerRegistrationType)
    {
        // Arrange
        _ = providerRegistrationType switch
        {
            1 => _systemUnderTest.AddProvider(_ => new NoOpFeatureProvider()),
            2 => _systemUnderTest.AddProvider("test", (_, _) => new NoOpFeatureProvider()),
            3 => _systemUnderTest.AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { }),
            4 => _systemUnderTest.AddProvider<TestOptions>("test", (_, _) => new NoOpFeatureProvider(), o => { }),
            _ => throw new InvalidOperationException("Invalid mode.")
        };

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var provider = providerRegistrationType switch
        {
            1 or 3 => serviceProvider.GetService<FeatureProvider>(),
            2 or 4 => serviceProvider.GetKeyedService<FeatureProvider>("test"),
            _ => throw new InvalidOperationException("Invalid mode.")
        };

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<NoOpFeatureProvider>(provider);
    }

    [Theory]
    [InlineData(1, true, 1)]
    [InlineData(2, true, 1)]
    [InlineData(3, false, 2)]
    [InlineData(4, true, 1)]
    [InlineData(5, true, 1)]
    [InlineData(6, false, 2)]
    [InlineData(7, true, 2)]
    [InlineData(8, true, 2)]
    public void AddProvider_VerifiesDefaultAndDomainBoundProvidersBasedOnConfiguration(int providerRegistrationType, bool expectsDefaultProvider, int expectsDomainBoundProvider)
    {
        // Act
        var featureBuilder = providerRegistrationType switch
        {
            1 => _systemUnderTest
                    .AddProvider(_ => new NoOpFeatureProvider())
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider()),
            2 => _systemUnderTest
                    .AddProvider(_ => new NoOpFeatureProvider())
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider()),
            3 => _systemUnderTest
                    .AddProvider("test1", (_, _) => new NoOpFeatureProvider())
                    .AddProvider("test2", (_, _) => new NoOpFeatureProvider()),
            4 => _systemUnderTest
                    .AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { })
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider()),
            5 => _systemUnderTest
                    .AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { })
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider()),
            6 => _systemUnderTest
                    .AddProvider<TestOptions>("test1", (_, _) => new NoOpFeatureProvider(), o => { })
                    .AddProvider("test2", (_, _) => new NoOpFeatureProvider()),
            7 => _systemUnderTest
                    .AddProvider(_ => new NoOpFeatureProvider())
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider())
                    .AddProvider("test2", (_, _) => new NoOpFeatureProvider()),
            8 => _systemUnderTest
                    .AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { })
                    .AddProvider<TestOptions>("test", (_, _) => new NoOpFeatureProvider(), o => { })
                    .AddProvider<TestOptions>("test2", (_, _) => new NoOpFeatureProvider(), o => { }),
            _ => throw new InvalidOperationException("Invalid mode.")
        };

        // Assert
        Assert.False(_systemUnderTest.IsContextConfigured, "The context should not be configured.");
        Assert.Equal(expectsDefaultProvider, _systemUnderTest.HasDefaultProvider);
        Assert.False(_systemUnderTest.IsPolicyConfigured, "The policy should not be configured.");
        Assert.Equal(expectsDomainBoundProvider, _systemUnderTest.DomainBoundProviderRegistrationCount);
        Assert.Equal(_systemUnderTest, featureBuilder);
    }

    [Theory]
    [InlineData(1, null)]
    [InlineData(2, "test")]
    [InlineData(3, "test2")]
    [InlineData(4, "test")]
    [InlineData(5, null)]
    [InlineData(6, "test1")]
    [InlineData(7, "test2")]
    [InlineData(8, null)]
    public void AddProvider_ConfiguresPolicyNameAcrossMultipleProviderSetups(int providerRegistrationType, string? policyName)
    {
        // Arrange
        var featureBuilder = providerRegistrationType switch
        {
            1 => _systemUnderTest
                    .AddProvider(_ => new NoOpFeatureProvider())
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider())
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            2 => _systemUnderTest
                    .AddProvider(_ => new NoOpFeatureProvider())
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider())
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            3 => _systemUnderTest
                    .AddProvider("test1", (_, _) => new NoOpFeatureProvider())
                    .AddProvider("test2", (_, _) => new NoOpFeatureProvider())
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            4 => _systemUnderTest
                    .AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { })
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider())
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            5 => _systemUnderTest
                    .AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { })
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider())
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            6 => _systemUnderTest
                    .AddProvider<TestOptions>("test1", (_, _) => new NoOpFeatureProvider(), o => { })
                    .AddProvider("test2", (_, _) => new NoOpFeatureProvider())
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            7 => _systemUnderTest
                    .AddProvider(_ => new NoOpFeatureProvider())
                    .AddProvider("test", (_, _) => new NoOpFeatureProvider())
                    .AddProvider("test2", (_, _) => new NoOpFeatureProvider())
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            8 => _systemUnderTest
                    .AddProvider<TestOptions>(_ => new NoOpFeatureProvider(), o => { })
                    .AddProvider<TestOptions>("test", (_, _) => new NoOpFeatureProvider(), o => { })
                    .AddProvider<TestOptions>("test2", (_, _) => new NoOpFeatureProvider(), o => { })
                    .AddPolicyName(policy => policy.DefaultNameSelector = provider => policyName),
            _ => throw new InvalidOperationException("Invalid mode.")
        };

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var policy = serviceProvider.GetRequiredService<IOptions<PolicyNameOptions>>().Value;
        var name = policy.DefaultNameSelector(serviceProvider);
        var provider = name == null ?
            serviceProvider.GetService<FeatureProvider>() :
            serviceProvider.GetRequiredKeyedService<FeatureProvider>(name);

        // Assert
        Assert.True(featureBuilder.IsPolicyConfigured, "The policy should be configured.");
        Assert.NotNull(provider);
        Assert.IsType<NoOpFeatureProvider>(provider);
    }

    [Fact]
    public void AddProvider_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange & Act
        _systemUnderTest.AddProvider(null!, (sp, domain) => new NoOpFeatureProvider());

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var ex = Assert.Throws<ArgumentNullException>(() => serviceProvider.GetKeyedService<FeatureProvider>(null));

        Assert.Equal("key", ex.ParamName);
    }

    [Fact]
    public void AddHook_AddsHookAsSingletonService()
    {
        // Arrange
        _systemUnderTest.AddHook<NoOpHook>();

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var hook = serviceProvider.GetRequiredService<Hook>();

        // Assert
        Assert.NotNull(hook);
    }

    [Fact]
    public void AddHook_WithSpecifiedNameAndImplementationFactory_AsKeyedService()
    {
        // Arrange
        _systemUnderTest.AddHook("my-custom-name", (serviceProvider) => new NoOpHook());

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var hook = serviceProvider.GetRequiredService<Hook>();

        // Assert
        Assert.NotNull(hook);
    }

    [Fact]
    public void AddHook_WithInstance_AddsHookAsSingletonService()
    {
        // Arrange
        var expectedHook = new NoOpHook();
        _systemUnderTest.AddHook(expectedHook);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var actualHook = serviceProvider.GetRequiredService<Hook>();

        // Assert
        Assert.NotNull(actualHook);
        Assert.Equal(expectedHook, actualHook);
    }

    [Fact]
    public void AddHook_WithSpecifiedNameAndInstance_AddsHookAsSingletonService()
    {
        // Arrange
        var expectedHook = new NoOpHook();
        _systemUnderTest.AddHook("custom-hook", expectedHook);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var actualHook = serviceProvider.GetRequiredService<Hook>();

        // Assert
        Assert.NotNull(actualHook);
        Assert.Equal(expectedHook, actualHook);
    }

    [Fact]
    public void AddHandler_AddsEventHandlerDelegateWrapperAsKeyedService()
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler(Constant.ProviderEventTypes.ProviderReady, eventHandler);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var handler = serviceProvider.GetService<EventHandlerDelegateWrapper>();

        // Assert
        Assert.NotNull(handler);
        Assert.Equal(eventHandler, handler.EventHandlerDelegate);
    }

    [Fact]
    public void AddHandlerTwice_MultipleEventHandlerDelegateWrappersAsKeyedServices()
    {
        // Arrange
        EventHandlerDelegate eventHandler1 = (eventDetails) => { };
        EventHandlerDelegate eventHandler2 = (eventDetails) => { };
        _systemUnderTest.AddHandler(Constant.ProviderEventTypes.ProviderReady, eventHandler1);
        _systemUnderTest.AddHandler(Constant.ProviderEventTypes.ProviderReady, eventHandler2);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var handler = serviceProvider.GetServices<EventHandlerDelegateWrapper>();

        // Assert
        Assert.NotEmpty(handler);
        Assert.Equal(eventHandler1, handler.ElementAt(0).EventHandlerDelegate);
        Assert.Equal(eventHandler2, handler.ElementAt(1).EventHandlerDelegate);
    }

    [Fact]
    public void AddHandler_WithImplementationFactory_AddsEventHandlerDelegateWrapperAsKeyedService()
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler(Constant.ProviderEventTypes.ProviderReady, _ => eventHandler);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var handler = serviceProvider.GetService<EventHandlerDelegateWrapper>();

        // Assert
        Assert.NotNull(handler);
        Assert.Equal(eventHandler, handler.EventHandlerDelegate);
    }

    [Fact]
    public void AddClient_AddsFeatureClient()
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        // Act
        _systemUnderTest.AddClient();

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var client = serviceProvider.GetService<IFeatureClient>();

        Assert.NotNull(client);
    }

    [Fact]
    public void AddClient_WithContext_AddsFeatureClient()
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        _systemUnderTest
            .AddContext((a) => a.Set("region", "euw"))
            .AddProvider(_systemUnderTest => new NoOpFeatureProvider());

        // Act
        _systemUnderTest.AddClient();

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var client = serviceProvider.GetService<IFeatureClient>();

        Assert.NotNull(client);

        var context = client.GetContext();
        var region = context.GetValue("region");
        Assert.Equal("euw", region.AsString);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddClient_WithInvalidName_AddsFeatureClient(string? name)
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        // Act
        _systemUnderTest.AddClient(name);

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var client = serviceProvider.GetService<IFeatureClient>();
        Assert.NotNull(client);

        var keyedClients = serviceProvider.GetKeyedServices<IFeatureClient>(name);
        Assert.Empty(keyedClients);
    }

    [Fact]
    public void AddClient_WithNullName_AddsFeatureClient()
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        // Act
        _systemUnderTest.AddClient(null);

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var client = serviceProvider.GetService<IFeatureClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddClient_WithName_AddsFeatureClient()
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        // Act
        _systemUnderTest.AddClient("client-name");

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var client = serviceProvider.GetKeyedService<IFeatureClient>("client-name");

        Assert.NotNull(client);
    }

    [Fact]
    public void AddClient_WithNameAndContext_AddsFeatureClient()
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        _systemUnderTest
            .AddContext((a) => a.Set("region", "euw"))
            .AddProvider(_systemUnderTest => new NoOpFeatureProvider());

        // Act
        _systemUnderTest.AddClient("client-name");

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var client = serviceProvider.GetKeyedService<IFeatureClient>("client-name");

        Assert.NotNull(client);

        var context = client.GetContext();
        var region = context.GetValue("region");
        Assert.Equal("euw", region.AsString);
    }

    [Fact]
    public void AddPolicyBasedClient_AddsScopedFeatureClient()
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        _services.AddOptions<PolicyNameOptions>()
            .Configure(options => options.DefaultNameSelector = _ => "default-name");

        _systemUnderTest.AddProvider("default-name", (_, key) => new NoOpFeatureProvider());

        // Act
        _systemUnderTest.AddPolicyBasedClient();

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetService<IFeatureClient>();
        Assert.NotNull(client);
    }

    [Fact(Skip = "Bug due to https://github.com/open-feature/dotnet-sdk/issues/543")]
    public void AddPolicyBasedClient_WithNoDefaultName_AddsScopedFeatureClient()
    {
        // Arrange
        _services.AddSingleton(sp => Api.Instance);

        _services.AddOptions<PolicyNameOptions>()
            .Configure(options => options.DefaultNameSelector = sp => null);

        _systemUnderTest.AddProvider("default", (_, key) => new NoOpFeatureProvider());

        // Act
        _systemUnderTest.AddPolicyBasedClient();

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetService<IFeatureClient>();
        Assert.NotNull(client);
    }
}
