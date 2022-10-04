using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeatureSDK.Model;

namespace OpenFeatureSDK.Tests
{
    public class TestHookNoOverride : Hook { }

    public class TestHook : Hook
    {
        public override Task<EvaluationContext> Before<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
        {
            return Task.FromResult(EvaluationContext.Empty);
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

    public class TestProvider : FeatureProvider
    {
        private readonly List<Hook> _hooks = new List<Hook>();

        public static string Name => "test-provider";

        public void AddHook(Hook hook) => this._hooks.Add(hook);

        public override IReadOnlyList<Hook> GetProviderHooks() => this._hooks.AsReadOnly();

        public override Metadata GetMetadata()
        {
            return new Metadata(Name);
        }

        public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue,
            EvaluationContext context = null)
        {
            return Task.FromResult(new ResolutionDetails<bool>(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue,
            EvaluationContext context = null)
        {
            return Task.FromResult(new ResolutionDetails<string>(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue,
            EvaluationContext context = null)
        {
            return Task.FromResult(new ResolutionDetails<int>(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue,
            EvaluationContext context = null)
        {
            return Task.FromResult(new ResolutionDetails<double>(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue,
            EvaluationContext context = null)
        {
            return Task.FromResult(new ResolutionDetails<Value>(flagKey, defaultValue));
        }
    }
}
