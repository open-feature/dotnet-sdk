using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
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

            context.TargetingKey.Should().Be("targeting_key");
            var targetingKeyValue = context.GetValue(context.TargetingKey);
            targetingKeyValue.IsString.Should().BeTrue();
            targetingKeyValue.AsString.Should().Be("userId");

            var value1 = context.GetValue("key1");
            value1.IsString.Should().BeTrue();
            value1.AsString.Should().Be("value");

            var value2 = context.GetValue("key2");
            value2.IsNumber.Should().BeTrue();
            value2.AsInteger.Should().Be(1);

            var value3 = context.GetValue("key3");
            value3.IsBoolean.Should().Be(true);
            value3.AsBoolean.Should().Be(true);

            var value4 = context.GetValue("key4");
            value4.IsDateTime.Should().BeTrue();
            value4.AsDateTime.Should().Be(now);

            var value5 = context.GetValue("key5");
            value5.IsStructure.Should().BeTrue();
            value5.AsStructure.Should().Equal(structure);

            var value6 = context.GetValue("key6");
            value6.IsNumber.Should().BeTrue();
            value6.AsDouble.Should().Be(1.0);
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
                context.GetValue(keyValue.Key).AsString.Should().Be(keyValue.Value.AsString);
                count++;
            }

            context.Count.Should().Be(count);
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
}
