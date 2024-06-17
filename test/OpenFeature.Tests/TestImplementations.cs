using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Tests
{
    public class TestHookNoOverride : Hook { }

    public class TestHook : Hook
    {
        public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            return new ValueTask<EvaluationContext>(EvaluationContext.Empty);
        }

        public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
            IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }

        public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }

        public override ValueTask FinallyAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }
    }

    public class TestProvider : FeatureProvider
    {
        private readonly List<Hook> _hooks = new List<Hook>();

        public static string DefaultName = "test-provider";

        public string Name { get; set; }

        private ProviderStatus _status;

        public void AddHook(Hook hook) => this._hooks.Add(hook);

        public override IImmutableList<Hook> GetProviderHooks() => this._hooks.ToImmutableList();

        public TestProvider()
        {
            this._status = ProviderStatus.NotReady;
            this.Name = DefaultName;
        }

        public TestProvider(string name)
        {
            this._status = ProviderStatus.NotReady;
            this.Name = name;
        }

        public override Metadata GetMetadata()
        {
            return new Metadata(this.Name);
        }

        public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ResolutionDetails<bool>(flagKey, !defaultValue));
        }

        public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ResolutionDetails<string>(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ResolutionDetails<int>(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ResolutionDetails<double>(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ResolutionDetails<Value>(flagKey, defaultValue));
        }

        public override ProviderStatus GetStatus()
        {
            return this._status;
        }

        public void SetStatus(ProviderStatus status)
        {
            this._status = status;
        }

        public override async Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
        {
            this._status = ProviderStatus.Ready;
            await this.EventChannel.Writer.WriteAsync(new ProviderEventPayload { Type = ProviderEventTypes.ProviderReady, ProviderName = this.GetMetadata().Name }, cancellationToken).ConfigureAwait(false);
            await base.InitializeAsync(context, cancellationToken).ConfigureAwait(false);
        }

        internal ValueTask SendEventAsync(ProviderEventTypes eventType, CancellationToken cancellationToken = default)
        {
            return this.EventChannel.Writer.WriteAsync(new ProviderEventPayload { Type = eventType, ProviderName = this.GetMetadata().Name }, cancellationToken);
        }
    }
}
