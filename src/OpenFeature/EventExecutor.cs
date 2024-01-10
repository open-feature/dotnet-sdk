using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{

    internal delegate Task ShutdownDelegate();

    internal class EventExecutor
    {
        private readonly object _lockObj = new object();
        public readonly Channel<object> EventChannel = Channel.CreateBounded<object>(1);
        private FeatureProviderReference _defaultProvider;
        private readonly Dictionary<string, FeatureProviderReference> _namedProviderReferences = new Dictionary<string, FeatureProviderReference>();
        private readonly List<FeatureProviderReference> _activeSubscriptions = new List<FeatureProviderReference>();
        private readonly SemaphoreSlim _shutdownSemaphore = new SemaphoreSlim(0);

        private ShutdownDelegate _shutdownDelegate;

        private readonly Dictionary<ProviderEventTypes, List<EventHandlerDelegate>> _apiHandlers = new Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>();
        private readonly Dictionary<string, Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>> _clientHandlers = new Dictionary<string, Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>>();

        internal ILogger Logger { get; set; }

        public EventExecutor()
        {
            this.Logger = new Logger<EventExecutor>(new NullLoggerFactory());
            this._shutdownDelegate = this.SignalShutdownAsync;
            var eventProcessing = new Thread(this.ProcessEventAsync);
            eventProcessing.Start();
        }

        internal void AddApiLevelHandler(ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            lock (this._lockObj)
            {
                if (!this._apiHandlers.TryGetValue(eventType, out var eventHandlers))
                {
                    eventHandlers = new List<EventHandlerDelegate>();
                    this._apiHandlers[eventType] = eventHandlers;
                }

                eventHandlers.Add(handler);

                this.EmitOnRegistration(this._defaultProvider, eventType, handler);
            }
        }

        internal void RemoveApiLevelHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            lock (this._lockObj)
            {
                if (this._apiHandlers.TryGetValue(type, out var eventHandlers))
                {
                    eventHandlers.Remove(handler);
                }
            }
        }

        internal void AddClientHandler(string client, ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            lock (this._lockObj)
            {
                // check if there is already a list of handlers for the given client and event type
                if (!this._clientHandlers.TryGetValue(client, out var registry))
                {
                    registry = new Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>();
                    this._clientHandlers[client] = registry;
                }

                if (!this._clientHandlers[client].TryGetValue(eventType, out var eventHandlers))
                {
                    eventHandlers = new List<EventHandlerDelegate>();
                    this._clientHandlers[client][eventType] = eventHandlers;
                }

                this._clientHandlers[client][eventType].Add(handler);

                this.EmitOnRegistration(
                    this._namedProviderReferences.TryGetValue(client, out var clientProviderReference)
                        ? clientProviderReference
                        : this._defaultProvider, eventType, handler);
            }
        }

        internal void RemoveClientHandler(string client, ProviderEventTypes type, EventHandlerDelegate handler)
        {
            lock (this._lockObj)
            {
                if (this._clientHandlers.TryGetValue(client, out var clientEventHandlers))
                {
                    if (clientEventHandlers.TryGetValue(type, out var eventHandlers))
                    {
                        eventHandlers.Remove(handler);
                    }
                }
            }
        }

        internal void RegisterDefaultFeatureProvider(FeatureProvider provider)
        {
            if (provider == null)
            {
                return;
            }
            lock (this._lockObj)
            {
                var oldProvider = this._defaultProvider;

                this._defaultProvider = new FeatureProviderReference(provider);

                this.StartListeningAndShutdownOld(this._defaultProvider, oldProvider);
            }
        }

        internal void RegisterClientFeatureProvider(string client, FeatureProvider provider)
        {
            if (provider == null)
            {
                return;
            }
            lock (this._lockObj)
            {
                var newProvider = new FeatureProviderReference(provider);
                FeatureProviderReference oldProvider = null;
                if (this._namedProviderReferences.TryGetValue(client, out var foundOldProvider))
                {
                    oldProvider = foundOldProvider;
                }

                this._namedProviderReferences[client] = newProvider;

                this.StartListeningAndShutdownOld(newProvider, oldProvider);
            }
        }

        private void StartListeningAndShutdownOld(FeatureProviderReference newProvider, FeatureProviderReference oldProvider)
        {
            // check if the provider is already active - if not, we need to start listening for its emitted events
            if (!this.IsProviderActive(newProvider))
            {
                this._activeSubscriptions.Add(newProvider);
                var featureProviderEventProcessing = new Thread(this.ProcessFeatureProviderEventsAsync);
                featureProviderEventProcessing.Start(newProvider);
            }

            if (oldProvider != null && !this.IsProviderBound(oldProvider))
            {
                this._activeSubscriptions.Remove(oldProvider);
                var channel = oldProvider.Provider.GetEventChannel();
                if (channel != null)
                {
                    channel.Writer.WriteAsync(new ShutdownSignal());
                }
            }
        }

        private bool IsProviderBound(FeatureProviderReference provider)
        {
            if (this._defaultProvider == provider)
            {
                return true;
            }
            foreach (var providerReference in this._namedProviderReferences.Values)
            {
                if (providerReference == provider)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsProviderActive(FeatureProviderReference providerRef)
        {
            return this._activeSubscriptions.Contains(providerRef);
        }

        private void EmitOnRegistration(FeatureProviderReference provider, ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            if (provider == null)
            {
                return;
            }
            var status = provider.Provider.GetStatus();

            var message = "";
            if (status == ProviderStatus.Ready && eventType == ProviderEventTypes.ProviderReady)
            {
                message = "Provider is ready";
            }
            else if (status == ProviderStatus.Error && eventType == ProviderEventTypes.ProviderError)
            {
                message = "Provider is in error state";
            }
            else if (status == ProviderStatus.Stale && eventType == ProviderEventTypes.ProviderStale)
            {
                message = "Provider is in stale state";
            }

            if (message != "")
            {
                try
                {
                    handler.Invoke(new ProviderEventPayload
                    {
                        ProviderName = provider.Provider?.GetMetadata()?.Name,
                        Type = eventType,
                        Message = message
                    });
                }
                catch (Exception exc)
                {
                    this.Logger?.LogError("Error running handler: " + exc);
                }
            }
        }

        private async void ProcessFeatureProviderEventsAsync(object providerRef)
        {
            while (true)
            {
                var typedProviderRef = (FeatureProviderReference)providerRef;
                if (typedProviderRef.Provider.GetEventChannel() == null)
                {
                    return;
                }
                var item = await typedProviderRef.Provider.GetEventChannel().Reader.ReadAsync().ConfigureAwait(false);

                switch (item)
                {
                    case ProviderEventPayload eventPayload:
                        await this.EventChannel.Writer.WriteAsync(new Event { Provider = typedProviderRef, EventPayload = eventPayload }).ConfigureAwait(false);
                        break;
                    case ShutdownSignal _:
                        typedProviderRef.ShutdownSemaphore.Release();
                        return;
                }
            }
        }

        // Method to process events
        private async void ProcessEventAsync()
        {
            while (true)
            {
                var item = await this.EventChannel.Reader.ReadAsync().ConfigureAwait(false);

                switch (item)
                {
                    case Event e:
                        lock (this._lockObj)
                        {
                            if (this._apiHandlers.TryGetValue(e.EventPayload.Type, out var eventHandlers))
                            {
                                foreach (var eventHandler in eventHandlers)
                                {
                                    this.InvokeEventHandler(eventHandler, e);
                                }
                            }

                            // look for client handlers and call invoke method there
                            foreach (var keyAndValue in this._namedProviderReferences)
                            {
                                if (keyAndValue.Value == e.Provider)
                                {
                                    if (this._clientHandlers.TryGetValue(keyAndValue.Key, out var clientRegistry))
                                    {
                                        if (clientRegistry.TryGetValue(e.EventPayload.Type, out var clientEventHandlers))
                                        {
                                            foreach (var eventHandler in clientEventHandlers)
                                            {
                                                this.InvokeEventHandler(eventHandler, e);
                                            }
                                        }
                                    }
                                }
                            }

                            if (e.Provider != this._defaultProvider)
                            {
                                break;
                            }
                            // handling the default provider - invoke event handlers for clients which are not bound
                            // to a particular feature provider
                            foreach (var keyAndValues in this._clientHandlers)
                            {
                                if (this._namedProviderReferences.TryGetValue(keyAndValues.Key, out _))
                                {
                                    // if there is an association for the client to a specific feature provider, then continue
                                    continue;
                                }
                                if (keyAndValues.Value.TryGetValue(e.EventPayload.Type, out var clientEventHandlers))
                                {
                                    foreach (var eventHandler in clientEventHandlers)
                                    {
                                        this.InvokeEventHandler(eventHandler, e);
                                    }
                                }
                            }
                        }
                        break;
                    case ShutdownSignal _:
                        this._shutdownSemaphore.Release();
                        return;
                }

            }
        }

        private void InvokeEventHandler(EventHandlerDelegate eventHandler, Event e)
        {
            try
            {
                eventHandler.Invoke(e.EventPayload);
            }
            catch (Exception exc)
            {
                this.Logger?.LogError("Error running handler: " + exc);
            }
        }

        public async Task Shutdown()
        {
            await this._shutdownDelegate().ConfigureAwait(false);
        }

        internal void SetShutdownDelegate(ShutdownDelegate del)
        {
            this._shutdownDelegate = del;
        }

        // Method to signal shutdown
        private async Task SignalShutdownAsync()
        {
            // Enqueue a shutdown signal
            await this.EventChannel.Writer.WriteAsync(new ShutdownSignal()).ConfigureAwait(false);

            // Wait for the processing loop to acknowledge the shutdown
            await this._shutdownSemaphore.WaitAsync().ConfigureAwait(false);
        }
    }

    internal class ShutdownSignal
    {
    }

    internal class FeatureProviderReference
    {
        internal readonly SemaphoreSlim ShutdownSemaphore = new SemaphoreSlim(0);
        internal FeatureProvider Provider { get; }

        public FeatureProviderReference(FeatureProvider provider)
        {
            this.Provider = provider;
        }
    }

    internal class Event
    {
        internal FeatureProviderReference Provider { get; set; }
        internal ProviderEventPayload EventPayload { get; set; }
    }
}
