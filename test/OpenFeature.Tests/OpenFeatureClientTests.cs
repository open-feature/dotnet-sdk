using System;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests;

public class OpenFeatureClientTests
{
    [Fact]
    public void ShouldResolveBooleanValue()
    {
        var fixture = new Fixture();
        var clientName = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<bool>();
        
        var featureProviderMock = new Mock<IFeatureProvider>();
        featureProviderMock
            .Setup(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
            .Returns(new ResolutionDetails<bool>(flagName, defaultValue));

        OpenFeature.SetProvider(featureProviderMock.Object);
        var client = OpenFeature.GetClient(clientName, clientVersion);

        client.GetBooleanValue(flagName, defaultValue).Should().Be(defaultValue);
        
        featureProviderMock.Verify(x => x.ResolveBooleanValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
    }
    
    [Fact]
    public void ShouldResolveStringValue()
    {
        var fixture = new Fixture();
        var clientName = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<string>();
        
        var featureProviderMock = new Mock<IFeatureProvider>();
        featureProviderMock
            .Setup(x => x.ResolveStringValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
            .Returns(new ResolutionDetails<string>(flagName, defaultValue));

        OpenFeature.SetProvider(featureProviderMock.Object);
        var client = OpenFeature.GetClient(clientName, clientVersion);

        client.GetStringValue(flagName, defaultValue).Should().Be(defaultValue);
        
        featureProviderMock.Verify(x => x.ResolveStringValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
    }
    
    [Fact]
    public void ShouldResolveNumberValue()
    {
        var fixture = new Fixture();
        var clientName = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<int>();
        
        var featureProviderMock = new Mock<IFeatureProvider>();
        featureProviderMock
            .Setup(x => x.ResolveNumberValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
            .Returns(new ResolutionDetails<int>(flagName, defaultValue));

        OpenFeature.SetProvider(featureProviderMock.Object);
        var client = OpenFeature.GetClient(clientName, clientVersion);

        client.GetNumberValue(flagName, defaultValue).Should().Be(defaultValue);
        
        featureProviderMock.Verify(x => x.ResolveNumberValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
    }
    
    [Fact]
    public void ShouldResolveStructureValue()
    {
        var fixture = new Fixture();
        var clientName = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<TestStructure>();
        
        var featureProviderMock = new Mock<IFeatureProvider>();
        featureProviderMock
            .Setup(x => x.ResolveStructureValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null))
            .Returns(new ResolutionDetails<TestStructure>(flagName, defaultValue));

        OpenFeature.SetProvider(featureProviderMock.Object);
        var client = OpenFeature.GetClient(clientName, clientVersion);

        client.GetObjectValue(flagName, defaultValue).Should().Be(defaultValue);
        
        featureProviderMock.Verify(x => x.ResolveStructureValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
    }

    [Fact]
    public void WhenExceptionOccursDuringEvaluationShouldReturnError()
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
        var response = client.GetObjectDetails(flagName, defaultValue);

        response.ErrorCode.Should().Be(errorMessage);
        response.Reason.Should().Be(Constant.Reason.Error);
        featureProviderMock.Verify(x => x.ResolveStructureValue(flagName, defaultValue, It.IsAny<EvaluationContext>(), null), Times.Once);
    }

    private class TestStructure
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}