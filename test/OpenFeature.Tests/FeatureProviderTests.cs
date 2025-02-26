using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public class FeatureProviderTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("2.1.1", "The provider interface MUST define a `metadata` member or accessor, containing a `name` field or accessor of type string, which identifies the provider implementation.")]
        public void Provider_Must_Have_Metadata()
        {
            var provider = new TestProvider();

            Assert.Equal(TestProvider.DefaultName, provider.GetMetadata().Name);
        }

        [Fact]
        [Specification("2.2.1", "The `feature provider` interface MUST define methods to resolve flag values, with parameters `flag key` (string, required), `default value` (boolean | number | string | structure, required) and `evaluation context` (optional), which returns a `resolution details` structure.")]
        [Specification("2.2.2.1", "The `feature provider` interface MUST define methods for typed flag resolution, including boolean, numeric, string, and structure.")]
        [Specification("2.2.3", "In cases of normal execution, the `provider` MUST populate the `resolution details` structure's `value` field with the resolved flag value.")]
        [Specification("2.2.4", "In cases of normal execution, the `provider` SHOULD populate the `resolution details` structure's `variant` field with a string identifier corresponding to the returned flag value.")]
        [Specification("2.2.5", "The `provider` SHOULD populate the `resolution details` structure's `reason` field with `\"STATIC\"`, `\"DEFAULT\",` `\"TARGETING_MATCH\"`, `\"SPLIT\"`, `\"CACHED\"`, `\"DISABLED\"`, `\"UNKNOWN\"`, `\"ERROR\"` or some other string indicating the semantic reason for the returned flag value.")]
        [Specification("2.2.6", "In cases of normal execution, the `provider` MUST NOT populate the `resolution details` structure's `error code` field, or otherwise must populate it with a null or falsy value.")]
        [Specification("2.2.8.1", "The `resolution details` structure SHOULD accept a generic argument (or use an equivalent language feature) which indicates the type of the wrapped `value` field.")]
        [Specification("2.3.2", "In cases of normal execution, the `provider` MUST NOT populate the `resolution details` structure's `error message` field, or otherwise must populate it with a null or falsy value.")]
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
            Assert.Equivalent(boolResolutionDetails, await provider.ResolveBooleanValueAsync(flagName, defaultBoolValue));

            var integerResolutionDetails = new ResolutionDetails<int>(flagName, defaultIntegerValue, ErrorType.None,
                NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            Assert.Equivalent(integerResolutionDetails, await provider.ResolveIntegerValueAsync(flagName, defaultIntegerValue));

            var doubleResolutionDetails = new ResolutionDetails<double>(flagName, defaultDoubleValue, ErrorType.None,
                NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            Assert.Equivalent(doubleResolutionDetails, await provider.ResolveDoubleValueAsync(flagName, defaultDoubleValue));

            var stringResolutionDetails = new ResolutionDetails<string>(flagName, defaultStringValue, ErrorType.None,
                NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            Assert.Equivalent(stringResolutionDetails, await provider.ResolveStringValueAsync(flagName, defaultStringValue));

            var structureResolutionDetails = new ResolutionDetails<Value>(flagName, defaultStructureValue,
                ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            Assert.Equivalent(structureResolutionDetails, await provider.ResolveStructureValueAsync(flagName, defaultStructureValue));
        }

        [Fact]
        [Specification("2.2.7", "In cases of abnormal execution, the `provider` MUST indicate an error using the idioms of the implementation language, with an associated `error code` and optional associated `error message`.")]
        [Specification("2.3.3", "In cases of abnormal execution, the `resolution details` structure's `error message` field MAY contain a string containing additional detail about the nature of the error.")]
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
            var providerMock = Substitute.For<FeatureProvider>();
            const string testMessage = "An error message";

            providerMock.ResolveBooleanValueAsync(flagName, defaultBoolValue, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<bool>(flagName, defaultBoolValue, ErrorType.General,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.ResolveIntegerValueAsync(flagName, defaultIntegerValue, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<int>(flagName, defaultIntegerValue, ErrorType.ParseError,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.ResolveDoubleValueAsync(flagName, defaultDoubleValue, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<double>(flagName, defaultDoubleValue, ErrorType.InvalidContext,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.ResolveStringValueAsync(flagName, defaultStringValue, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<string>(flagName, defaultStringValue, ErrorType.TypeMismatch,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.ResolveStructureValueAsync(flagName, defaultStructureValue, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<Value>(flagName, defaultStructureValue, ErrorType.FlagNotFound,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.ResolveStructureValueAsync(flagName2, defaultStructureValue, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<Value>(flagName2, defaultStructureValue, ErrorType.ProviderNotReady,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant, testMessage));

            providerMock.ResolveBooleanValueAsync(flagName2, defaultBoolValue, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<bool>(flagName2, defaultBoolValue, ErrorType.TargetingKeyMissing,
                    NoOpProvider.ReasonNoOp, NoOpProvider.Variant));

            var boolRes = await providerMock.ResolveBooleanValueAsync(flagName, defaultBoolValue);
            Assert.Equal(ErrorType.General, boolRes.ErrorType);
            Assert.Equal(testMessage, boolRes.ErrorMessage);

            var intRes = await providerMock.ResolveIntegerValueAsync(flagName, defaultIntegerValue);
            Assert.Equal(ErrorType.ParseError, intRes.ErrorType);
            Assert.Equal(testMessage, intRes.ErrorMessage);

            var doubleRes = await providerMock.ResolveDoubleValueAsync(flagName, defaultDoubleValue);
            Assert.Equal(ErrorType.InvalidContext, doubleRes.ErrorType);
            Assert.Equal(testMessage, doubleRes.ErrorMessage);

            var stringRes = await providerMock.ResolveStringValueAsync(flagName, defaultStringValue);
            Assert.Equal(ErrorType.TypeMismatch, stringRes.ErrorType);
            Assert.Equal(testMessage, stringRes.ErrorMessage);

            var structRes1 = await providerMock.ResolveStructureValueAsync(flagName, defaultStructureValue);
            Assert.Equal(ErrorType.FlagNotFound, structRes1.ErrorType);
            Assert.Equal(testMessage, structRes1.ErrorMessage);

            var structRes2 = await providerMock.ResolveStructureValueAsync(flagName2, defaultStructureValue);
            Assert.Equal(ErrorType.ProviderNotReady, structRes2.ErrorType);
            Assert.Equal(testMessage, structRes2.ErrorMessage);

            var boolRes2 = await providerMock.ResolveBooleanValueAsync(flagName2, defaultBoolValue);
            Assert.Equal(ErrorType.TargetingKeyMissing, boolRes2.ErrorType);
            Assert.Null(boolRes2.ErrorMessage);
        }
    }
}
