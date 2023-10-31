using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public class OpenFeatureClientTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("1.2.1", "The client MUST provide a method to add `hooks` which accepts one or more API-conformant `hooks`, and appends them to the collection of any previously added hooks. When new hooks are added, previously added hooks are not removed.")]
        public void OpenFeatureClient_Should_Allow_Hooks()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var hook1 = Substitute.For<Hook>();
            var hook2 = Substitute.For<Hook>();
            var hook3 = Substitute.For<Hook>();

            var client = Api.Instance.GetClient(clientName);

            client.AddHooks(new[] { hook1, hook2 });

            client.GetHooks().Should().ContainInOrder(hook1, hook2);
            client.GetHooks().Count().Should().Be(2);

            client.AddHooks(hook3);
            client.GetHooks().Should().ContainInOrder(hook1, hook2, hook3);
            client.GetHooks().Count().Should().Be(3);

            client.ClearHooks();
            Assert.Empty(client.GetHooks());
        }

        [Fact]
        [Specification("1.2.2", "The client interface MUST define a `metadata` member or accessor, containing an immutable `name` field or accessor of type string, which corresponds to the `name` value supplied during client creation.")]
        public void OpenFeatureClient_Metadata_Should_Have_Name()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var client = Api.Instance.GetClient(clientName, clientVersion);

            client.GetMetadata().Name.Should().Be(clientName);
            client.GetMetadata().Version.Should().Be(clientVersion);
        }

        [Fact]
        [Specification("1.3.1", "The `client` MUST provide methods for typed flag evaluation, including boolean, numeric, string, and structure, with parameters `flag key` (string, required), `default value` (boolean | number | string | structure, required), `evaluation context` (optional), and `evaluation options` (optional), which returns the flag value.")]
        [Specification("1.3.2.1", "The client SHOULD provide functions for floating-point numbers and integers, consistent with language idioms.")]
        [Specification("1.3.3", "The `client` SHOULD guarantee the returned value of any typed flag evaluation method is of the expected type. If the value returned by the underlying provider implementation does not match the expected type, it's to be considered abnormal execution, and the supplied `default value` should be returned.")]
        public async Task OpenFeatureClient_Should_Allow_Flag_Evaluation()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultBoolValue = fixture.Create<bool>();
            var defaultStringValue = fixture.Create<string>();
            var defaultIntegerValue = fixture.Create<int>();
            var defaultDoubleValue = fixture.Create<double>();
            var defaultStructureValue = fixture.Create<Value>();
            var emptyFlagOptions = new FlagEvaluationOptions(ImmutableList<Hook>.Empty, ImmutableDictionary<string, object>.Empty);

            await Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(clientName, clientVersion);

            (await client.GetBooleanValue(flagName, defaultBoolValue)).Should().Be(defaultBoolValue);
            (await client.GetBooleanValue(flagName, defaultBoolValue, EvaluationContext.Empty)).Should().Be(defaultBoolValue);
            (await client.GetBooleanValue(flagName, defaultBoolValue, EvaluationContext.Empty, emptyFlagOptions)).Should().Be(defaultBoolValue);

            (await client.GetIntegerValue(flagName, defaultIntegerValue)).Should().Be(defaultIntegerValue);
            (await client.GetIntegerValue(flagName, defaultIntegerValue, EvaluationContext.Empty)).Should().Be(defaultIntegerValue);
            (await client.GetIntegerValue(flagName, defaultIntegerValue, EvaluationContext.Empty, emptyFlagOptions)).Should().Be(defaultIntegerValue);

            (await client.GetDoubleValue(flagName, defaultDoubleValue)).Should().Be(defaultDoubleValue);
            (await client.GetDoubleValue(flagName, defaultDoubleValue, EvaluationContext.Empty)).Should().Be(defaultDoubleValue);
            (await client.GetDoubleValue(flagName, defaultDoubleValue, EvaluationContext.Empty, emptyFlagOptions)).Should().Be(defaultDoubleValue);

            (await client.GetStringValue(flagName, defaultStringValue)).Should().Be(defaultStringValue);
            (await client.GetStringValue(flagName, defaultStringValue, EvaluationContext.Empty)).Should().Be(defaultStringValue);
            (await client.GetStringValue(flagName, defaultStringValue, EvaluationContext.Empty, emptyFlagOptions)).Should().Be(defaultStringValue);

            (await client.GetObjectValue(flagName, defaultStructureValue)).Should().BeEquivalentTo(defaultStructureValue);
            (await client.GetObjectValue(flagName, defaultStructureValue, EvaluationContext.Empty)).Should().BeEquivalentTo(defaultStructureValue);
            (await client.GetObjectValue(flagName, defaultStructureValue, EvaluationContext.Empty, emptyFlagOptions)).Should().BeEquivalentTo(defaultStructureValue);
        }

        [Fact]
        [Specification("1.4.1", "The `client` MUST provide methods for detailed flag value evaluation with parameters `flag key` (string, required), `default value` (boolean | number | string | structure, required), `evaluation context` (optional), and `evaluation options` (optional), which returns an `evaluation details` structure.")]
        [Specification("1.4.2", "The `evaluation details` structure's `value` field MUST contain the evaluated flag value.")]
        [Specification("1.4.3.1", "The `evaluation details` structure SHOULD accept a generic argument (or use an equivalent language feature) which indicates the type of the wrapped `value` field.")]
        [Specification("1.4.4", "The `evaluation details` structure's `flag key` field MUST contain the `flag key` argument passed to the detailed flag evaluation method.")]
        [Specification("1.4.5", "In cases of normal execution, the `evaluation details` structure's `variant` field MUST contain the value of the `variant` field in the `flag resolution` structure returned by the configured `provider`, if the field is set.")]
        [Specification("1.4.6", "In cases of normal execution, the `evaluation details` structure's `reason` field MUST contain the value of the `reason` field in the `flag resolution` structure returned by the configured `provider`, if the field is set.")]
        [Specification("1.4.11", "The `client` SHOULD provide asynchronous or non-blocking mechanisms for flag evaluation.")]
        [Specification("2.2.8.1", "The `resolution details` structure SHOULD accept a generic argument (or use an equivalent language feature) which indicates the type of the wrapped `value` field.")]
        public async Task OpenFeatureClient_Should_Allow_Details_Flag_Evaluation()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultBoolValue = fixture.Create<bool>();
            var defaultStringValue = fixture.Create<string>();
            var defaultIntegerValue = fixture.Create<int>();
            var defaultDoubleValue = fixture.Create<double>();
            var defaultStructureValue = fixture.Create<Value>();
            var emptyFlagOptions = new FlagEvaluationOptions(ImmutableList<Hook>.Empty, ImmutableDictionary<string, object>.Empty);

            await Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(clientName, clientVersion);

            var boolFlagEvaluationDetails = new FlagEvaluationDetails<bool>(flagName, defaultBoolValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await client.GetBooleanDetails(flagName, defaultBoolValue)).Should().BeEquivalentTo(boolFlagEvaluationDetails);
            (await client.GetBooleanDetails(flagName, defaultBoolValue, EvaluationContext.Empty)).Should().BeEquivalentTo(boolFlagEvaluationDetails);
            (await client.GetBooleanDetails(flagName, defaultBoolValue, EvaluationContext.Empty, emptyFlagOptions)).Should().BeEquivalentTo(boolFlagEvaluationDetails);

            var integerFlagEvaluationDetails = new FlagEvaluationDetails<int>(flagName, defaultIntegerValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await client.GetIntegerDetails(flagName, defaultIntegerValue)).Should().BeEquivalentTo(integerFlagEvaluationDetails);
            (await client.GetIntegerDetails(flagName, defaultIntegerValue, EvaluationContext.Empty)).Should().BeEquivalentTo(integerFlagEvaluationDetails);
            (await client.GetIntegerDetails(flagName, defaultIntegerValue, EvaluationContext.Empty, emptyFlagOptions)).Should().BeEquivalentTo(integerFlagEvaluationDetails);

            var doubleFlagEvaluationDetails = new FlagEvaluationDetails<double>(flagName, defaultDoubleValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await client.GetDoubleDetails(flagName, defaultDoubleValue)).Should().BeEquivalentTo(doubleFlagEvaluationDetails);
            (await client.GetDoubleDetails(flagName, defaultDoubleValue, EvaluationContext.Empty)).Should().BeEquivalentTo(doubleFlagEvaluationDetails);
            (await client.GetDoubleDetails(flagName, defaultDoubleValue, EvaluationContext.Empty, emptyFlagOptions)).Should().BeEquivalentTo(doubleFlagEvaluationDetails);

            var stringFlagEvaluationDetails = new FlagEvaluationDetails<string>(flagName, defaultStringValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await client.GetStringDetails(flagName, defaultStringValue)).Should().BeEquivalentTo(stringFlagEvaluationDetails);
            (await client.GetStringDetails(flagName, defaultStringValue, EvaluationContext.Empty)).Should().BeEquivalentTo(stringFlagEvaluationDetails);
            (await client.GetStringDetails(flagName, defaultStringValue, EvaluationContext.Empty, emptyFlagOptions)).Should().BeEquivalentTo(stringFlagEvaluationDetails);

            var structureFlagEvaluationDetails = new FlagEvaluationDetails<Value>(flagName, defaultStructureValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
            (await client.GetObjectDetails(flagName, defaultStructureValue)).Should().BeEquivalentTo(structureFlagEvaluationDetails);
            (await client.GetObjectDetails(flagName, defaultStructureValue, EvaluationContext.Empty)).Should().BeEquivalentTo(structureFlagEvaluationDetails);
            (await client.GetObjectDetails(flagName, defaultStructureValue, EvaluationContext.Empty, emptyFlagOptions)).Should().BeEquivalentTo(structureFlagEvaluationDetails);
        }

        [Fact]
        [Specification("1.1.2", "The `API` MUST provide a function to set the default `provider`, which accepts an API-conformant `provider` implementation.")]
        [Specification("1.3.3", "The `client` SHOULD guarantee the returned value of any typed flag evaluation method is of the expected type. If the value returned by the underlying provider implementation does not match the expected type, it's to be considered abnormal execution, and the supplied `default value` should be returned.")]
        [Specification("1.4.7", "In cases of abnormal execution, the `evaluation details` structure's `error code` field MUST contain an `error code`.")]
        [Specification("1.4.8", "In cases of abnormal execution (network failure, unhandled error, etc) the `reason` field in the `evaluation details` SHOULD indicate an error.")]
        [Specification("1.4.9", "Methods, functions, or operations on the client MUST NOT throw exceptions, or otherwise abnormally terminate. Flag evaluation calls must always return the `default value` in the event of abnormal execution. Exceptions include functions or methods for the purposes for configuration or setup.")]
        [Specification("1.4.10", "In the case of abnormal execution, the client SHOULD log an informative error message.")]
        public async Task OpenFeatureClient_Should_Return_DefaultValue_When_Type_Mismatch()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<Value>();
            var mockedFeatureProvider = Substitute.For<FeatureProvider>();
            var mockedLogger = Substitute.For<ILogger<Api>>();

            // This will fail to case a String to TestStructure
            mockedFeatureProvider.ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Throws<InvalidCastException>();
            mockedFeatureProvider.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            mockedFeatureProvider.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(mockedFeatureProvider);
            var client = Api.Instance.GetClient(clientName, clientVersion, mockedLogger);

            var evaluationDetails = await client.GetObjectDetails(flagName, defaultValue);
            evaluationDetails.ErrorType.Should().Be(ErrorType.TypeMismatch);

            _ = mockedFeatureProvider.Received(1).ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>());

            mockedLogger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<FormattedLogValues>(t => string.Equals($"Error while evaluating flag {flagName}", t.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task Should_Resolve_BooleanValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<bool>();

            var featureProviderMock = Substitute.For<FeatureProvider>();
            featureProviderMock.ResolveBooleanValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<bool>(flagName, defaultValue));
            featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(featureProviderMock);
            var client = Api.Instance.GetClient(clientName, clientVersion);

            (await client.GetBooleanValue(flagName, defaultValue)).Should().Be(defaultValue);

            _ = featureProviderMock.Received(1).ResolveBooleanValue(flagName, defaultValue, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public async Task Should_Resolve_StringValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<string>();

            var featureProviderMock = Substitute.For<FeatureProvider>();
            featureProviderMock.ResolveStringValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<string>(flagName, defaultValue));
            featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(featureProviderMock);
            var client = Api.Instance.GetClient(clientName, clientVersion);

            (await client.GetStringValue(flagName, defaultValue)).Should().Be(defaultValue);

            _ = featureProviderMock.Received(1).ResolveStringValue(flagName, defaultValue, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public async Task Should_Resolve_IntegerValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<int>();

            var featureProviderMock = Substitute.For<FeatureProvider>();
            featureProviderMock.ResolveIntegerValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<int>(flagName, defaultValue));
            featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(featureProviderMock);
            var client = Api.Instance.GetClient(clientName, clientVersion);

            (await client.GetIntegerValue(flagName, defaultValue)).Should().Be(defaultValue);

            _ = featureProviderMock.Received(1).ResolveIntegerValue(flagName, defaultValue, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public async Task Should_Resolve_DoubleValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<double>();

            var featureProviderMock = Substitute.For<FeatureProvider>();
            featureProviderMock.ResolveDoubleValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<double>(flagName, defaultValue));
            featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(featureProviderMock);
            var client = Api.Instance.GetClient(clientName, clientVersion);

            (await client.GetDoubleValue(flagName, defaultValue)).Should().Be(defaultValue);

            _ = featureProviderMock.Received(1).ResolveDoubleValue(flagName, defaultValue, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public async Task Should_Resolve_StructureValue()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<Value>();

            var featureProviderMock = Substitute.For<FeatureProvider>();
            featureProviderMock.ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<Value>(flagName, defaultValue));
            featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(featureProviderMock);
            var client = Api.Instance.GetClient(clientName, clientVersion);

            (await client.GetObjectValue(flagName, defaultValue)).Should().Be(defaultValue);

            _ = featureProviderMock.Received(1).ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public async Task When_Error_Is_Returned_From_Provider_Should_Return_Error()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<Value>();
            const string testMessage = "Couldn't parse flag data.";

            var featureProviderMock = Substitute.For<FeatureProvider>();
            featureProviderMock.ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(Task.FromResult(new ResolutionDetails<Value>(flagName, defaultValue, ErrorType.ParseError, "ERROR", null, testMessage)));
            featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(featureProviderMock);
            var client = Api.Instance.GetClient(clientName, clientVersion);
            var response = await client.GetObjectDetails(flagName, defaultValue);

            response.ErrorType.Should().Be(ErrorType.ParseError);
            response.Reason.Should().Be(Reason.Error);
            response.ErrorMessage.Should().Be(testMessage);
            _ = featureProviderMock.Received(1).ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public async Task When_Exception_Occurs_During_Evaluation_Should_Return_Error()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<Value>();
            const string testMessage = "Couldn't parse flag data.";

            var featureProviderMock = Substitute.For<FeatureProvider>();
            featureProviderMock.ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>()).Throws(new FeatureProviderException(ErrorType.ParseError, testMessage));
            featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
            featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            await Api.Instance.SetProvider(featureProviderMock);
            var client = Api.Instance.GetClient(clientName, clientVersion);
            var response = await client.GetObjectDetails(flagName, defaultValue);

            response.ErrorType.Should().Be(ErrorType.ParseError);
            response.Reason.Should().Be(Reason.Error);
            response.ErrorMessage.Should().Be(testMessage);
            _ = featureProviderMock.Received(1).ResolveStructureValue(flagName, defaultValue, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public async Task Should_Use_No_Op_When_Provider_Is_Null()
        {
            await Api.Instance.SetProvider(null);
            var client = new FeatureClient("test", "test");
            (await client.GetIntegerValue("some-key", 12)).Should().Be(12);
        }

        [Fact]
        public void Should_Get_And_Set_Context()
        {
            var KEY = "key";
            var VAL = 1;
            FeatureClient client = Api.Instance.GetClient();
            client.SetContext(new EvaluationContextBuilder().Set(KEY, VAL).Build());
            Assert.Equal(VAL, client.GetContext().GetValue(KEY).AsInteger);
        }
    }
}
