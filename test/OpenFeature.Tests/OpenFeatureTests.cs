using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenFeature.Constant;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
    public class OpenFeatureTests
    {
        [Fact]
        public void ShouldSetGivenProvider()
        {
            var provider = new TestProvider();

            OpenFeature.SetProvider(provider);

            OpenFeature.GetProvider().GetMetadata().Name.Should().Be(provider.Name);
            OpenFeature.GetProviderMetadata().Name.Should().Be(provider.Name);
        }

        [Fact]
        public void ShouldSetGivenContext()
        {
            var fixture = new Fixture();
            var context = fixture.Create<EvaluationContext>();

            OpenFeature.SetContext(context);

            OpenFeature.GetContext().Should().Equal(context);
        }

        [Fact]
        public void ShouldAddGivenHooks()
        {
            var fixture = new Fixture();
            var hooks = fixture.Create<List<TestHook>>();

            OpenFeature.AddHooks(hooks);

            OpenFeature.GetHooks().Should().Contain(hooks);

            OpenFeature.ClearHooks();

            OpenFeature.GetHooks().Should().BeEmpty();
        }
    }
}
