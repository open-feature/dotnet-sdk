using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Hosting.Internal;

internal record EventHandlerDelegateWrapper(
    ProviderEventTypes ProviderEventType,
    EventHandlerDelegate EventHandlerDelegate);
