using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
    public class OpenFeatureClientTests
    {
        [Fact]
        public async Task ShouldResolveBooleanValue()
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

            OpenFeature.SetProvider(featureProviderMock.Object);
            var client = OpenFeature.GetClient(clientName, clientVersion);

            (await client.GetBooleanValue(flagName, defaultValue)).Should().Be(defaultValue);

            featureProviderMock.Verify(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
        }

        [Fact]
        public async Task ShouldResolveStringValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<string>();

            var featureProviderMock = new Mock<IFeatureProvider>();
            featureProviderMock
                .Setup(x => x.ResolveStringValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
                .ReturnsAsync(new ResolutionDetails<string>(flagName, defaultValue));

            OpenFeature.SetProvider(featureProviderMock.Object);
            var client = OpenFeature.GetClient(clientName, clientVersion);

            (await client.GetStringValue(flagName, defaultValue)).Should().Be(defaultValue);

            featureProviderMock.Verify(x => x.ResolveStringValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
        }

        [Fact]
        public async Task ShouldResolveNumberValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<int>();

            var featureProviderMock = new Mock<IFeatureProvider>();
            featureProviderMock
                .Setup(x => x.ResolveNumberValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
                .ReturnsAsync(new ResolutionDetails<int>(flagName, defaultValue));

            OpenFeature.SetProvider(featureProviderMock.Object);
            var client = OpenFeature.GetClient(clientName, clientVersion);

            (await client.GetNumberValue(flagName, defaultValue)).Should().Be(defaultValue);

            featureProviderMock.Verify(x => x.ResolveNumberValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
        }

        [Fact]
        public async Task ShouldResolveStructureValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<TestStructure>();

            var featureProviderMock = new Mock<IFeatureProvider>();
            featureProviderMock
                .Setup(x => x.ResolveStructureValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
                .ReturnsAsync(new ResolutionDetails<TestStructure>(flagName, defaultValue));

            OpenFeature.SetProvider(featureProviderMock.Object);
            var client = OpenFeature.GetClient(clientName, clientVersion);

            (await client.GetObjectValue(flagName, defaultValue)).Should().Be(defaultValue);

            featureProviderMock.Verify(x => x.ResolveStructureValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
        }

        [Fact]
        public async Task WhenExceptionOccursDuringEvaluationShouldReturnError()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<TestStructure>();
            var errorMessage = fixture.Create<string>();

            var featureProviderMock = new Mock<IFeatureProvider>();
            featureProviderMock
                .Setup(x => x.ResolveStructureValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
                .Throws(new Exception(errorMessage));

            OpenFeature.SetProvider(featureProviderMock.Object);
            var client = OpenFeature.GetClient(clientName, clientVersion);
            var response = await client.GetObjectDetails(flagName, defaultValue);

            response.ErrorCode.Should().Be(errorMessage);
            response.Reason.Should().Be(Constant.Reason.Error);
            featureProviderMock.Verify(x => x.ResolveStructureValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
        }

        [Fact]
        public void ShouldAddGivenHooks()
        {
            var fixture = new Fixture();
            var hooks = fixture.Create<List<TestHook>>();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();

            var client = OpenFeature.GetClient(clientName, clientVersion);

            client.AddHooks(hooks);

            client.GetHooks().Should().Contain(hooks);

            client.ClearHooks();

            client.GetHooks().Should().BeEmpty();
        }
    }
}
