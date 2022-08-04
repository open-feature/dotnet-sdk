using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeature.SDK.Model;

namespace OpenFeature.SDK.Tests
{
    public class TestStructure
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TestHookNoOverride : Hook { }

    public class TestHook : Hook
    {
        public override Task<EvaluationContext> Before<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.FromResult(new EvaluationContext());
        }

        public override Task After<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
            IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }

        public override Task Error<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }

        public override Task Finally<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.CompletedTask;
        }
    }

    public class TestProvider : IFeatureProvider
    {
        public static string Name => "test-provider";

        public Metadata GetMetadata()
        {
            return new Metadata(Name);
        }

        public Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue,
            EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }

        public Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue,
            EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }

        public Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue,
            EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }

        public Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue,
            EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }

        public Task<ResolutionDetails<T>> ResolveStructureValue<T>(string flagKey, T defaultValue,
            EvaluationContext context = null,
            FlagEvaluationOptions config = null)
        {
            throw new NotImplementedException();
        }
    }
}
