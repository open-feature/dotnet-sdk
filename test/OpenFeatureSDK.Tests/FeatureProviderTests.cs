using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenFeatureSDK.Constant;
using OpenFeatureSDK.Model;
using OpenFeatureSDK.Tests.Internal;
using Xunit;

namespace OpenFeatureSDK.Tests
{
    public class FeatureProviderTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("2.1",
            "The provider interface MUST define a `metadata` member or accessor, containing a `name` field or accessor of type string, which identifies the provider implementation.")]
        public void Provider_Must_Have_Metadata()
        {
            var provider = new TestProvider();

            provider.GetMetadata().Name.Should().Be(TestProvider.Name);
        }

        [Fact]
        [Specification("2.2",
            "The `feature provider` interface MUST define methods to resolve flag values, with parameters `flag key` (string, required), `default value` (boolean | number | string | structure, required) and `evaluation context` (optional), which returns a `flag resolution` structure.")]
        [Specification("2.3.1",
            "The `feature provider` interface MUST define methods for typed flag resolution, including boolean, numeric, string, and structure.")]
        [Specification("2.4",
            "In cases of normal execution, the `provider` MUST populate the `flag resolution` structure's `value` field with the resolved flag value.")]
        [Specification("2.5",
            "In cases of normal execution, the `provider` SHOULD populate the `flag resolution` structure's `variant` field with a string identifier corresponding to the returned flag value.")]
        [Specification("2.6",
            "The `provider` SHOULD populate the `flag resolution` structure's `reason` field with a string indicating the semantic reason for the returned flag value.")]
        [Specification("2.7",
            "In cases of normal execution, the `provider` MUST NOT populate the `flag resolution` structure's `error code` field, or otherwise must populate it with a null or falsy value.")]
        [Specification("2.9.1",
            "The `flag resolution` structure SHOULD accept a generic argument (or use an equivalent language feature) which indicates the type of the wrapped `value` field.")]
        [Specification("2.11",
            "In cases of normal execution, the `provider` MUST NOT populate the `flag resolution` structure's `error message` field, or otherwise must populate it with a null or falsy value.")]
        public async Task Provider_Must_Resolve_Flag_Values()
        {
            var fixture = new Fixture();
            var flagName = fixture.Create<string>();
            var defaultBoolValue = fixture.Create<bool>();
            var defaultStringValue = fixture.Create<string>();
            var defaultIntegerValue = fixture.Create<int>();
            var defaultDoubleValue = fixture.Create<double>();
            var defaultStructureValue = fixture.Create<Value>();
            var provider = new NoOpFeatureProvider();

            var boolResolutionDetails = new ResolutionDetails<bool>(flagName, defaultBoolValue, ErrorType.None,
                NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveBooleanValue(flagName, defaultBoolValue)).Should()
                .BeEquivalentTo(boolResolutionDetails);

            var integerResolutionDetails = new ResolutionDetails<int>(flagName, defaultIntegerValue, ErrorType.None,
                NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveIntegerValue(flagName, defaultIntegerValue)).Should()
                .BeEquivalentTo(integerResolutionDetails);

            var doubleResolutionDetails = new ResolutionDetails<double>(flagName, defaultDoubleValue, ErrorType.None,
                NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveDoubleValue(flagName, defaultDoubleValue)).Should()
                .BeEquivalentTo(doubleResolutionDetails);

            var stringResolutionDetails = new ResolutionDetails<string>(flagName, defaultStringValue, ErrorType.None,
                NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveStringValue(flagName, defaultStringValue)).Should()
                .BeEquivalentTo(stringResolutionDetails);

            var structureResolutionDetails = new ResolutionDetails<Value>(flagName, defaultStructureValue,
                ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await provider.ResolveStructureValue(flagName, defaultStructureValue)).Should()
                .BeEquivalentTo(structureResolutionDetails);
        }

        [Fact]
        [Specification("2.8",
            "In cases of abnormal execution, the `provider` MUST indicate an error using the idioms of the implementation language, with an associated `error code` and optional associated `error message`.")]
        [Specification("2.12",
            "In cases of abnormal execution, the `evaluation details` structure's `error message` field MAY contain a string containing additional detail about the nature of the error.")]
        public async Task Provider_Must_ErrorType()
        {
            var fixture = new Fixture();
            var flagName = fixture.Create<string>();
            var flagName2 = fixture.Create<string>();
            var defaultBoolValue = fixture.Create<bool>();
            var defaultStringValue = fixture.Create<string>();
            var defaultIntegerValue = fixture.Create<int>();
            var defaultDoubleValue = fixture.Create<double>();
            var defaultStructureValue = fixture.Create<Value>();
            var providerMock = new Mock<FeatureProvider>(MockBehavior.Strict);
            const string testMessage = "An error message";

            providerMock.Setup(x => x.ResolveBooleanValue(flagName, defaultBoolValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<bool>(flagName, defaultBoolValue, ErrorType.General,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.Setup(x => x.ResolveIntegerValue(flagName, defaultIntegerValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<int>(flagName, defaultIntegerValue, ErrorType.ParseError,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.Setup(x => x.ResolveDoubleValue(flagName, defaultDoubleValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<double>(flagName, defaultDoubleValue, ErrorType.InvalidContext,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.Setup(x => x.ResolveStringValue(flagName, defaultStringValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<string>(flagName, defaultStringValue, ErrorType.TypeMismatch,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.Setup(x =>
                    x.ResolveStructureValue(flagName, defaultStructureValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<Value>(flagName, defaultStructureValue, ErrorType.FlagNotFound,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.Setup(x =>
                    x.ResolveStructureValue(flagName2, defaultStructureValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<Value>(flagName2, defaultStructureValue, ErrorType.ProviderNotReady,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.Setup(x => x.ResolveBooleanValue(flagName2, defaultBoolValue, It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<bool>(flagName2, defaultBoolValue, ErrorType.TargetingKeyMissing,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant));


            var provider = providerMock.Object;

            var boolRes = await provider.ResolveBooleanValue(flagName, defaultBoolValue);
            boolRes.ErrorType.Should().Be(ErrorType.General);
            boolRes.ErrorMessage.Should().Be(testMessage);

            var intRes = await provider.ResolveIntegerValue(flagName, defaultIntegerValue);
            intRes.ErrorType.Should().Be(ErrorType.ParseError);
            intRes.ErrorMessage.Should().Be(testMessage);

            var doubleRes = await provider.ResolveDoubleValue(flagName, defaultDoubleValue);
            doubleRes.ErrorType.Should().Be(ErrorType.InvalidContext);
            doubleRes.ErrorMessage.Should().Be(testMessage);

            var stringRes = await provider.ResolveStringValue(flagName, defaultStringValue);
            stringRes.ErrorType.Should().Be(ErrorType.TypeMismatch);
            stringRes.ErrorMessage.Should().Be(testMessage);

            var structRes1 = await provider.ResolveStructureValue(flagName, defaultStructureValue);
            structRes1.ErrorType.Should().Be(ErrorType.FlagNotFound);
            structRes1.ErrorMessage.Should().Be(testMessage);

            var structRes2 = await provider.ResolveStructureValue(flagName2, defaultStructureValue);
            structRes2.ErrorType.Should().Be(ErrorType.ProviderNotReady);
            structRes2.ErrorMessage.Should().Be(testMessage);

            var boolRes2 = await provider.ResolveBooleanValue(flagName2, defaultBoolValue);
            boolRes2.ErrorType.Should().Be(ErrorType.TargetingKeyMissing);
            boolRes2.ErrorMessage.Should().BeNull();
        }
    }
}
