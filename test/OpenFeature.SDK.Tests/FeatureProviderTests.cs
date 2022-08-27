using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenFeature.SDK.Constant;
using OpenFeature.SDK.Model;
using OpenFeature.SDK.Tests.Internal;
using Xunit;

namespace OpenFeature.SDK.Tests
{
    public class FeatureProviderTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("2.1", "The provider interface MUST define a `metadata` member or accessor, containing a `name` field or accessor of type string, which identifies the provider implementation.")]
        public void Provider_Must_Have_Metadata()
        {
            var provider = new TestProvider();

            provider.GetMetadata().Name.Should().Be(TestProvider.Name);
        }

        [Fact]
        [Specification("2.2", "The `feature provider` interface MUST define methods to resolve flag values, with parameters `flag key` (string, required), `default value` (boolean | number | string | structure, required) and `evaluation context` (optional), which returns a `flag resolution` structure.")]
        [Specification("2.3.1", "The `feature provider` interface MUST define methods for typed flag resolution, including boolean, numeric, string, and structure.")]
        [Specification("2.4", "In cases of normal execution, the `provider` MUST populate the `flag resolution` structure's `value` field with the resolved flag value.")]
        [Specification("2.5", "In cases of normal execution, the `provider` SHOULD populate the `flag resolution` structure's `variant` field with a string identifier corresponding to the returned flag value.")]
        [Specification("2.6", "The `provider` SHOULD populate the `flag resolution` structure's `reason` field with a string indicating the semantic reason for the returned flag value.")]
        [Specification("2.7", "In cases of normal execution, the `provider` MUST NOT populate the `flag resolution` structure's `error code` field, or otherwise must populate it with a null or falsy value.")]
        [Specification("2.9", "In cases of normal execution, the `provider` MUST NOT populate the `flag resolution` structure's `error code` field, or otherwise must populate it with a null or falsy value.")]
        public async Task Provider_Must_Resolve_Flag_Values()
        {
            var fixture = new Fixture();
            var flagName = fixture.Create<string>();
            var defaultBoolValue = fixture.Create<bool>();
            var defaultStringValue = fixture.Create<string>();
            var defaultIntegerValue = fixture.Create<int>();
            var defaultDoubleValue = fixture.Create<double>();
            var defaultStructureValue = fixture.Create<Structure>();
            var provider = new NoOpFeatureProvider();

            var boolResolutionDetails = new ResolutionDetails<bool>(flagName, defaultBoolValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveBooleanValue(flagName, defaultBoolValue)).Should().BeEquivalentTo(boolResolutionDetails);

            var integerResolutionDetails = new ResolutionDetails<int>(flagName, defaultIntegerValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveIntegerValue(flagName, defaultIntegerValue)).Should().BeEquivalentTo(integerResolutionDetails);

            var doubleResolutionDetails = new ResolutionDetails<double>(flagName, defaultDoubleValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveDoubleValue(flagName, defaultDoubleValue)).Should().BeEquivalentTo(doubleResolutionDetails);

            var stringResolutionDetails = new ResolutionDetails<string>(flagName, defaultStringValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveStringValue(flagName, defaultStringValue)).Should().BeEquivalentTo(stringResolutionDetails);

            var structureResolutionDetails = new ResolutionDetails<Structure>(flagName, defaultStructureValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveStructureValue(flagName, defaultStructureValue)).Should().BeEquivalentTo(structureResolutionDetails);
        }

        [Fact]
        [Specification("2.8", "In cases of abnormal execution, the `provider` MUST indicate an error using the idioms of the implementation language, with an associated error code having possible values `PROVIDER_NOT_READY`, `FLAG_NOT_FOUND`, `PARSE_ERROR`, `TYPE_MISMATCH`, or `GENERAL`.")]
        public async Task Provider_Must_ErrorType()
        {
            var fixture = new Fixture();
            var flagName = fixture.Create<string>();
            var flagName2 = fixture.Create<string>();
            var defaultBoolValue = fixture.Create<bool>();
            var defaultStringValue = fixture.Create<string>();
            var defaultIntegerValue = fixture.Create<int>();
            var defaultDoubleValue = fixture.Create<double>();
            var defaultStructureValue = fixture.Create<Structure>();
            var providerMock = new Mock<FeatureProvider>(MockBehavior.Strict);

            providerMock.Setup(x => x.ResolveBooleanValue(flagName, defaultBoolValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<bool>(flagName, defaultBoolValue, ErrorType.General, NoOpProvider.ReasonNoOp, NoOpProvider.Variant));

            providerMock.Setup(x => x.ResolveIntegerValue(flagName, defaultIntegerValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<int>(flagName, defaultIntegerValue, ErrorType.ParseError, NoOpProvider.ReasonNoOp, NoOpProvider.Variant));

            providerMock.Setup(x => x.ResolveDoubleValue(flagName, defaultDoubleValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<double>(flagName, defaultDoubleValue, ErrorType.ParseError, NoOpProvider.ReasonNoOp, NoOpProvider.Variant));

            providerMock.Setup(x => x.ResolveStringValue(flagName, defaultStringValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<string>(flagName, defaultStringValue, ErrorType.TypeMismatch, NoOpProvider.ReasonNoOp, NoOpProvider.Variant));

            providerMock.Setup(x => x.ResolveStructureValue(flagName, defaultStructureValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<Structure>(flagName, defaultStructureValue, ErrorType.FlagNotFound, NoOpProvider.ReasonNoOp, NoOpProvider.Variant));

            providerMock.Setup(x => x.ResolveStructureValue(flagName2, defaultStructureValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<Structure>(flagName, defaultStructureValue, ErrorType.ProviderNotReady, NoOpProvider.ReasonNoOp, NoOpProvider.Variant));

            var provider = providerMock.Object;

            (await provider.ResolveBooleanValue(flagName, defaultBoolValue)).ErrorType.Should().Be(ErrorType.General);
            (await provider.ResolveIntegerValue(flagName, defaultIntegerValue)).ErrorType.Should().Be(ErrorType.ParseError);
            (await provider.ResolveDoubleValue(flagName, defaultDoubleValue)).ErrorType.Should().Be(ErrorType.ParseError);
            (await provider.ResolveStringValue(flagName, defaultStringValue)).ErrorType.Should().Be(ErrorType.TypeMismatch);
            (await provider.ResolveStructureValue(flagName, defaultStructureValue)).ErrorType.Should().Be(ErrorType.FlagNotFound);
            (await provider.ResolveStructureValue(flagName2, defaultStructureValue)).ErrorType.Should().Be(ErrorType.ProviderNotReady);
        }
    }
}
