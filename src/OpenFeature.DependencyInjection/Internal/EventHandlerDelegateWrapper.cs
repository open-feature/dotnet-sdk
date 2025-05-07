using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.DependencyInjection.Internal;

internal record EventHandlerDelegateWrapper
{
    public ProviderEventTypes ProviderEventType { get; }

    public EventHandlerDelegate EventHandlerDelegate { get; }

    public EventHandlerDelegateWrapper(ProviderEventTypes providerEventTypes, EventHandlerDelegate eventHandlerDelegate)
    {
        ProviderEventType = providerEventTypes;
        EventHandlerDelegate = eventHandlerDelegate;
    }
}
