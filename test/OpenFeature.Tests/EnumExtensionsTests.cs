using OpenFeature.Constant;
using OpenFeature.Extension;

namespace OpenFeature.Tests;

public class EnumExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.None, "NONE")]
    [InlineData(ErrorType.ProviderNotReady, "PROVIDER_NOT_READY")]
    [InlineData(ErrorType.FlagNotFound, "FLAG_NOT_FOUND")]
    [InlineData(ErrorType.ParseError, "PARSE_ERROR")]
    [InlineData(ErrorType.TypeMismatch, "TYPE_MISMATCH")]
    [InlineData(ErrorType.General, "GENERAL")]
    [InlineData(ErrorType.InvalidContext, "INVALID_CONTEXT")]
    [InlineData(ErrorType.TargetingKeyMissing, "TARGETING_KEY_MISSING")]
    [InlineData(ErrorType.ProviderFatal, "PROVIDER_FATAL")]
    public void GetDescription_WithErrorType_ReturnsExpectedDescription(ErrorType errorType, string expectedDescription)
    {
        // Act
        var result = errorType.GetDescription();

        // Assert
        Assert.Equal(expectedDescription, result);
    }
}
