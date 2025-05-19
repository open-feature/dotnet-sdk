using AutoFixture;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;

namespace OpenFeature.Tests;

public class OpenFeatureEvaluationContextTests
{
    [Fact]
    public void Should_Merge_Two_Contexts()
    {
        var contextBuilder1 = new EvaluationContextBuilder()
            .Set("key1", "value1");
        var contextBuilder2 = new EvaluationContextBuilder()
            .Set("key2", "value2");
        var context1 = contextBuilder1.Merge(contextBuilder2.Build()).Build();

        Assert.Equal(2, context1.Count);
        Assert.Equal("value1", context1.GetValue("key1").AsString);
        Assert.Equal("value2", context1.GetValue("key2").AsString);
    }

    [Fact]
    public void Should_Change_TargetingKey_From_OverridingContext()
    {
        var contextBuilder1 = new EvaluationContextBuilder()
            .Set("key1", "value1")
            .SetTargetingKey("targeting_key");
        var contextBuilder2 = new EvaluationContextBuilder()
            .Set("key2", "value2")
            .SetTargetingKey("overriding_key");

        var mergeContext = contextBuilder1.Merge(contextBuilder2.Build()).Build();

        Assert.Equal("overriding_key", mergeContext.TargetingKey);
    }

    [Fact]
    public void Should_Retain_TargetingKey_When_OverridingContext_TargetingKey_Value_IsEmpty()
    {
        var contextBuilder1 = new EvaluationContextBuilder()
            .Set("key1", "value1")
            .SetTargetingKey("targeting_key");
        var contextBuilder2 = new EvaluationContextBuilder()
            .Set("key2", "value2");

        var mergeContext = contextBuilder1.Merge(contextBuilder2.Build()).Build();

        Assert.Equal("targeting_key", mergeContext.TargetingKey);
    }

    [Fact]
    [Specification("3.2.2", "Evaluation context MUST be merged in the order: API (global; lowest precedence) - client - invocation - before hooks (highest precedence), with duplicate values being overwritten.")]
    public void Should_Merge_TwoContexts_And_Override_Duplicates_With_RightHand_Context()
    {
        var contextBuilder1 = new EvaluationContextBuilder();
        var contextBuilder2 = new EvaluationContextBuilder();

        contextBuilder1.Set("key1", "value1");
        contextBuilder2.Set("key1", "overriden_value");
        contextBuilder2.Set("key2", "value2");

        var context1 = contextBuilder1.Merge(contextBuilder2.Build()).Build();

        Assert.Equal(2, context1.Count);
        Assert.Equal("overriden_value", context1.GetValue("key1").AsString);
        Assert.Equal("value2", context1.GetValue("key2").AsString);
    }

    [Fact]
    [Specification("3.1.1", "The `evaluation context` structure MUST define an optional `targeting key` field of type string, identifying the subject of the flag evaluation.")]
    [Specification("3.1.2", "The evaluation context MUST support the inclusion of custom fields, having keys of type `string`, and values of type `boolean | string | number | datetime | structure`.")]
    public void EvaluationContext_Should_All_Types()
    {
        var fixture = new Fixture();
        var now = fixture.Create<DateTime>();
        var structure = fixture.Create<Structure>();
        var contextBuilder = new EvaluationContextBuilder()
            .SetTargetingKey("targeting_key")
            .Set("targeting_key", "userId")
            .Set("key1", "value")
            .Set("key2", 1)
            .Set("key3", true)
            .Set("key4", now)
            .Set("key5", structure)
            .Set("key6", 1.0);

        var context = contextBuilder.Build();

        Assert.Equal("targeting_key", context.TargetingKey);
        var targetingKeyValue = context.GetValue(context.TargetingKey!);
        Assert.True(targetingKeyValue.IsString);
        Assert.Equal("userId", targetingKeyValue.AsString);

        var value1 = context.GetValue("key1");
        Assert.True(value1.IsString);
        Assert.Equal("value", value1.AsString);

        var value2 = context.GetValue("key2");
        Assert.True(value2.IsNumber);
        Assert.Equal(1, value2.AsInteger);

        var value3 = context.GetValue("key3");
        Assert.True(value3.IsBoolean);
        Assert.True(value3.AsBoolean);

        var value4 = context.GetValue("key4");
        Assert.True(value4.IsDateTime);
        Assert.Equal(now, value4.AsDateTime);

        var value5 = context.GetValue("key5");
        Assert.True(value5.IsStructure);
        Assert.Equal(structure, value5.AsStructure);

        var value6 = context.GetValue("key6");
        Assert.True(value6.IsNumber);
        Assert.Equal(1.0, value6.AsDouble);
    }

    [Fact]
    [Specification("3.1.4", "The evaluation context fields MUST have an unique key.")]
    public void When_Duplicate_Key_Set_It_Replaces_Value()
    {
        var contextBuilder = new EvaluationContextBuilder().Set("key", "value");
        contextBuilder.Set("key", "overriden_value");
        Assert.Equal("overriden_value", contextBuilder.Build().GetValue("key").AsString);
    }

    [Fact]
    [Specification("3.1.3", "The evaluation context MUST support fetching the custom fields by key and also fetching all key value pairs.")]
    public void Should_Be_Able_To_Get_All_Values()
    {
        var context = new EvaluationContextBuilder()
            .Set("key1", "value1")
            .Set("key2", "value2")
            .Set("key3", "value3")
            .Set("key4", "value4")
            .Set("key5", "value5")
            .Build();

        // Iterate over key value pairs and check consistency
        var count = 0;
        foreach (var keyValue in context)
        {
            Assert.Equal(keyValue.Value.AsString, context.GetValue(keyValue.Key).AsString);
            count++;
        }

        Assert.Equal(count, context.Count);
    }

    [Fact]
    public void TryGetValue_WhenCalledWithExistingKey_ReturnsTrueAndExpectedValue()
    {
        // Arrange
        var key = "testKey";
        var expectedValue = new Value("testValue");
        var structure = new Structure(new Dictionary<string, Value> { { key, expectedValue } });
        var evaluationContext = new EvaluationContext(structure);

        // Act
        var result = evaluationContext.TryGetValue(key, out var actualValue);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void GetValueOnTargetingKeySetWithTargetingKey_Equals_TargetingKey()
    {
        // Arrange
        var value = "my_targeting_key";
        var evaluationContext = EvaluationContext.Builder().SetTargetingKey(value).Build();

        // Act
        var result = evaluationContext.TryGetValue(EvaluationContext.TargetingKeyIndex, out var actualFromStructure);
        var actualFromTargetingKey = evaluationContext.TargetingKey;

        // Assert
        Assert.True(result);
        Assert.Equal(value, actualFromStructure?.AsString);
        Assert.Equal(value, actualFromTargetingKey);
    }

    [Fact]
    public void GetValueOnTargetingKeySetWithStructure_Equals_TargetingKey()
    {
        // Arrange
        var value = "my_targeting_key";
        var evaluationContext = EvaluationContext.Builder().Set(EvaluationContext.TargetingKeyIndex, new Value(value)).Build();

        // Act
        var result = evaluationContext.TryGetValue(EvaluationContext.TargetingKeyIndex, out var actualFromStructure);
        var actualFromTargetingKey = evaluationContext.TargetingKey;

        // Assert
        Assert.True(result);
        Assert.Equal(value, actualFromStructure?.AsString);
        Assert.Equal(value, actualFromTargetingKey);
    }

    [Fact]
    public void GetValueOnTargetingKeySetWithNonStringValue_Equals_Null()
    {
        // Arrange
        var evaluationContext = EvaluationContext.Builder().Set(EvaluationContext.TargetingKeyIndex, new Value(1)).Build();

        // Act
        var result = evaluationContext.TryGetValue(EvaluationContext.TargetingKeyIndex, out var actualFromStructure);
        var actualFromTargetingKey = evaluationContext.TargetingKey;

        // Assert
        Assert.True(result);
        Assert.Null(actualFromStructure?.AsString);
        Assert.Null(actualFromTargetingKey);
    }
}
