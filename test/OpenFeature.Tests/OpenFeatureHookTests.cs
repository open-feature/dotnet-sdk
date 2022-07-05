using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
    public class OpenFeatureHookTests
    {
        [Fact]
        public async Task ShouldRunAllHooksSuccessfully()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<bool>();

            var featureProviderMock = new Mock<IFeatureProvider>();
            featureProviderMock
                .Setup(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
                .ReturnsAsync(new ResolutionDetails<bool>(flagName, defaultValue));

            var client = new FeatureClient(featureProviderMock.Object, clientName, clientVersion);
            client.AddHooks(new TestHook());

            (await client.GetBooleanValue(flagName, defaultValue)).Should().Be(defaultValue);

            featureProviderMock.Verify(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
        }

        [Fact]
        public async Task ShouldCatchExceptionFromNoImplementedHookMethod()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<bool>();

            var featureProviderMock = new Mock<IFeatureProvider>();
            featureProviderMock
                .Setup(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
                .ReturnsAsync(new ResolutionDetails<bool>(flagName, defaultValue));

            var hookMock = new Mock<Hook>();
            hookMock
                .Setup(x => x.Before(It.IsAny<HookContext<bool>>(), null))
                .ThrowsAsync(new NotImplementedException());

            hookMock
                .Setup(x => x.After(It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), null))
                .ThrowsAsync(new NotImplementedException());

            hookMock
                .Setup(x => x.Finally(It.IsAny<HookContext<bool>>(), null))
                .ThrowsAsync(new NotImplementedException());

            var client = new FeatureClient(featureProviderMock.Object, clientName, clientVersion);
            client.AddHooks(hookMock.Object);

            (await client.GetBooleanValue(flagName, defaultValue)).Should().Be(defaultValue);

            featureProviderMock.Verify(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
            hookMock.Verify(x => x.Before(It.IsAny<HookContext<bool>>(), null), Times.Once);
            hookMock.Verify(x => x.After(It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), null), Times.Once);
            hookMock.Verify(x => x.Finally(It.IsAny<HookContext<bool>>(), null), Times.Once);
        }

        [Fact]
        public async Task ShouldCatchExceptionFromNoImplementedHookMethodWhenProviderThrowException()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<bool>();

            var featureProviderMock = new Mock<IFeatureProvider>();
            featureProviderMock
                .Setup(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
                .ThrowsAsync(new Exception());

            var hookMock = new Mock<Hook>();
            hookMock
                .Setup(x => x.Before(It.IsAny<HookContext<bool>>(), null))
                .ThrowsAsync(new NotImplementedException());

            hookMock
                .Setup(x => x.Error(It.IsAny<HookContext<bool>>(), It.IsAny<Exception>(), null))
                .ThrowsAsync(new NotImplementedException());

            hookMock
                .Setup(x => x.Finally(It.IsAny<HookContext<bool>>(), null))
                .ThrowsAsync(new NotImplementedException());

            var client = new FeatureClient(featureProviderMock.Object, clientName, clientVersion);
            client.AddHooks(hookMock.Object);

            (await client.GetBooleanValue(flagName, defaultValue)).Should().Be(defaultValue);

            featureProviderMock.Verify(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
            hookMock.Verify(x => x.Before(It.IsAny<HookContext<bool>>(), null), Times.Once);
            hookMock.Verify(x => x.Error(It.IsAny<HookContext<bool>>(), It.IsAny<Exception>(), null), Times.Once);
            hookMock.Verify(x => x.Finally(It.IsAny<HookContext<bool>>(), null), Times.Once);
        }
    }
}
