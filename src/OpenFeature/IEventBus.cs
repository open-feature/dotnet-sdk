using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{
    public interface IEventBus {
        void AddHandler(ProviderEventTypes type, EventHandlerDelegate handler);
        void RemoveHandler(ProviderEventTypes type, EventHandlerDelegate handler);
    }   
}