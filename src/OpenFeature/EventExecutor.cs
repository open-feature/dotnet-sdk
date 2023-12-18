
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
        private FeatureProviderReference defaultProvider;
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
            await this.shutdownSemaphore.WaitAsync().ConfigureAwait(false);
        }
    }

    public class ShutdownSignal
    {
    }

    internal class FeatureProviderReference
    {
        private Channel<Boolean> _shutdownChannel;
        private readonly SemaphoreSlim _shutdownSemaphore = new SemaphoreSlim(0);

        public FeatureProviderReference()
        {
            this._shutdownChannel = Channel.CreateBounded<Boolean>(new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }
    }
}
