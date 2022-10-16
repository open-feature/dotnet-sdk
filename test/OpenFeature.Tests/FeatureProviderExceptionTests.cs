using System;
using FluentAssertions;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Extension;
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
            var ex = new FeatureProviderException(errorType);
            ex.ErrorType.GetDescription().Should().Be(errorDescription);
        }

        [Theory]
        [InlineData(ErrorType.General, "Subscription has expired, please renew your subscription.")]
        [InlineData(ErrorType.ProviderNotReady, "User has exceeded the quota for this feature.")]
        public void FeatureProviderException_Should_Allow_Custom_ErrorCode_Messages(ErrorType errorCode, string message)
        {
            var ex = new FeatureProviderException(errorCode, message, new ArgumentOutOfRangeException("flag"));
            ex.ErrorType.Should().Be(errorCode);
            ex.Message.Should().Be(message);
            ex.InnerException.Should().BeOfType<ArgumentOutOfRangeException>();
        }
    }
}
