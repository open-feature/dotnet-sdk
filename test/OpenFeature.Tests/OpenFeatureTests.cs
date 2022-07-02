using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using OpenFeature.Constant;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests;

public class OpenFeatureTests
{
    [Fact]
    public void ShouldDefaultToNoOpProvider()
    {
        var fixture = new Fixture();
        var clientName = fixture.Create<string>();
        var clientVersion = fixture.Create<string>();
        
        var provider = OpenFeature.GetProvider().GetMetadata();
        var clientMetadata = OpenFeature.GetClient(clientName, clientVersion).GetMetadata();
        
        provider.Name.Should().Be(NoOpProvider.NoOpProviderName);
        clientMetadata.Name.Should().Be(clientName);
        clientMetadata.Version.Should().Be(clientVersion);
    }

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
        
        OpenFeature.GetContext().Should().Be(context);
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

    private class TestHook : IHook
    {
        public EvaluationContext Before<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            throw new NotImplementedException();
        }

        public void After<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object> hints = null)
        {
            throw new NotImplementedException();
        }

        public void Error<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object> hints = null)
        {
            throw new NotImplementedException();
        }

        public void Finally<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            throw new NotImplementedException();
        }
    }

    private class TestProvider : IFeatureProvider
    {
        public string Name => "test-provider";
        public Metadata GetMetadata()
        {
            return new Metadata(Name);
        }

        public ResolutionDetails<bool> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }

        public ResolutionDetails<string> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }

        public ResolutionDetails<int> ResolveNumberValue(string flagKey, int defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }

        public ResolutionDetails<T> ResolveStructureValue<T>(string flagKey, T defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }
    }
}