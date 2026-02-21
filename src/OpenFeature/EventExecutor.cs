using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature;

internal sealed partial class EventExecutor : IAsyncDisposable
{
    private readonly object _lockObj = new();
    public readonly Channel<object> EventChannel = Channel.CreateBounded<object>(1);
    private FeatureProvider? _defaultProvider;
    private readonly Dictionary<string, FeatureProvider> _namedProviderReferences = [];
    private readonly List<FeatureProvider> _activeSubscriptions = [];

    /// placeholder for anonymous clients
    private static readonly Guid _defaultClientName = Guid.NewGuid();

    private readonly Dictionary<ProviderEventTypes, List<EventHandlerDelegate>> _apiHandlers = [];
    private readonly Dictionary<string, Dictionary<ProviderEventTypes, List<EventHandlerDelegate>>> _clientHandlers = [];

    private ILogger _logger;

    public EventExecutor()
    {
        this._logger = NullLogger<EventExecutor>.Instance;
        Task.Run(this.ProcessEventAsync);
    }

    public ValueTask DisposeAsync() => new(this.ShutdownAsync());

    internal void SetLogger(ILogger logger) => this._logger = logger;

    internal void AddApiLevelHandler(ProviderEventTypes eventType, EventHandlerDelegate handler)
    {
        lock (this._lockObj)
        {
            if (!this._apiHandlers.TryGetValue(eventType, out var eventHandlers))
            {
                eventHandlers = [];
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
        var clientName = GetClientName(client);

        lock (this._lockObj)
        {
            // check if there is already a list of handlers for the given client and event type
            if (!this._clientHandlers.TryGetValue(clientName, out var registry))
            {
                registry = [];
                this._clientHandlers[clientName] = registry;
            }

            if (!this._clientHandlers[clientName].TryGetValue(eventType, out var eventHandlers))
            {
                eventHandlers = [];
                this._clientHandlers[clientName][eventType] = eventHandlers;
            }

            this._clientHandlers[clientName][eventType].Add(handler);

            this.EmitOnRegistration(
                this._namedProviderReferences.TryGetValue(clientName, out var clientProviderReference)
                    ? clientProviderReference
                    : this._defaultProvider, eventType, handler);
        }
    }

    internal void RemoveClientHandler(string client, ProviderEventTypes type, EventHandlerDelegate handler)
    {
        var clientName = GetClientName(client);

        lock (this._lockObj)
        {
            if (this._clientHandlers.TryGetValue(clientName, out var clientEventHandlers)
                    && clientEventHandlers.TryGetValue(type, out var eventHandlers))
            {
                eventHandlers.Remove(handler);
            }
        }
    }

    internal void RegisterDefaultFeatureProvider(FeatureProvider? provider)
    {
        if (provider == null)
        {
            return;
        }
        lock (this._lockObj)
        {
            var oldProvider = this._defaultProvider;

            this._defaultProvider = provider;

            this.StartListeningAndShutdownOld(this._defaultProvider, oldProvider);
        }
    }

    internal void RegisterClientFeatureProvider(string client, FeatureProvider? provider)
    {
        if (provider == null)
        {
            return;
        }

        var clientName = GetClientName(client);

        lock (this._lockObj)
        {
            FeatureProvider? oldProvider = null;
            if (this._namedProviderReferences.TryGetValue(clientName, out var foundOldProvider))
            {
                oldProvider = foundOldProvider;
            }

            this._namedProviderReferences[clientName] = provider;

            this.StartListeningAndShutdownOld(provider, oldProvider);
        }
    }

    private void StartListeningAndShutdownOld(FeatureProvider newProvider, FeatureProvider? oldProvider)
    {
        // check if the provider is already active - if not, we need to start listening for its emitted events
        if (!this.IsProviderActive(newProvider))
        {
            this._activeSubscriptions.Add(newProvider);
            Task.Run(() => this.ProcessFeatureProviderEventsAsync(newProvider));
        }

        if (oldProvider != null && !this.IsProviderBound(oldProvider))
        {
            this._activeSubscriptions.Remove(oldProvider);
            oldProvider.GetEventChannel().Writer.Complete();
        }
    }

    private bool IsProviderBound(FeatureProvider provider)
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

    private bool IsProviderActive(FeatureProvider providerRef)
    {
        return this._activeSubscriptions.Contains(providerRef);
    }

    private void EmitOnRegistration(FeatureProvider? provider, ProviderEventTypes eventType, EventHandlerDelegate handler)
    {
        if (provider == null)
        {
            return;
        }
        var status = provider.Status;

        var message = status switch
        {
            ProviderStatus.Ready when eventType == ProviderEventTypes.ProviderReady => "Provider is ready",
            ProviderStatus.Error when eventType == ProviderEventTypes.ProviderError => "Provider is in error state",
            ProviderStatus.Stale when eventType == ProviderEventTypes.ProviderStale => "Provider is in stale state",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            handler.Invoke(new ProviderEventPayload
            {
                ProviderName = provider.GetMetadata()?.Name,
                Type = eventType,
                Message = message
            });
        }
        catch (Exception exc)
        {
            this.ErrorRunningHandler(exc);
        }
    }

    private async Task ProcessFeatureProviderEventsAsync(FeatureProvider provider)
    {
        if (provider.GetEventChannel() is not { Reader: { } reader })
        {
            return;
        }

        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            if (!reader.TryRead(out var item))
                continue;

            switch (item)
            {
                case ProviderEventPayload eventPayload:
                    UpdateProviderStatus(provider, eventPayload);
                    await this.EventChannel.Writer.WriteAsync(new Event { Provider = provider, EventPayload = eventPayload }).ConfigureAwait(false);
                    break;
            }
        }
    }

    // Method to process events
    private async Task ProcessEventAsync()
    {
        while (await this.EventChannel.Reader.WaitToReadAsync().ConfigureAwait(false))
        {
            if (!this.EventChannel.Reader.TryRead(out var item))
            {
                continue;
            }

            if (item is not Event e)
            {
                continue;
            }

            lock (this._lockObj)
            {
                this.ProcessApiHandlers(e);
                this.ProcessClientHandlers(e);
                this.ProcessDefaultProviderHandlers(e);
            }
        }
    }

    private void ProcessApiHandlers(Event e)
    {
        if (e.EventPayload?.Type != null && this._apiHandlers.TryGetValue(e.EventPayload.Type, out var eventHandlers))
        {
            foreach (var eventHandler in eventHandlers)
            {
                this.InvokeEventHandler(eventHandler, e);
            }
        }
    }

    private void ProcessClientHandlers(Event e)
    {
        foreach (var keyAndValue in this._namedProviderReferences)
        {
            if (keyAndValue.Value == e.Provider
                && this._clientHandlers.TryGetValue(keyAndValue.Key, out var clientRegistry)
                && e.EventPayload?.Type != null
                && clientRegistry.TryGetValue(e.EventPayload.Type, out var clientEventHandlers))
            {
                foreach (var eventHandler in clientEventHandlers)
                {
                    this.InvokeEventHandler(eventHandler, e);
                }
            }
        }
    }

    private void ProcessDefaultProviderHandlers(Event e)
    {
        if (e.Provider != this._defaultProvider)
        {
            return;
        }

        foreach (var keyAndValues in this._clientHandlers)
        {
            if (this._namedProviderReferences.ContainsKey(keyAndValues.Key))
            {
                continue;
            }

            if (e.EventPayload?.Type != null && keyAndValues.Value.TryGetValue(e.EventPayload.Type, out var clientEventHandlers))
            {
                foreach (var eventHandler in clientEventHandlers)
                {
                    this.InvokeEventHandler(eventHandler, e);
                }
            }
        }
    }

    private static string GetClientName(string client)
    {
        if (string.IsNullOrWhiteSpace(client))
        {
            return _defaultClientName.ToString();
        }
        return client;
    }

    // map events to provider status as per spec: https://openfeature.dev/specification/sections/events/#requirement-535
    private static void UpdateProviderStatus(FeatureProvider provider, ProviderEventPayload eventPayload)
    {
        switch (eventPayload.Type)
        {
            case ProviderEventTypes.ProviderReady:
                provider.Status = ProviderStatus.Ready;
                break;
            case ProviderEventTypes.ProviderStale:
                provider.Status = ProviderStatus.Stale;
                break;
            case ProviderEventTypes.ProviderError:
                provider.Status = eventPayload.ErrorType == ErrorType.ProviderFatal ? ProviderStatus.Fatal : ProviderStatus.Error;
                break;
            case ProviderEventTypes.ProviderConfigurationChanged:
            default: break;
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
            this.ErrorRunningHandler(exc);
        }
    }

    public async Task ShutdownAsync()
    {
        this.EventChannel.Writer.Complete();
        await this.EventChannel.Reader.Completion.ConfigureAwait(false);
    }

    [LoggerMessage(100, LogLevel.Error, "Error running handler")]
    partial void ErrorRunningHandler(Exception exception);
}

internal class Event
{
    internal FeatureProvider? Provider { get; set; }
    internal ProviderEventPayload? EventPayload { get; set; }
}
