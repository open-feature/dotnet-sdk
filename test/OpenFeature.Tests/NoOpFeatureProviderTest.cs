using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using OpenFeature.Constant;
using Xunit;

namespace OpenFeature.Tests
{
    public class NoOpFeatureProviderTest
    {
        [Fact]
        public async Task ShouldResolveBoolFlag()
        {
            var fixture = new Fixture();
            var flagKey = fixture.Create<string>();
            var defaultValue = fixture.Create<bool>();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var client = new FeatureClient(new NoOpFeatureProvider(), clientName, clientVersion);
        
            var result = await client.GetBooleanValue(flagKey, defaultValue);
            var resultDetails = await client.GetBooleanDetails(flagKey, defaultValue);

            result.Should().Be(defaultValue);
            resultDetails.Reason.Should().Be(NoOpProvider.ReasonNoOp);
            resultDetails.Value.Should().Be(defaultValue);
        }
    
        [Fact]
        public async Task ShouldResolveNumberFlag()
        {
            var fixture = new Fixture();
            var flagKey = fixture.Create<string>();
            var defaultValue = fixture.Create<int>();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var client = new FeatureClient(new NoOpFeatureProvider(), clientName, clientVersion);
        
            var result = await client.GetNumberValue(flagKey, defaultValue);
            var resultDetails = await client.GetNumberDetails(flagKey, defaultValue);

            result.Should().Be(defaultValue);
            resultDetails.Reason.Should().Be(NoOpProvider.ReasonNoOp);
            resultDetails.Value.Should().Be(defaultValue);
        }
    
        [Fact]
        public async Task ShouldResolveStringFlag()
        {
            var fixture = new Fixture();
            var flagKey = fixture.Create<string>();
            var defaultValue = fixture.Create<string>();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var client = new FeatureClient(new NoOpFeatureProvider(), clientName, clientVersion);
        
            var result = await client.GetStringValue(flagKey, defaultValue);
            var resultDetails = await client.GetStringDetails(flagKey, defaultValue);

            result.Should().Be(defaultValue);
            resultDetails.Reason.Should().Be(NoOpProvider.ReasonNoOp);
            resultDetails.Value.Should().Be(defaultValue);
        }
    
        [Fact]
        public async Task ShouldResolveStructureFlag()
        {
            var fixture = new Fixture();
            var flagKey = fixture.Create<string>();
            var testStructure = fixture.Create<TestStructure>();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var client = new FeatureClient(new NoOpFeatureProvider(), clientName, clientVersion);

            var result = await client.GetObjectValue(flagKey, testStructure);
            var resultDetails = await client.GetObjectDetails(flagKey, testStructure);

            result.Should().Be(testStructure);
            resultDetails.Reason.Should().Be(NoOpProvider.ReasonNoOp);
            resultDetails.Value.Should().Be(testStructure);
        }
    }
}