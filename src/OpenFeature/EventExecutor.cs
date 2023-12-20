
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
        private readonly SemaphoreSlim _shutdownSemaphore = new SemaphoreSlim(0);
        private readonly Dictionary<ProviderEventTypes, List<EventHandlerDelegate>> _apiLevelHandlers = new Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>();
        
        public EventExecutor()
        {
            this.ProcessEventAsync();
        }
        
        public void AddGlobalHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            if (!this._apiLevelHandlers.TryGetValue(type, out var eventHandlers))
            {
                eventHandlers = new List<EventHandlerDelegate>();
                this._apiLevelHandlers[type] = eventHandlers;
            }
            
            eventHandlers.Add(handler);
        }
        
        public void RemoveGlobalHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            if (this._apiLevelHandlers.TryGetValue(type, out var eventHandlers))
            {
                eventHandlers.Remove(handler);
            }
        }
        
        internal void RegisterDefaultFeatureProvider(FeatureProvider provider)
        {
            if (this._defaultProvider != null)
            {
                this._defaultProvider.Provider.GetEventChannel().Writer.TryWrite(new ShutdownSignal());
            }
            this._defaultProvider = new FeatureProviderReference(provider);
            this.ProcessFeatureProviderEventsAsync(this._defaultProvider);
        }

        private async Task ProcessFeatureProviderEventsAsync(FeatureProviderReference providerRef)
        {
            while (true)
            {
                var item = await providerRef.Provider.GetEventChannel().Reader.ReadAsync().ConfigureAwait(false);

                switch (item)
                {
                    case ProviderEventPayload eventPayload:
                        // TODO encapsulate eventPayload into object containing the feature provider as well
                        this.eventChannel.Writer.TryWrite(eventPayload);
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
                   case ProviderEventPayload eventPayload:
                       if (this._apiLevelHandlers.TryGetValue(eventPayload.Type, out var eventHandlers))
                       {
                           foreach (var eventHandler in eventHandlers)
                           {
                               eventHandler.Invoke(eventPayload);
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

    public class ShutdownSignal
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
}
