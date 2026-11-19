using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.DependencyInjection.Internal;

internal record EventHandlerDelegateWrapper(
    ProviderEventTypes ProviderEventType,
    EventHandlerDelegate EventHandlerDelegate);
