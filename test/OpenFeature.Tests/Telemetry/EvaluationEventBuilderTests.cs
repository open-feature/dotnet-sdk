using System.Collections.Generic;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Telemetry;
using Xunit;

namespace OpenFeature.Tests.Telemetry;

public class EvaluationEventBuilderTests
{
    private readonly EvaluationEventBuilder _builder = EvaluationEventBuilder.Default;

    [Fact]
    public void Build_ShouldReturnEventWithCorrectAttributes()
    {
        // Arrange
        var clientMetadata = new ClientMetadata("client", "1.0.0");
        var providerMetadata = new Metadata("provider");
        var hookContext = new HookContext<Value>("flagKey", new Value(), FlagValueType.Object, clientMetadata,
            providerMetadata, EvaluationContext.Empty);
        var metadata = new Dictionary<string, object>
        {
            { "flagSetId", "flagSetId" }, { "contextId", "contextId" }, { "version", "version" }
        };
        var flagMetadata = new ImmutableMetadata(metadata);
        var details = new FlagEvaluationDetails<Value>("flagKey", new Value("value"), ErrorType.None,
            reason: "reason", variant: "variant", flagMetadata: flagMetadata);

        // Act
        var evaluationEvent = _builder.Build(hookContext, details);

        // Assert
        Assert.Equal("feature_flag.evaluation", evaluationEvent.Name);
        Assert.Equal("flagKey", evaluationEvent.Attributes[TelemetryConstants.Key]);
        Assert.Equal("provider", evaluationEvent.Attributes[TelemetryConstants.Provider]);
        Assert.Equal("reason", evaluationEvent.Attributes[TelemetryConstants.Reason]);
        Assert.Equal("variant", evaluationEvent.Attributes[TelemetryConstants.Variant]);
        Assert.Equal("contextId", evaluationEvent.Attributes[TelemetryConstants.ContextId]);
        Assert.Equal("flagSetId", evaluationEvent.Attributes[TelemetryConstants.FlagSetId]);
        Assert.Equal("version", evaluationEvent.Attributes[TelemetryConstants.Version]);
    }

    [Fact]
    public void Build_ShouldHandleErrorDetails()
    {
        // Arrange
        var clientMetadata = new ClientMetadata("client", "1.0.0");
        var providerMetadata = new Metadata("provider");
        var hookContext = new HookContext<Value>("flagKey", new Value(), FlagValueType.Object, clientMetadata,
            providerMetadata, EvaluationContext.Empty);
        var metadata = new Dictionary<string, object>
        {
            { "flagSetId", "flagSetId" }, { "contextId", "contextId" }, { "version", "version" }
        };
        var flagMetadata = new ImmutableMetadata(metadata);
        var details = new FlagEvaluationDetails<Value>("flagKey", new Value("value"), ErrorType.General,
            errorMessage: "errorMessage", reason: "reason", variant: "variant", flagMetadata: flagMetadata);

        // Act
        var evaluationEvent = _builder.Build(hookContext, details);

        // Assert
        Assert.Equal("general", evaluationEvent.Attributes[TelemetryConstants.ErrorCode]);
        Assert.Equal("errorMessage", evaluationEvent.Attributes[TelemetryConstants.ErrorMessage]);
    }

    [Fact]
    public void Build_ShouldHandleMissingVariant()
    {
        // Arrange
        var clientMetadata = new ClientMetadata("client", "1.0.0");
        var providerMetadata = new Metadata("provider");
        var hookContext = new HookContext<Value>("flagKey", new Value("value"), FlagValueType.Object, clientMetadata,
            providerMetadata, EvaluationContext.Empty);
        var metadata = new Dictionary<string, object>
        {
            { "flagSetId", "flagSetId" }, { "contextId", "contextId" }, { "version", "version" }
        };
        var flagMetadata = new ImmutableMetadata(metadata);
        var details = new FlagEvaluationDetails<Value>("flagKey", new Value("value"), ErrorType.None,
            reason: "reason", variant: null, flagMetadata: flagMetadata);

        // Act
        var evaluationEvent = _builder.Build(hookContext, details);

        // Assert
        Assert.Null(evaluationEvent.Attributes[TelemetryConstants.Variant]);
    }

    [Fact]
    public void Build_ShouldHandleMissingFlagMetadata()
    {
        // Arrange
        var clientMetadata = new ClientMetadata("client", "1.0.0");
        var providerMetadata = new Metadata("provider");
        var hookContext = new HookContext<Value>("flagKey", new Value("value"), FlagValueType.Object, clientMetadata,
            providerMetadata, EvaluationContext.Empty);
        var flagMetadata = new ImmutableMetadata();
        var details = new FlagEvaluationDetails<Value>("flagKey", new Value("value"), ErrorType.None,
            reason: "reason", variant: "", flagMetadata: flagMetadata);

        // Act
        var evaluationEvent = _builder.Build(hookContext, details);

        // Assert
        Assert.Null(evaluationEvent.Attributes[TelemetryConstants.ContextId]);
        Assert.Null(evaluationEvent.Attributes[TelemetryConstants.FlagSetId]);
        Assert.Null(evaluationEvent.Attributes[TelemetryConstants.Version]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Build_ShouldHandleMissingReason(string? reason)
    {
        // Arrange
        var clientMetadata = new ClientMetadata("client", "1.0.0");
        var providerMetadata = new Metadata("provider");
        var hookContext = new HookContext<Value>("flagKey", new Value("value"), FlagValueType.Object, clientMetadata,
            providerMetadata, EvaluationContext.Empty);
        var flagMetadata = new ImmutableMetadata();
        var details = new FlagEvaluationDetails<Value>("flagKey", new Value("value"), ErrorType.None,
            reason: reason, variant: "", flagMetadata: flagMetadata);

        // Act
        var evaluationEvent = _builder.Build(hookContext, details);

        // Assert
        Assert.Equal(Reason.Unknown.ToLowerInvariant(), evaluationEvent.Attributes[TelemetryConstants.Reason]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Build_ShouldHandleErrorWithEmptyErrorMessage(string? errorMessage)
    {
        // Arrange
        var clientMetadata = new ClientMetadata("client", "1.0.0");
        var providerMetadata = new Metadata("provider");
        var hookContext = new HookContext<Value>("flagKey", new Value("value"), FlagValueType.Object, clientMetadata,
            providerMetadata, EvaluationContext.Empty);
        var flagMetadata = new ImmutableMetadata();
        var details = new FlagEvaluationDetails<Value>("flagKey", new Value("value"), ErrorType.General,
            errorMessage: errorMessage, reason: "reason", variant: "", flagMetadata: flagMetadata);

        // Act
        var evaluationEvent = _builder.Build(hookContext, details);

        // Assert
        Assert.Equal("general", evaluationEvent.Attributes[TelemetryConstants.ErrorCode]);
        Assert.False(evaluationEvent.Attributes.ContainsKey(TelemetryConstants.ErrorMessage));
    }

    [Fact]
    public void Build_ShouldIncludeValueAttributeInEvent()
    {
        // Arrange
        var clientMetadata = new ClientMetadata("client", "1.0.0");
        var providerMetadata = new Metadata("provider");
        var hookContext = new HookContext<Value>("flagKey", new Value(), FlagValueType.Object, clientMetadata,
            providerMetadata, EvaluationContext.Empty);
        var testValue = new Value("test-value");
        var details = new FlagEvaluationDetails<Value>("flagKey", testValue, ErrorType.None,
            reason: "reason", variant: "variant", flagMetadata: new ImmutableMetadata());

        // Act
        var evaluationEvent = _builder.Build(hookContext, details);

        // Assert
        Assert.Equal(testValue, evaluationEvent.Attributes[TelemetryConstants.Value]);
    }
}
