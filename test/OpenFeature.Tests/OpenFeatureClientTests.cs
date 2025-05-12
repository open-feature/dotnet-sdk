using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Extension;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests;

public class OpenFeatureClientTests : ClearOpenFeatureInstanceFixture
{
    [Fact]
    [Specification("1.2.1", "The client MUST provide a method to add `hooks` which accepts one or more API-conformant `hooks`, and appends them to the collection of any previously added hooks. When new hooks are added, previously added hooks are not removed.")]
    public void OpenFeatureClient_Should_Allow_Hooks()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var hook1 = Substitute.For<Hook>();
        var hook2 = Substitute.For<Hook>();
        var hook3 = Substitute.For<Hook>();

        var client = Api.Instance.GetClient(domain, clientVersion);

        client.AddHooks(new[] { hook1, hook2 });

        var expectedHooks = new[] { hook1, hook2 }.AsEnumerable();
        Assert.Equal(expectedHooks, client.GetHooks());

        client.AddHooks(hook3);

        expectedHooks = new[] { hook1, hook2, hook3 }.AsEnumerable();
        Assert.Equal(expectedHooks, client.GetHooks());

        client.ClearHooks();
        Assert.Empty(client.GetHooks());
    }

    [Fact]
    [Specification("1.2.2", "The client interface MUST define a `metadata` member or accessor, containing an immutable `name` field or accessor of type string, which corresponds to the `name` value supplied during client creation.")]
    public void OpenFeatureClient_Metadata_Should_Have_Name()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var client = Api.Instance.GetClient(domain, clientVersion);

        Assert.Equal(domain, client.GetMetadata().Name);
        Assert.Equal(clientVersion, client.GetMetadata().Version);
    }

    [Fact]
    [Specification("1.3.1", "The `client` MUST provide methods for typed flag evaluation, including boolean, numeric, string, and structure, with parameters `flag key` (string, required), `default value` (boolean | number | string | structure, required), `evaluation context` (optional), and `evaluation options` (optional), which returns the flag value.")]
    [Specification("1.3.2.1", "The client SHOULD provide functions for floating-point numbers and integers, consistent with language idioms.")]
    [Specification("1.3.3", "The `client` SHOULD guarantee the returned value of any typed flag evaluation method is of the expected type. If the value returned by the underlying provider implementation does not match the expected type, it's to be considered abnormal execution, and the supplied `default value` should be returned.")]
    public async Task OpenFeatureClient_Should_Allow_Flag_Evaluation()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultBoolValue = fixture.Create<bool>();
        var defaultStringValue = fixture.Create<string>();
        var defaultIntegerValue = fixture.Create<int>();
        var defaultDoubleValue = fixture.Create<double>();
        var defaultStructureValue = fixture.Create<Value>();
        var emptyFlagOptions = new FlagEvaluationOptions(ImmutableList<Hook>.Empty, ImmutableDictionary<string, object>.Empty);

        await Api.Instance.SetProviderAsync(new NoOpFeatureProvider());
        var client = Api.Instance.GetClient(domain, clientVersion);

        Assert.Equal(defaultBoolValue, await client.GetBooleanValueAsync(flagName, defaultBoolValue));
        Assert.Equal(defaultBoolValue, await client.GetBooleanValueAsync(flagName, defaultBoolValue, EvaluationContext.Empty));
        Assert.Equal(defaultBoolValue, await client.GetBooleanValueAsync(flagName, defaultBoolValue, EvaluationContext.Empty, emptyFlagOptions));

        Assert.Equal(defaultIntegerValue, await client.GetIntegerValueAsync(flagName, defaultIntegerValue));
        Assert.Equal(defaultIntegerValue, await client.GetIntegerValueAsync(flagName, defaultIntegerValue, EvaluationContext.Empty));
        Assert.Equal(defaultIntegerValue, await client.GetIntegerValueAsync(flagName, defaultIntegerValue, EvaluationContext.Empty, emptyFlagOptions));

        Assert.Equal(defaultDoubleValue, await client.GetDoubleValueAsync(flagName, defaultDoubleValue));
        Assert.Equal(defaultDoubleValue, await client.GetDoubleValueAsync(flagName, defaultDoubleValue, EvaluationContext.Empty));
        Assert.Equal(defaultDoubleValue, await client.GetDoubleValueAsync(flagName, defaultDoubleValue, EvaluationContext.Empty, emptyFlagOptions));

        Assert.Equal(defaultStringValue, await client.GetStringValueAsync(flagName, defaultStringValue));
        Assert.Equal(defaultStringValue, await client.GetStringValueAsync(flagName, defaultStringValue, EvaluationContext.Empty));
        Assert.Equal(defaultStringValue, await client.GetStringValueAsync(flagName, defaultStringValue, EvaluationContext.Empty, emptyFlagOptions));

        Assert.Equivalent(defaultStructureValue, await client.GetObjectValueAsync(flagName, defaultStructureValue));
        Assert.Equivalent(defaultStructureValue, await client.GetObjectValueAsync(flagName, defaultStructureValue, EvaluationContext.Empty));
        Assert.Equivalent(defaultStructureValue, await client.GetObjectValueAsync(flagName, defaultStructureValue, EvaluationContext.Empty, emptyFlagOptions));
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
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultBoolValue = fixture.Create<bool>();
        var defaultStringValue = fixture.Create<string>();
        var defaultIntegerValue = fixture.Create<int>();
        var defaultDoubleValue = fixture.Create<double>();
        var defaultStructureValue = fixture.Create<Value>();
        var emptyFlagOptions = new FlagEvaluationOptions(ImmutableList<Hook>.Empty, ImmutableDictionary<string, object>.Empty);

        await Api.Instance.SetProviderAsync(new NoOpFeatureProvider());
        var client = Api.Instance.GetClient(domain, clientVersion);

        var boolFlagEvaluationDetails = new FlagEvaluationDetails<bool>(flagName, defaultBoolValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
        Assert.Equivalent(boolFlagEvaluationDetails, await client.GetBooleanDetailsAsync(flagName, defaultBoolValue));
        Assert.Equivalent(boolFlagEvaluationDetails, await client.GetBooleanDetailsAsync(flagName, defaultBoolValue, EvaluationContext.Empty));
        Assert.Equivalent(boolFlagEvaluationDetails, await client.GetBooleanDetailsAsync(flagName, defaultBoolValue, EvaluationContext.Empty, emptyFlagOptions));

        var integerFlagEvaluationDetails = new FlagEvaluationDetails<int>(flagName, defaultIntegerValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
        Assert.Equivalent(integerFlagEvaluationDetails, await client.GetIntegerDetailsAsync(flagName, defaultIntegerValue));
        Assert.Equivalent(integerFlagEvaluationDetails, await client.GetIntegerDetailsAsync(flagName, defaultIntegerValue, EvaluationContext.Empty));
        Assert.Equivalent(integerFlagEvaluationDetails, await client.GetIntegerDetailsAsync(flagName, defaultIntegerValue, EvaluationContext.Empty, emptyFlagOptions));

        var doubleFlagEvaluationDetails = new FlagEvaluationDetails<double>(flagName, defaultDoubleValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
        Assert.Equivalent(doubleFlagEvaluationDetails, await client.GetDoubleDetailsAsync(flagName, defaultDoubleValue));
        Assert.Equivalent(doubleFlagEvaluationDetails, await client.GetDoubleDetailsAsync(flagName, defaultDoubleValue, EvaluationContext.Empty));
        Assert.Equivalent(doubleFlagEvaluationDetails, await client.GetDoubleDetailsAsync(flagName, defaultDoubleValue, EvaluationContext.Empty, emptyFlagOptions));

        var stringFlagEvaluationDetails = new FlagEvaluationDetails<string>(flagName, defaultStringValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
        Assert.Equivalent(stringFlagEvaluationDetails, await client.GetStringDetailsAsync(flagName, defaultStringValue));
        Assert.Equivalent(stringFlagEvaluationDetails, await client.GetStringDetailsAsync(flagName, defaultStringValue, EvaluationContext.Empty));
        Assert.Equivalent(stringFlagEvaluationDetails, await client.GetStringDetailsAsync(flagName, defaultStringValue, EvaluationContext.Empty, emptyFlagOptions));

        var structureFlagEvaluationDetails = new FlagEvaluationDetails<Value>(flagName, defaultStructureValue, ErrorType.None, NoOpProvider.ReasonNoOp, NoOpProvider.Variant);
        Assert.Equivalent(structureFlagEvaluationDetails, await client.GetObjectDetailsAsync(flagName, defaultStructureValue));
        Assert.Equivalent(structureFlagEvaluationDetails, await client.GetObjectDetailsAsync(flagName, defaultStructureValue, EvaluationContext.Empty));
        Assert.Equivalent(structureFlagEvaluationDetails, await client.GetObjectDetailsAsync(flagName, defaultStructureValue, EvaluationContext.Empty, emptyFlagOptions));
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
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<Value>();
        var mockedFeatureProvider = Substitute.For<FeatureProvider>();
        var mockedLogger = Substitute.For<ILogger<Api>>();

        // This will fail to case a String to TestStructure
        mockedFeatureProvider.ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Throws<InvalidCastException>();
        mockedFeatureProvider.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        mockedFeatureProvider.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(mockedFeatureProvider);
        var client = Api.Instance.GetClient(domain, clientVersion, mockedLogger);

        var evaluationDetails = await client.GetObjectDetailsAsync(flagName, defaultValue);
        Assert.Equal(ErrorType.TypeMismatch, evaluationDetails.ErrorType);
        Assert.Equal(new InvalidCastException().Message, evaluationDetails.ErrorMessage);

        _ = mockedFeatureProvider.Received(1).ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());

        mockedLogger.Received(0).IsEnabled(LogLevel.Error);
    }

    [Fact]
    [Specification("1.7.3", "The client's provider status accessor MUST indicate READY if the initialize function of the associated provider terminates normally.")]
    [Specification("1.7.1", "The client MUST define a provider status accessor which indicates the readiness of the associated provider, with possible values NOT_READY, READY, STALE, ERROR, or FATAL.")]
    public async Task Provider_Status_Should_Be_Ready_If_Init_Succeeds()
    {
        var name = "1.7.3";
        // provider which succeeds initialization
        var provider = new TestProvider();
        FeatureClient client = Api.Instance.GetClient(name);
        Assert.Equal(ProviderStatus.NotReady, provider.Status);
        await Api.Instance.SetProviderAsync(name, provider);

        // after init fails fatally, status should be READY
        Assert.Equal(ProviderStatus.Ready, client.ProviderStatus);
    }

    [Fact]
    [Specification("1.7.4", "The client's provider status accessor MUST indicate ERROR if the initialize function of the associated provider terminates abnormally.")]
    [Specification("1.7.1", "The client MUST define a provider status accessor which indicates the readiness of the associated provider, with possible values NOT_READY, READY, STALE, ERROR, or FATAL.")]
    public async Task Provider_Status_Should_Be_Error_If_Init_Fails()
    {
        var name = "1.7.4";
        // provider which fails initialization
        var provider = new TestProvider("some-name", new GeneralException("fake"));
        FeatureClient client = Api.Instance.GetClient(name);
        Assert.Equal(ProviderStatus.NotReady, provider.Status);
        await Api.Instance.SetProviderAsync(name, provider);

        // after init fails fatally, status should be ERROR
        Assert.Equal(ProviderStatus.Error, client.ProviderStatus);
    }

    [Fact]
    [Specification("1.7.5", "The client's provider status accessor MUST indicate FATAL if the initialize function of the associated provider terminates abnormally and indicates error code PROVIDER_FATAL.")]
    [Specification("1.7.1", "The client MUST define a provider status accessor which indicates the readiness of the associated provider, with possible values NOT_READY, READY, STALE, ERROR, or FATAL.")]
    public async Task Provider_Status_Should_Be_Fatal_If_Init_Fatal()
    {
        var name = "1.7.5";
        // provider which fails initialization fatally
        var provider = new TestProvider(name, new ProviderFatalException("fatal"));
        FeatureClient client = Api.Instance.GetClient(name);
        Assert.Equal(ProviderStatus.NotReady, provider.Status);
        await Api.Instance.SetProviderAsync(name, provider);

        // after init fails fatally, status should be FATAL
        Assert.Equal(ProviderStatus.Fatal, client.ProviderStatus);
    }

    [Fact]
    [Specification("1.7.6", "The client MUST default, run error hooks, and indicate an error if flag resolution is attempted while the provider is in NOT_READY.")]
    public async Task Must_Short_Circuit_Not_Ready()
    {
        var name = "1.7.6";
        var defaultStr = "123-default";

        // provider which is never ready (ready after maxValue)
        var provider = new TestProvider(name, null, int.MaxValue);
        FeatureClient client = Api.Instance.GetClient(name);
        Assert.Equal(ProviderStatus.NotReady, provider.Status);
        _ = Api.Instance.SetProviderAsync(name, provider);

        var details = await client.GetStringDetailsAsync("some-flag", defaultStr);
        Assert.Equal(defaultStr, details.Value);
        Assert.Equal(ErrorType.ProviderNotReady, details.ErrorType);
        Assert.Equal(Reason.Error, details.Reason);
    }

    [Fact]
    [Specification("1.7.7", "The client MUST default, run error hooks, and indicate an error if flag resolution is attempted while the provider is in NOT_READY.")]
    public async Task Must_Short_Circuit_Fatal()
    {
        var name = "1.7.6";
        var defaultStr = "456-default";

        // provider which immediately fails fatally
        var provider = new TestProvider(name, new ProviderFatalException("fake"));
        FeatureClient client = Api.Instance.GetClient(name);
        Assert.Equal(ProviderStatus.NotReady, provider.Status);
        _ = Api.Instance.SetProviderAsync(name, provider);

        var details = await client.GetStringDetailsAsync("some-flag", defaultStr);
        Assert.Equal(defaultStr, details.Value);
        Assert.Equal(ErrorType.ProviderFatal, details.ErrorType);
        Assert.Equal(Reason.Error, details.Reason);
    }

    [Fact]
    public async Task Should_Resolve_BooleanValue()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<bool>();

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveBooleanValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<bool>(flagName, defaultValue));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);

        Assert.Equal(defaultValue, await client.GetBooleanValueAsync(flagName, defaultValue));

        _ = featureProviderMock.Received(1).ResolveBooleanValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());
    }

    [Fact]
    public async Task Should_Resolve_StringValue()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<string>();

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveStringValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<string>(flagName, defaultValue));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);

        Assert.Equal(defaultValue, await client.GetStringValueAsync(flagName, defaultValue));

        _ = featureProviderMock.Received(1).ResolveStringValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());
    }

    [Fact]
    public async Task Should_Resolve_IntegerValue()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<int>();

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveIntegerValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<int>(flagName, defaultValue));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);

        Assert.Equal(defaultValue, await client.GetIntegerValueAsync(flagName, defaultValue));

        _ = featureProviderMock.Received(1).ResolveIntegerValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());
    }

    [Fact]
    public async Task Should_Resolve_DoubleValue()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<double>();

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveDoubleValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<double>(flagName, defaultValue));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);

        Assert.Equal(defaultValue, await client.GetDoubleValueAsync(flagName, defaultValue));

        _ = featureProviderMock.Received(1).ResolveDoubleValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());
    }

    [Fact]
    public async Task Should_Resolve_StructureValue()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<Value>();

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<Value>(flagName, defaultValue));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);

        Assert.Equal(defaultValue, await client.GetObjectValueAsync(flagName, defaultValue));

        _ = featureProviderMock.Received(1).ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());
    }

    [Fact]
    public async Task When_Error_Is_Returned_From_Provider_Should_Return_Error()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<Value>();
        const string testMessage = "Couldn't parse flag data.";

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Returns(Task.FromResult(new ResolutionDetails<Value>(flagName, defaultValue, ErrorType.ParseError, "ERROR", null, testMessage)));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);
        var response = await client.GetObjectDetailsAsync(flagName, defaultValue);

        Assert.Equal(ErrorType.ParseError, response.ErrorType);
        Assert.Equal(Reason.Error, response.Reason);
        Assert.Equal(testMessage, response.ErrorMessage);
        _ = featureProviderMock.Received(1).ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());
    }

    [Fact]
    public async Task When_Exception_Occurs_During_Evaluation_Should_Return_Error()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<Value>();
        const string testMessage = "Couldn't parse flag data.";

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>()).Throws(new FeatureProviderException(ErrorType.ParseError, testMessage));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);
        var response = await client.GetObjectDetailsAsync(flagName, defaultValue);

        Assert.Equal(ErrorType.ParseError, response.ErrorType);
        Assert.Equal(Reason.Error, response.Reason);
        Assert.Equal(testMessage, response.ErrorMessage);
        _ = featureProviderMock.Received(1).ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());
    }

    [Fact]
    public async Task When_Error_Is_Returned_From_Provider_Should_Not_Run_After_Hook_But_Error_Hook()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultValue = fixture.Create<Value>();
        const string testMessage = "Couldn't parse flag data.";

        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>())
            .Returns(Task.FromResult(new ResolutionDetails<Value>(flagName, defaultValue, ErrorType.ParseError,
                "ERROR", null, testMessage)));
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);
        var testHook = new TestHook();
        client.AddHooks(testHook);
        var response = await client.GetObjectDetailsAsync(flagName, defaultValue);

        Assert.Equal(ErrorType.ParseError, response.ErrorType);
        Assert.Equal(Reason.Error, response.Reason);
        Assert.Equal(testMessage, response.ErrorMessage);
        _ = featureProviderMock.Received(1)
            .ResolveStructureValueAsync(flagName, defaultValue, Arg.Any<EvaluationContext>());

        Assert.Equal(1, testHook.BeforeCallCount);
        Assert.Equal(0, testHook.AfterCallCount);
        Assert.Equal(1, testHook.ErrorCallCount);
        Assert.Equal(1, testHook.FinallyCallCount);
    }

    [Fact]
    public async Task Cancellation_Token_Added_Is_Passed_To_Provider()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var flagName = fixture.Create<string>();
        var defaultString = fixture.Create<string>();
        var cancelledReason = "cancelled";

        var cts = new CancellationTokenSource();


        var featureProviderMock = Substitute.For<FeatureProvider>();
        featureProviderMock.ResolveStringValueAsync(flagName, defaultString, Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>()).Returns(async args =>
        {
            var token = args.ArgAt<CancellationToken>(3);
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(10); // artificially delay until cancelled
            }

            return new ResolutionDetails<string>(flagName, defaultString, ErrorType.None, cancelledReason);
        });
        featureProviderMock.GetMetadata().Returns(new Metadata(fixture.Create<string>()));
        featureProviderMock.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

        await Api.Instance.SetProviderAsync(domain, featureProviderMock);
        var client = Api.Instance.GetClient(domain, clientVersion);
        var task = client.GetStringDetailsAsync(flagName, defaultString, EvaluationContext.Empty, null, cts.Token);
        cts.Cancel(); // cancel before awaiting

        var response = await task;
        Assert.Equal(defaultString, response.Value);
        Assert.Equal(cancelledReason, response.Reason);
        _ = featureProviderMock.Received(1).ResolveStringValueAsync(flagName, defaultString, Arg.Any<EvaluationContext>(), cts.Token);
    }

    [Fact]
    public void Should_Get_And_Set_Context()
    {
        var fixture = new Fixture();
        var domain = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        var KEY = "key";
        var VAL = 1;
        FeatureClient client = Api.Instance.GetClient(domain, clientVersion);
        client.SetContext(new EvaluationContextBuilder().Set(KEY, VAL).Build());
        Assert.Equal(VAL, client.GetContext().GetValue(KEY).AsInteger);
    }


    [Fact]
    public void ToFlagEvaluationDetails_Should_Convert_All_Properties()
    {
        var fixture = new Fixture();
        var flagName = fixture.Create<string>();
        var boolValue = fixture.Create<bool>();
        var errorType = fixture.Create<ErrorType>();
        var reason = fixture.Create<string>();
        var variant = fixture.Create<string>();
        var errorMessage = fixture.Create<string>();
        var flagData = fixture
            .CreateMany<KeyValuePair<string, object>>(10)
            .ToDictionary(x => x.Key, x => x.Value);
        var flagMetadata = new ImmutableMetadata(flagData);

        var expected = new ResolutionDetails<bool>(flagName, boolValue, errorType, reason, variant, errorMessage, flagMetadata);
        var result = expected.ToFlagEvaluationDetails();

        Assert.Equivalent(expected, result);
    }

    [Fact]
    [Specification("6.1.1", "The `client` MUST define a function for tracking the occurrence of a particular action or application state, with parameters `tracking event name` (string, required), `evaluation context` (optional) and `tracking event details` (optional), which returns nothing.")]
    [Specification("6.1.2", "The `client` MUST define a function for tracking the occurrence of a particular action or application state, with parameters `tracking event name` (string, required) and `tracking event details` (optional), which returns nothing.")]
    [Specification("6.1.4", "If the client's `track` function is called and the associated provider does not implement tracking, the client's `track` function MUST no-op.")]
    public async Task TheClient_ImplementsATrackingFunction()
    {
        var provider = new TestProvider();
        await Api.Instance.SetProviderAsync(provider);
        var client = Api.Instance.GetClient();

        const string trackingEventName = "trackingEventName";
        var trackingEventDetails = new TrackingEventDetailsBuilder().Build();
        client.Track(trackingEventName);
        client.Track(trackingEventName, EvaluationContext.Empty);
        client.Track(trackingEventName, EvaluationContext.Empty, trackingEventDetails);
        client.Track(trackingEventName, trackingEventDetails: trackingEventDetails);

        Assert.Equal(4, provider.GetTrackingInvocations().Count);
        Assert.Equal(trackingEventName, provider.GetTrackingInvocations()[0].Item1);
        Assert.Equal(trackingEventName, provider.GetTrackingInvocations()[1].Item1);
        Assert.Equal(trackingEventName, provider.GetTrackingInvocations()[2].Item1);
        Assert.Equal(trackingEventName, provider.GetTrackingInvocations()[3].Item1);

        Assert.NotNull(provider.GetTrackingInvocations()[0].Item2);
        Assert.NotNull(provider.GetTrackingInvocations()[1].Item2);
        Assert.NotNull(provider.GetTrackingInvocations()[2].Item2);
        Assert.NotNull(provider.GetTrackingInvocations()[3].Item2);

        Assert.Equal(EvaluationContext.Empty.Count, provider.GetTrackingInvocations()[0].Item2!.Count);
        Assert.Equal(EvaluationContext.Empty.Count, provider.GetTrackingInvocations()[1].Item2!.Count);
        Assert.Equal(EvaluationContext.Empty.Count, provider.GetTrackingInvocations()[2].Item2!.Count);
        Assert.Equal(EvaluationContext.Empty.Count, provider.GetTrackingInvocations()[3].Item2!.Count);

        Assert.Equal(EvaluationContext.Empty.TargetingKey, provider.GetTrackingInvocations()[0].Item2!.TargetingKey);
        Assert.Equal(EvaluationContext.Empty.TargetingKey, provider.GetTrackingInvocations()[1].Item2!.TargetingKey);
        Assert.Equal(EvaluationContext.Empty.TargetingKey, provider.GetTrackingInvocations()[2].Item2!.TargetingKey);
        Assert.Equal(EvaluationContext.Empty.TargetingKey, provider.GetTrackingInvocations()[3].Item2!.TargetingKey);

        Assert.Null(provider.GetTrackingInvocations()[0].Item3);
        Assert.Null(provider.GetTrackingInvocations()[1].Item3);
        Assert.Equal(trackingEventDetails, provider.GetTrackingInvocations()[2].Item3);
        Assert.Equal(trackingEventDetails, provider.GetTrackingInvocations()[3].Item3);
    }

    [Fact]
    public async Task PassingAnEmptyStringAsTrackingEventName_ThrowsArgumentException()
    {
        var provider = new TestProvider();
        await Api.Instance.SetProviderAsync(provider);
        var client = Api.Instance.GetClient();

        Assert.Throws<ArgumentException>(() => client.Track(""));
    }

    [Fact]
    public async Task PassingABlankStringAsTrackingEventName_ThrowsArgumentException()
    {
        var provider = new TestProvider();
        await Api.Instance.SetProviderAsync(provider);
        var client = Api.Instance.GetClient();

        Assert.Throws<ArgumentException>(() => client.Track(" \n "));
    }

    public static TheoryData<string, EvaluationContext?, EvaluationContext?, EvaluationContext?, string> GenerateMergeEvaluationContextTestData()
    {
        const string key = "key";
        const string global = "global";
        const string client = "client";
        const string invocation = "invocation";
        var globalEvaluationContext = new[] { new EvaluationContextBuilder().Set(key, "global").Build(), null };
        var clientEvaluationContext = new[] { new EvaluationContextBuilder().Set(key, "client").Build(), null };
        var invocationEvaluationContext = new[] { new EvaluationContextBuilder().Set(key, "invocation").Build(), null };

        var data = new TheoryData<string, EvaluationContext?, EvaluationContext?, EvaluationContext?, string>();
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    if (i == 1 && j == 1 && k == 1) continue;
                    string expected;
                    if (k == 0) expected = invocation;
                    else if (j == 0) expected = client;
                    else expected = global;
                    data.Add(key, globalEvaluationContext[i], clientEvaluationContext[j], invocationEvaluationContext[k], expected);
                }
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(GenerateMergeEvaluationContextTestData))]
    [Specification("6.1.3", "The evaluation context passed to the provider's track function MUST be merged in the order: API (global; lowest precedence) - transaction - client - invocation (highest precedence), with duplicate values being overwritten.")]
    public async Task TheClient_MergesTheEvaluationContextInTheCorrectOrder(string key, EvaluationContext? globalEvaluationContext, EvaluationContext? clientEvaluationContext, EvaluationContext? invocationEvaluationContext, string expectedResult)
    {
        var provider = new TestProvider();
        await Api.Instance.SetProviderAsync(provider);
        var client = Api.Instance.GetClient();

        const string trackingEventName = "trackingEventName";

        Api.Instance.SetContext(globalEvaluationContext);
        client.SetContext(clientEvaluationContext);
        client.Track(trackingEventName, invocationEvaluationContext);
        Assert.Single(provider.GetTrackingInvocations());
        var actualEvaluationContext = provider.GetTrackingInvocations()[0].Item2;
        Assert.NotNull(actualEvaluationContext);
        Assert.NotEqual(0, actualEvaluationContext.Count);

        Assert.Equal(expectedResult, actualEvaluationContext.GetValue(key).AsString);
    }

    [Fact]
    [Specification("4.3.8", "'evaluation details' passed to the 'finally' stage matches the evaluation details returned to the application author")]
    public async Task FinallyHook_IncludesEvaluationDetails()
    {
        // Arrange
        var provider = new TestProvider();
        var providerHook = Substitute.For<Hook>();
        provider.AddHook(providerHook);
        await Api.Instance.SetProviderAsync(provider);
        var client = Api.Instance.GetClient();

        const string flagName = "flagName";

        // Act
        var evaluationDetails = await client.GetBooleanDetailsAsync(flagName, true);

        // Assert
        await providerHook.Received(1).FinallyAsync(Arg.Any<HookContext<bool>>(), evaluationDetails);
    }
}
