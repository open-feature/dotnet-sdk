using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Tests.Strategies.Models;

public class FinalResultTests
{
    private const string TestFlagKey = "test-flag";
    private const string TestProviderName = "test-provider";
    private const string TestVariant = "test-variant";
    private const bool TestValue = true;

    private readonly FeatureProvider _mockProvider = Substitute.For<FeatureProvider>();
    private readonly ResolutionDetails<bool> _testDetails = new(TestFlagKey, TestValue, ErrorType.None, Reason.Static, TestVariant);

    [Fact]
    public void Constructor_WithAllParameters_CreatesFinalResult()
    {
        // Arrange
        var errors = new List<ProviderError>
        {
            new("provider1", new InvalidOperationException("Test error"))
        };

        // Act
        var result = new FinalResult<bool>(this._testDetails, this._mockProvider, TestProviderName, errors);

        // Assert
        Assert.Equal(this._testDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(errors, result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_CreatesEmptyErrorsList()
    {
        // Act
        var result = new FinalResult<bool>(this._testDetails, this._mockProvider, TestProviderName, null);

        // Assert
        Assert.Equal(this._testDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyErrors_CreatesEmptyErrorsList()
    {
        // Arrange
        var emptyErrors = new List<ProviderError>();

        // Act
        var result = new FinalResult<bool>(this._testDetails, this._mockProvider, TestProviderName, emptyErrors);

        // Assert
        Assert.Equal(this._testDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(emptyErrors, result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithMultipleErrors_StoresAllErrors()
    {
        // Arrange
        var errors = new List<ProviderError>
        {
            new("provider1", new InvalidOperationException("Error 1")),
            new("provider2", new ArgumentException("Error 2")),
            new("provider3", null)
        };

        // Act
        var result = new FinalResult<bool>(this._testDetails, this._mockProvider, TestProviderName, errors);

        // Assert
        Assert.Equal(this._testDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Equal(errors, result.Errors);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void Constructor_WithDifferentGenericType_CreatesTypedResult()
    {
        // Arrange
        const string stringValue = "test-string-value";
        var stringDetails = new ResolutionDetails<string>(TestFlagKey, stringValue, ErrorType.None, Reason.Static, TestVariant);

        // Act
        var result = new FinalResult<string>(stringDetails, this._mockProvider, TestProviderName, null);

        // Assert
        Assert.Equal(stringDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithIntegerType_CreatesTypedResult()
    {
        // Arrange
        const int intValue = 42;
        var intDetails = new ResolutionDetails<int>(TestFlagKey, intValue, ErrorType.None, Reason.Static, TestVariant);

        // Act
        var result = new FinalResult<int>(intDetails, this._mockProvider, TestProviderName, null);

        // Assert
        Assert.Equal(intDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithComplexType_CreatesTypedResult()
    {
        // Arrange
        var complexValue = new { Name = "Test", Value = 123 };
        var complexDetails = new ResolutionDetails<object>(TestFlagKey, complexValue, ErrorType.None, Reason.Static, TestVariant);

        // Act
        var result = new FinalResult<object>(complexDetails, this._mockProvider, TestProviderName, null);

        // Assert
        Assert.Equal(complexDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithErrorDetails_PreservesErrorInformation()
    {
        // Arrange
        var errorDetails = new ResolutionDetails<bool>(TestFlagKey, false, ErrorType.ProviderNotReady, Reason.Error, errorMessage: "Provider not ready");
        var errors = new List<ProviderError>
        {
            new(TestProviderName, new InvalidOperationException("Provider initialization failed"))
        };

        // Act
        var result = new FinalResult<bool>(errorDetails, this._mockProvider, TestProviderName, errors);

        // Assert
        Assert.Equal(errorDetails, result.Details);
        Assert.Equal(ErrorType.ProviderNotReady, result.Details.ErrorType);
        Assert.Equal(Reason.Error, result.Details.Reason);
        Assert.Equal("Provider not ready", result.Details.ErrorMessage);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Details_Property_HasPrivateSetter()
    {
        // Act & Assert
        var detailsProperty = typeof(FinalResult<bool>).GetProperty(nameof(FinalResult<bool>.Details));
        Assert.NotNull(detailsProperty);
        Assert.True(detailsProperty.CanRead);
        Assert.True(detailsProperty.CanWrite);
        Assert.True(detailsProperty.GetSetMethod(true)?.IsPrivate);
    }

    [Fact]
    public void Provider_Property_HasPrivateSetter()
    {
        // Act & Assert
        var providerProperty = typeof(FinalResult<bool>).GetProperty(nameof(FinalResult<bool>.Provider));
        Assert.NotNull(providerProperty);
        Assert.True(providerProperty.CanRead);
        Assert.True(providerProperty.CanWrite);
        Assert.True(providerProperty.GetSetMethod(true)?.IsPrivate);
    }

    [Fact]
    public void ProviderName_Property_HasPrivateSetter()
    {
        // Act & Assert
        var providerNameProperty = typeof(FinalResult<bool>).GetProperty(nameof(FinalResult<bool>.ProviderName));
        Assert.NotNull(providerNameProperty);
        Assert.True(providerNameProperty.CanRead);
        Assert.True(providerNameProperty.CanWrite);
        Assert.True(providerNameProperty.GetSetMethod(true)?.IsPrivate);
    }

    [Fact]
    public void Errors_Property_HasPrivateSetter()
    {
        // Act & Assert
        var errorsProperty = typeof(FinalResult<bool>).GetProperty(nameof(FinalResult<bool>.Errors));
        Assert.NotNull(errorsProperty);
        Assert.True(errorsProperty.CanRead);
        Assert.True(errorsProperty.CanWrite);
        Assert.True(errorsProperty.GetSetMethod(true)?.IsPrivate);
    }

    [Fact]
    public void Constructor_WithNullProvider_StoresNullProvider()
    {
        // Act
        var result = new FinalResult<bool>(this._testDetails, null!, TestProviderName, null);

        // Assert
        Assert.Equal(this._testDetails, result.Details);
        Assert.Null(result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithNullProviderName_StoresNullProviderName()
    {
        // Act
        var result = new FinalResult<bool>(this._testDetails, this._mockProvider, null!, null);

        // Assert
        Assert.Equal(this._testDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Null(result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyProviderName_StoresEmptyProviderName()
    {
        // Act
        var result = new FinalResult<bool>(this._testDetails, this._mockProvider, string.Empty, null);

        // Assert
        Assert.Equal(this._testDetails, result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(string.Empty, result.ProviderName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Constructor_WithNullDetails_StoresNullDetails()
    {
        // Act
        var result = new FinalResult<bool>(null!, this._mockProvider, TestProviderName, null);

        // Assert
        Assert.Null(result.Details);
        Assert.Equal(this._mockProvider, result.Provider);
        Assert.Equal(TestProviderName, result.ProviderName);
        Assert.Empty(result.Errors);
    }
}
