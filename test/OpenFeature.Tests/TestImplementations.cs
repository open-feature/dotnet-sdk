using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Tests
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

        private ProviderStatus _status;

        public void AddHook(Hook hook) => this._hooks.Add(hook);

        public override IImmutableList<Hook> GetProviderHooks() => this._hooks.ToImmutableList();

        public TestProvider() => this._status = ProviderStatus.NotReady;

        public override Metadata GetMetadata()
        {
            return new Metadata(Name);
        }

        public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue,
            EvaluationContext context = null)
        {
            return Task.FromResult(new ResolutionDetails<bool>(flagKey, !defaultValue));
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

        public override ProviderStatus GetStatus()
        {
            return this._status;
        }

        public override Task Initialize(EvaluationContext context)
        {
            this._status = ProviderStatus.Ready;
            this.EventChannel.Writer.TryWrite(new ProviderEventPayload { Type = ProviderEventTypes.ProviderReady, ProviderName = this.GetMetadata().Name});
            return base.Initialize(context);
        }

        internal void SendEvent(ProviderEventTypes eventType)
        {
            this.EventChannel.Writer.TryWrite(new ProviderEventPayload { Type = eventType, ProviderName = this.GetMetadata().Name });
        }
    }
}
