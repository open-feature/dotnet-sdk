using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Tests
{
    public class TestHookNoOverride : Hook
    {
    }

    public class TestHook : Hook
    {
        private int _beforeCallCount;
        public int BeforeCallCount { get => this._beforeCallCount; }

        private int _afterCallCount;
        public int AfterCallCount { get => this._afterCallCount; }

        private int _errorCallCount;
        public int ErrorCallCount { get => this._errorCallCount; }

        private int _finallyCallCount;
        public int FinallyCallCount { get => this._finallyCallCount; }

        public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context,
            IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref this._beforeCallCount);
            return new ValueTask<EvaluationContext>(EvaluationContext.Empty);
        }

        public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
            IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref this._afterCallCount);
            return new ValueTask();
        }

        public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error,
            IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref this._errorCallCount);
            return new ValueTask();
        }

        public override ValueTask FinallyAsync<T>(HookContext<T> context,
            IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref this._finallyCallCount);
            return new ValueTask();
        }
    }

    public class TestProvider : FeatureProvider
    {
        private readonly List<Hook> _hooks = new List<Hook>();

        public static string DefaultName = "test-provider";
        private readonly List<Tuple<string, EvaluationContext?, TrackingEventDetails?>> TrackingInvocations = [];

        public string? Name { get; set; }

        public void AddHook(Hook hook) => this._hooks.Add(hook);

        public override IImmutableList<Hook> GetProviderHooks() => this._hooks.ToImmutableList();
        private Exception? initException = null;
        private int initDelay = 0;

        public TestProvider()
        {
            this.Name = DefaultName;
        }

        /// <summary>
        /// A provider used for testing.
        /// </summary>
        /// <param name="name">the name of the provider.</param>
        /// <param name="initException">Optional exception to throw during init.</param>
        /// <para>
        public TestProvider(string? name, Exception? initException = null, int initDelay = 0)
        {
            this.Name = string.IsNullOrEmpty(name) ? DefaultName : name;
            this.initException = initException;
            this.initDelay = initDelay;
        }

        public ImmutableList<Tuple<string, EvaluationContext?, TrackingEventDetails?>> GetTrackingInvocations()
        {
            return this.TrackingInvocations.ToImmutableList();
        }

        public void Reset()
        {
            this.TrackingInvocations.Clear();
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

        public override async Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
        {
            await Task.Delay(this.initDelay).ConfigureAwait(false);
            if (this.initException != null)
            {
                throw this.initException;
            }
        }

        public override void Track(string trackingEventName, EvaluationContext? evaluationContext = default, TrackingEventDetails? trackingEventDetails = default)
        {
            this.TrackingInvocations.Add(new Tuple<string, EvaluationContext?, TrackingEventDetails?>(trackingEventName, evaluationContext, trackingEventDetails));
        }

        internal ValueTask SendEventAsync(ProviderEventTypes eventType, CancellationToken cancellationToken = default)
        {
            return this.EventChannel.Writer.WriteAsync(new ProviderEventPayload { Type = eventType, ProviderName = this.GetMetadata().Name, }, cancellationToken);
        }

        internal ValueTask SendEventAsync(ProviderEventPayload payload, CancellationToken cancellationToken = default)
        {
            return this.EventChannel.Writer.WriteAsync(payload, cancellationToken);
        }
    }
}
