using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
        var result = useServiceProviderDelegate ?
            _systemUnderTest.AddContext(_ => { }) :
            _systemUnderTest.AddContext((_, _) => { });

        // Assert
        result.Should().BeSameAs(_systemUnderTest, "The method should return the same builder instance.");
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
    [Fact]
    public void AddProvider_ShouldAddProviderToCollection()
    {
        // Act
        var result = _systemUnderTest.AddProvider<NoOpFeatureProviderFactory>();

        // Assert
        _systemUnderTest.IsContextConfigured.Should().BeFalse("The context should not be configured.");
        result.Should().BeSameAs(_systemUnderTest, "The method should return the same builder instance.");
        _services.Should().ContainSingle(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(FeatureProvider) &&
            serviceDescriptor.Lifetime == ServiceLifetime.Singleton,
            "A singleton service of type FeatureProvider should be added.");
    }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
    [Fact]
    public void AddProvider_ShouldResolveCorrectProvider()
    {
        // Arrange
        _systemUnderTest.AddProvider<NoOpFeatureProviderFactory>();

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetService<FeatureProvider>();

        // Assert
        _systemUnderTest.IsContextConfigured.Should().BeFalse("The context should not be configured.");
        provider.Should().NotBeNull("The FeatureProvider should be resolvable.");
        provider.Should().BeOfType<NoOpFeatureProvider>("The resolved provider should be of type DefaultFeatureProvider.");
    }
}
