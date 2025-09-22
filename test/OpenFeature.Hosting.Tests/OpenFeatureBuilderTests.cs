using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature.Hosting.Tests;

public class OpenFeatureBuilderTests
{
    [Fact]
    public void Validate_DoesNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        var ex = Record.Exception(builder.Validate);

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_WithPolicySet_DoesNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
        {
            IsPolicyConfigured = true
        };

        // Act
        var ex = Record.Exception(builder.Validate);

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_WithMultipleDomainProvidersRegistered_ThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
        {
            IsPolicyConfigured = false,
            DomainBoundProviderRegistrationCount = 2
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(builder.Validate);

        // Assert
        Assert.Equal("Multiple providers have been registered, but no policy has been configured.", ex.Message);
    }

    [Fact]
    public void Validate_WithDefaultAndDomainProvidersRegistered_ThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
        {
            IsPolicyConfigured = false,
            DomainBoundProviderRegistrationCount = 1,
            HasDefaultProvider = true
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(builder.Validate);

        // Assert
        Assert.Equal("A default provider and an additional provider have been registered without a policy configuration.", ex.Message);
    }

    [Fact]
    public void Validate_WithNoDefaultProviderRegistered_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
        {
            IsPolicyConfigured = false,
            DomainBoundProviderRegistrationCount = 1,
            HasDefaultProvider = false
        };

        // Act
        var ex = Record.Exception(builder.Validate);

        // Assert
        Assert.Null(ex);
    }
}
