using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using OpenFeature.SDK.Model;
using OpenFeature.SDK.Tests.Internal;
using Xunit;

namespace OpenFeature.SDK.Tests
{
    public class OpenFeatureEvaluationContextTests
    {
        [Fact]
        public void Should_Merge_Two_Contexts()
        {
            var context1 = new EvaluationContext()
                .Add("key1", "value1");
            var context2 = new EvaluationContext()
                .Add("key2", "value2");

            context1.Merge(context2);

            Assert.Equal(2, context1.Count);
            Assert.Equal("value1", context1.GetValue("key1").AsString);
            Assert.Equal("value2", context1.GetValue("key2").AsString);
        }

        [Fact]
        [Specification("3.2.2", "Duplicate values being overwritten.")]
        public void Should_Merge_TwoContexts_And_Override_Duplicates_With_RightHand_Context()
        {
            var context1 = new EvaluationContext();
            var context2 = new EvaluationContext();

            context1.Add("key1", "value1");
            context2.Add("key1", "overriden_value");
            context2.Add("key2", "value2");

            context1.Merge(context2);

            Assert.Equal(2, context1.Count);
            Assert.Equal("overriden_value", context1.GetValue("key1").AsString);
            Assert.Equal("value2", context1.GetValue("key2").AsString);

            context1.Remove("key1");
            Assert.Throws<KeyNotFoundException>(() => context1.GetValue("key1"));
        }

        [Fact]
        [Specification("3.1", "The `evaluation context` structure MUST define an optional `targeting key` field of type string, identifying the subject of the flag evaluation.")]
        [Specification("3.2", "The evaluation context MUST support the inclusion of custom fields, having keys of type `string`, and values of type `boolean | string | number | datetime | structure`.")]
        public void EvaluationContext_Should_All_Types()
        {
            var fixture = new Fixture();
            var now = fixture.Create<DateTime>();
            var structure = fixture.Create<Structure>();
            var context = new EvaluationContext()
                .Add("key1", "value")
                .Add("key2", 1)
                .Add("key3", true)
                .Add("key4", now)
                .Add("key5", structure)
                .Add("key6", 1.0);

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
        public void When_Duplicate_Key_Throw_Unique_Constraint()
        {
            var context = new EvaluationContext().Add("key", "value");
            var exception = Assert.Throws<ArgumentException>(() =>
                context.Add("key", "overriden_value"));
            exception.Message.Should().StartWith("An item with the same key has already been added.");
        }

        [Fact]
        [Specification("3.1.3", "The evaluation context MUST support fetching the custom fields by key and also fetching all key value pairs.")]
        public void Should_Be_Able_To_Get_All_Values()
        {
            var context = new EvaluationContext()
                .Add("key1", "value1")
                .Add("key2", "value2")
                .Add("key3", "value3")
                .Add("key4", "value4")
                .Add("key5", "value5");

            // Iterate over key value pairs and check consistency
            var count = 0;
            foreach (var keyValue in context)
            {
                context.GetValue(keyValue.Key).AsString.Should().Be(keyValue.Value.AsString);
                count++;
            }

            context.Count.Should().Be(count);
        }
    }
}
