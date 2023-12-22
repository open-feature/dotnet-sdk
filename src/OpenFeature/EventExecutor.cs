
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{
    public class EventExecutor
    {
        public readonly Channel<object> eventChannel = Channel.CreateBounded<object>(1);
        private FeatureProviderReference _defaultProvider;
        private readonly Dictionary<string, FeatureProviderReference> _namedProviderReferences = new Dictionary<string, FeatureProviderReference>();
        private readonly List<FeatureProviderReference> _activeSubscriptions = new List<FeatureProviderReference>();
        private readonly SemaphoreSlim _shutdownSemaphore = new SemaphoreSlim(0);

        private readonly Dictionary<ProviderEventTypes, List<EventHandlerDelegate>> _apiLevelHandlers = new Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>();
        private readonly Dictionary<string, Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>> _scopedApiHandlers = new Dictionary<string, Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>>();

        public EventExecutor()
        {
            this.ProcessEventAsync();
        }
        
        internal void AddApiLevelHandler(ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            if (!this._apiLevelHandlers.TryGetValue(eventType, out var eventHandlers))
            {
                eventHandlers = new List<EventHandlerDelegate>();
                this._apiLevelHandlers[eventType] = eventHandlers;
            }
            
            eventHandlers.Add(handler);

            this.EmitOnRegistration(this._defaultProvider, eventType, handler);
        }
        
        internal void RemoveGlobalHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            if (this._apiLevelHandlers.TryGetValue(type, out var eventHandlers))
            {
                eventHandlers.Remove(handler);
            }
        }
        
        internal void AddNamedHandler(string client, ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            // check if there is already a list of handlers for the given client and event type
            if (!this._scopedApiHandlers.TryGetValue(client, out var registry))
            {
                registry = new Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>();
                this._scopedApiHandlers[client] = registry;
            }

            if (!this._scopedApiHandlers[client].TryGetValue(eventType, out var eventHandlers))
            {
                eventHandlers = new List<EventHandlerDelegate>();
                this._scopedApiHandlers[client][eventType] = eventHandlers;
            }

            this._scopedApiHandlers[client][eventType].Add(handler);

            if (this._namedProviderReferences.TryGetValue(client, out var clientProviderReference))
            {
                this.EmitOnRegistration(clientProviderReference, eventType, handler);
            }
        }

        internal void RegisterDefaultFeatureProvider(FeatureProvider provider)
        {
            var oldProvider = this._defaultProvider;

            this._defaultProvider = new FeatureProviderReference(provider);

            this.StartListeningAndShutdownOld(this._defaultProvider, oldProvider);
        }

        internal void RegisterClientFeatureProvider(string client, FeatureProvider provider)
        {
            var newProvider = new FeatureProviderReference(provider);
            FeatureProviderReference oldProvider = null;
            if (this._namedProviderReferences.TryGetValue(client, out var foundOldProvider))
            {
                oldProvider = foundOldProvider;
            }

            this._namedProviderReferences.Add(client, newProvider);

            this.StartListeningAndShutdownOld(newProvider, oldProvider);
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
                oldProvider.Provider.GetEventChannel().Writer.TryWrite(new ShutdownSignal());
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
            if (status == ProviderStatus.Ready && eventType == ProviderEventTypes.PROVIDER_READY)
            {
                message = "Provider is ready";
            } else if (status == ProviderStatus.Error && eventType == ProviderEventTypes.PROVIDER_ERROR)
            {
                message = "Provider is in error state";
            } else if (status == ProviderStatus.Stale && eventType == ProviderEventTypes.PROVIDER_STALE)
            {
                message = "Provider is in stale state";
            }

            if (message != "")
            {
                handler.Invoke(new ProviderEventPayload
                {
                    ProviderName = provider.ToString(),
                    Type = eventType,
                    Message = message,
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
                        this.eventChannel.Writer.TryWrite(new Event{ Provider = providerRef, EventPayload = eventPayload });
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
               var item = await this.eventChannel.Reader.ReadAsync().ConfigureAwait(false);
               
               switch (item)
               {
                   case Event e:
                       if (this._apiLevelHandlers.TryGetValue(e.EventPayload.Type, out var eventHandlers))
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
                               if (this._scopedApiHandlers.TryGetValue(keyAndValue.Key, out var clientRegistry))
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
            this.eventChannel.Writer.TryWrite(new ShutdownSignal());

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
