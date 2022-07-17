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
            var context1 = new EvaluationContext();
            var context2 = new EvaluationContext();

            context1.Add("key1", "value1");
            context2.Add("key2", "value2");

            context1.Merge(context2);

            Assert.Equal(2, context1.Count);
            Assert.Equal("value1", context1["key1"]);
            Assert.Equal("value2", context1["key2"]);
        }

        [Fact]
        public void Should_Merge_TwoContexts_And_Override_Duplicates_With_RightHand_Context()
        {
            var context1 = new EvaluationContext();
            var context2 = new EvaluationContext();

            context1.Add("key1", "value1");
            context2.Add("key1", "overriden_value");
            context2.Add("key2", "value2");

            context1.Merge(context2);

            Assert.Equal(2, context1.Count);
            Assert.Equal("overriden_value", context1["key1"]);
            Assert.Equal("value2", context1["key2"]);

            context1.Remove("key1");
            Assert.Throws<KeyNotFoundException>(() => context1["key1"]);
        }

        [Fact]
        public void Should_Be_Able_To_Set_Value_Via_Indexer()
        {
            var context = new EvaluationContext();
            context["key"] = "value";
            context["key"].Should().Be("value");
        }

        [Fact]
        [Specification("3.1", "The `evaluation context` structure MUST define an optional `targeting key` field of type string, identifying the subject of the flag evaluation.")]
        [Specification("3.2", "The evaluation context MUST support the inclusion of custom fields, having keys of type `string`, and values of type `boolean | string | number | datetime | structure`.")]
        public void EvaluationContext_Should_All_Types()
        {
            var fixture = new Fixture();
            var now = fixture.Create<DateTime>();
            var structure = fixture.Create<TestStructure>();
            var context = new EvaluationContext
            {
                { "key1", "value" },
                { "key2", 1 },
                { "key3", true },
                { "key4", now },
                { "key5", structure}
            };

            context.Get<string>("key1").Should().Be("value");
            context.Get<int>("key2").Should().Be(1);
            context.Get<bool>("key3").Should().Be(true);
            context.Get<DateTime>("key4").Should().Be(now);
            context.Get<TestStructure>("key5").Should().Be(structure);
        }
    }
}
