using System;
using AutoFixture;
using FluentAssertions;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
    public class OpenFeatureEvaluationContextTests
    {
        [Fact]
        public void ShouldMergeTwoContexts()
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
        public void ShouldMergeTwoContextsAndOverrideDuplicatesWithRightHandContext()
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
        }

        [Fact]
        public void ShouldReturnValueTypeViaGet()
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
