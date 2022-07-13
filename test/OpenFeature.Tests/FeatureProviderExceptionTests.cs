using System;
using FluentAssertions;
using OpenFeature.Constant;
using OpenFeature.Error;
using Xunit;

namespace OpenFeature.Tests
{
    public class FeatureProviderExceptionTests
    {
        [Theory]
        [InlineData(ErrorType.General, "GENERAL")]
        [InlineData(ErrorType.ParseError, "PARSE_ERROR")]
        [InlineData(ErrorType.TypeMismatch, "TYPE_MISMATCH")]
        [InlineData(ErrorType.FlagNotFound, "FLAG_NOT_FOUND")]
        [InlineData(ErrorType.ProviderNotReady, "PROVIDER_NOT_READY")]
        public void FeatureProviderException_Should_Resolve_Description(ErrorType errorType, string errorDescription)
        {
            var ex = new FeatureProviderException(errorType, new Exception());
            ex.ErrorType.Should().Be(errorType);
            ex.ErrorTypeDescription.Should().Be(errorDescription);
        }
    }
}
