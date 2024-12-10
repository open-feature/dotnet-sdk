using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        featureBuilder.Should().BeSameAs(_systemUnderTest, "The method should return the same builder instance.");
        _systemUnderTest.IsContextConfigured.Should().BeTrue("The context should be configured.");
        _services.Should().ContainSingle(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(EvaluationContext) &&
            serviceDescriptor.Lifetime == ServiceLifetime.Transient,
            "A transient service of type EvaluationContext should be added.");
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
        _systemUnderTest.IsContextConfigured.Should().BeTrue("The context should be configured.");
        context.Should().NotBeNull("The EvaluationContext should be resolvable.");
        delegateCalled.Should().BeTrue("The delegate should be invoked.");
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
        _systemUnderTest.IsContextConfigured.Should().BeFalse("The context should not be configured.");
        _systemUnderTest.HasDefaultProvider.Should().Be(expectsDefaultProvider, "The default provider flag should be set correctly.");
        _systemUnderTest.IsPolicyConfigured.Should().BeFalse("The policy should not be configured.");
        _systemUnderTest.DomainBoundProviderRegistrationCount.Should().Be(expectsDomainBoundProvider, "The domain-bound provider count should be correct.");
        featureBuilder.Should().BeSameAs(_systemUnderTest, "The method should return the same builder instance.");
        _services.Should().ContainSingle(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(FeatureProvider) &&
            serviceDescriptor.Lifetime == ServiceLifetime.Transient,
            "A singleton service of type FeatureProvider should be added.");
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
        provider.Should().NotBeNull("The FeatureProvider should be resolvable.");
        provider.Should().BeOfType<NoOpFeatureProvider>("The resolved provider should be of type DefaultFeatureProvider.");
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
        _systemUnderTest.IsContextConfigured.Should().BeFalse("The context should not be configured.");
        _systemUnderTest.HasDefaultProvider.Should().Be(expectsDefaultProvider, "The default provider flag should be set correctly.");
        _systemUnderTest.IsPolicyConfigured.Should().BeFalse("The policy should not be configured.");
        _systemUnderTest.DomainBoundProviderRegistrationCount.Should().Be(expectsDomainBoundProvider, "The domain-bound provider count should be correct.");
        featureBuilder.Should().BeSameAs(_systemUnderTest, "The method should return the same builder instance.");
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
        featureBuilder.IsPolicyConfigured.Should().BeTrue("The policy should be configured.");
        provider.Should().NotBeNull("The FeatureProvider should be resolvable.");
        provider.Should().BeOfType<NoOpFeatureProvider>("The resolved provider should be of type DefaultFeatureProvider.");
    }
}
