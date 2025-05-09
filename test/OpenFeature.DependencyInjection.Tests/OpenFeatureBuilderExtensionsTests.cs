using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenFeature.DependencyInjection.Internal;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.DependencyInjection.Tests;

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
        bool delegateCalled = false;

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
    public void AddHook_AddsHookAsKeyedService()
    {
        // Arrange
        _systemUnderTest.AddHook<NoOpHook>();

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var hook = serviceProvider.GetKeyedService<Hook>("NoOpHook");

        // Assert
        Assert.NotNull(hook);
    }

    [Fact]
    public void AddHook_AddsHookNameToOpenFeatureOptions()
    {
        // Arrange
        _systemUnderTest.AddHook(sp => new NoOpHook());

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<OpenFeatureOptions>>();

        // Assert
        Assert.Contains(options.Value.HookNames, t => t == "NoOpHook");
    }

    [Fact]
    public void AddHook_WithSpecifiedNameToOpenFeatureOptions()
    {
        // Arrange
        _systemUnderTest.AddHook<NoOpHook>("my-custom-name");

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var hook = serviceProvider.GetKeyedService<Hook>("my-custom-name");

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
        var hook = serviceProvider.GetKeyedService<Hook>("my-custom-name");

        // Assert
        Assert.NotNull(hook);
    }

    [Fact]
    public void AddHandler_AddsEventHandlerDelegateWrapperAsKeyedService()
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler("test", Constant.ProviderEventTypes.ProviderReady, eventHandler);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var handler = serviceProvider.GetKeyedService<EventHandlerDelegateWrapper>("test:ProviderReady");

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
        var handler = serviceProvider.GetKeyedServices<EventHandlerDelegateWrapper>("EventHandlerDelegate:ProviderReady");

        // Assert
        Assert.NotEmpty(handler);
        Assert.Equal(eventHandler1, handler.ElementAt(0).EventHandlerDelegate);
        Assert.Equal(eventHandler2, handler.ElementAt(1).EventHandlerDelegate);
    }

    [Fact]
    public void AddHandler_SetsHandlerNameInOpenFeatureOptions()
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler("test", Constant.ProviderEventTypes.ProviderReady, eventHandler);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetService<IOptions<OpenFeatureOptions>>();
        var openFeatureOptions = options!.Value;

        // Assert
        Assert.Contains("test:ProviderReady", openFeatureOptions.HandlerNames);
    }

    [Fact]
    public void AddHandler_WithoutName_AddsEventHandlerDelegateWrapperAsKeyedService()
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler(Constant.ProviderEventTypes.ProviderReady, eventHandler);

        // Act
        var handlers = _services.Where(s => s.ServiceType == typeof(EventHandlerDelegateWrapper)).ToList();
        var handler = handlers.First();

        // Assert
        Assert.True(handler.IsKeyedService);
        Assert.Equal(ServiceLifetime.Singleton, handler.Lifetime);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AddHandler_WithEmptyName_AddsEventHandlerDelegateWrapperAsKeyedService(string? handlerName)
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler(handlerName!, Constant.ProviderEventTypes.ProviderReady, eventHandler);

        // Act
        var handlers = _services.Where(s => s.ServiceType == typeof(EventHandlerDelegateWrapper)).ToList();
        var handler = handlers.First();

        // Assert
        Assert.True(handler.IsKeyedService);
        Assert.Equal(ServiceLifetime.Singleton, handler.Lifetime);
    }

    [Fact]
    public void AddHandler_WithImplementationFactory_AddsEventHandlerDelegateWrapperAsKeyedService()
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler("test", Constant.ProviderEventTypes.ProviderReady, sp => eventHandler);

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var handler = serviceProvider.GetKeyedService<EventHandlerDelegateWrapper>("test:ProviderReady");

        // Assert
        Assert.NotNull(handler);
        Assert.Equal(eventHandler, handler.EventHandlerDelegate);
    }

    [Fact]
    public void AddHandlerWithImplementationFactory_WithoutName_AddsEventHandlerDelegateWrapperAsKeyedService()
    {
        // Arrange
        EventHandlerDelegate eventHandler = (eventDetails) => { };
        _systemUnderTest.AddHandler(Constant.ProviderEventTypes.ProviderReady, sp => eventHandler);

        // Act
        var handlers = _services.Where(s => s.ServiceType == typeof(EventHandlerDelegateWrapper)).ToList();
        var handler = handlers.First();

        // Assert
        Assert.True(handler.IsKeyedService);
        Assert.Equal(ServiceLifetime.Singleton, handler.Lifetime);
    }
}
