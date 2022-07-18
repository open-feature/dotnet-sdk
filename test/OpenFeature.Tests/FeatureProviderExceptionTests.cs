using System;
using FluentAssertions;
using OpenFeature.SDK.Constant;
using OpenFeature.SDK.Error;
using Xunit;

namespace OpenFeature.SDK.Tests
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
            var ex = new FeatureProviderException(errorType);
            ex.ErrorDescription.Should().Be(errorDescription);
        }

        [Theory]
        [InlineData("OUT_OF_CREDIT", "Subscription has expired, please renew your subscription.")]
        [InlineData("Exceed quota", "User has exceeded the quota for this feature.")]
        public void FeatureProviderException_Should_Allow_Custom_ErrorCode_Messages(string errorCode, string message)
        {
            var ex = new FeatureProviderException(errorCode, message, new ArgumentOutOfRangeException("flag"));
            ex.ErrorDescription.Should().Be(errorCode);
            ex.Message.Should().Be(message);
            ex.InnerException.Should().BeOfType<ArgumentOutOfRangeException>();
        }
    }
}
