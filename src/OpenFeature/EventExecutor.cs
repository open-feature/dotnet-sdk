using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{
    [SuppressMessage("warning", "CS4014")]
    internal class EventExecutor
    {
        private readonly Mutex _mutex = new Mutex();
        public readonly Channel<object> EventChannel = Channel.CreateBounded<object>(1);
        private FeatureProviderReference _defaultProvider;
        private readonly Dictionary<string, FeatureProviderReference> _namedProviderReferences = new Dictionary<string, FeatureProviderReference>();
        private readonly List<FeatureProviderReference> _activeSubscriptions = new List<FeatureProviderReference>();
        private readonly SemaphoreSlim _shutdownSemaphore = new SemaphoreSlim(0);

        private readonly Dictionary<ProviderEventTypes, List<EventHandlerDelegate>> _apiHandlers = new Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>();
        private readonly Dictionary<string, Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>> _clientHandlers = new Dictionary<string, Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>>();

        public EventExecutor()
        {
            this.ProcessEventAsync();
        }

        internal void AddApiLevelHandler(ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            this._mutex.WaitOne();
            if (!this._apiHandlers.TryGetValue(eventType, out var eventHandlers))
            {
                eventHandlers = new List<EventHandlerDelegate>();
                this._apiHandlers[eventType] = eventHandlers;
            }

            eventHandlers.Add(handler);

            this.EmitOnRegistration(this._defaultProvider, eventType, handler);
            this._mutex.ReleaseMutex();
        }

        internal void RemoveApiLevelHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            this._mutex.WaitOne();
            if (this._apiHandlers.TryGetValue(type, out var eventHandlers))
            {
                eventHandlers.Remove(handler);
            }
            this._mutex.ReleaseMutex();
        }

        internal void AddClientHandler(string client, ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            this._mutex.WaitOne();
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

            if (this._namedProviderReferences.TryGetValue(client, out var clientProviderReference))
            {
                this.EmitOnRegistration(clientProviderReference, eventType, handler);
            }
            this._mutex.ReleaseMutex();
        }

        internal void RemoveClientHandler(string client, ProviderEventTypes type, EventHandlerDelegate handler)
        {
            this._mutex.WaitOne();
            if (this._clientHandlers.TryGetValue(client, out var clientEventHandlers))
            {
                if (clientEventHandlers.TryGetValue(type, out var eventHandlers))
                {
                    eventHandlers.Remove(handler);
                }
            }
            this._mutex.ReleaseMutex();
        }

        internal void RegisterDefaultFeatureProvider(FeatureProvider provider)
        {
            if (provider == null)
            {
                return;
            }
            this._mutex.WaitOne();
            var oldProvider = this._defaultProvider;

            this._defaultProvider = new FeatureProviderReference(provider);

            this.StartListeningAndShutdownOld(this._defaultProvider, oldProvider);
            this._mutex.ReleaseMutex();
        }

        internal void RegisterClientFeatureProvider(string client, FeatureProvider provider)
        {
            this._mutex.WaitOne();
            var newProvider = new FeatureProviderReference(provider);
            FeatureProviderReference oldProvider = null;
            if (this._namedProviderReferences.TryGetValue(client, out var foundOldProvider))
            {
                oldProvider = foundOldProvider;
            }

            this._namedProviderReferences[client] = newProvider;

            this.StartListeningAndShutdownOld(newProvider, oldProvider);
            this._mutex.ReleaseMutex();
        }

        private void StartListeningAndShutdownOld(FeatureProviderReference newProvider, FeatureProviderReference oldProvider)
        {
            // check if the provider is already active - if not, we need to start listening for its emitted events
            if (!this.IsProviderActive(newProvider))
            {
                this._activeSubscriptions.Add(newProvider);
                this.ProcessFeatureProviderEventsAsync(newProvider);
            }

            if (oldProvider != null && !this.IsProviderBound(oldProvider))
            {
                this._activeSubscriptions.Remove(oldProvider);
                if (oldProvider.Provider == null)
                {
                    Console.WriteLine("wtf");
                }
                var channel = oldProvider.Provider.GetEventChannel();
                if (channel != null)
                {
                    channel.Writer.TryWrite(new ShutdownSignal());
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
                handler(new ProviderEventPayload
                {
                    ProviderName = provider.Provider.GetMetadata().Name,
                    Type = eventType,
                    Message = message
                });
            }
        }

        private async Task ProcessFeatureProviderEventsAsync(FeatureProviderReference providerRef)
        {
            while (true)
            {
                var item = await providerRef.Provider.GetEventChannel().Reader.ReadAsync().ConfigureAwait(false);

                switch (item)
                {
                    case ProviderEventPayload eventPayload:
                        this.EventChannel.Writer.TryWrite(new Event { Provider = providerRef, EventPayload = eventPayload });
                        break;
                    case ShutdownSignal _:
                        providerRef.ShutdownSemaphore.Release();
                        return;
                }
            }
        }

        // Method to process events
        private async Task ProcessEventAsync()
        {
            while (true)
            {
                var item = await this.EventChannel.Reader.ReadAsync().ConfigureAwait(false);

                switch (item)
                {
                    case Event e:
                        this._mutex.WaitOne();
                        if (this._apiHandlers.TryGetValue(e.EventPayload.Type, out var eventHandlers))
                        {
                            foreach (var eventHandler in eventHandlers)
                            {
                                eventHandler.Invoke(e.EventPayload);
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
                                            eventHandler.Invoke(e.EventPayload);
                                        }
                                    }
                                }
                            }
                        }
                        this._mutex.ReleaseMutex();
                        break;
                    case ShutdownSignal _:
                        this._shutdownSemaphore.Release();
                        return;
                }

            }
        }

        // Method to signal shutdown
        public async Task SignalShutdownAsync()
        {
            // Enqueue a shutdown signal
            this.EventChannel.Writer.TryWrite(new ShutdownSignal());

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
