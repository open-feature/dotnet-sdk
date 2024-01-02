using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{
    public interface IEventBus
    {
        /// <summary>
        /// Adds an Event Handler for the given event type.
        /// </summary>
        /// <param name="type">The type of the event</param>
        /// <param name="handler">Implementation of the <see cref="EventHandlerDelegate"/></param>
        void AddHandler(ProviderEventTypes type, EventHandlerDelegate handler);
        /// <summary>
        /// Removes an Event Handler for the given event type.
        /// </summary>
        /// <param name="type">The type of the event</param>
        /// <param name="handler">Implementation of the <see cref="EventHandlerDelegate"/></param>
        void RemoveHandler(ProviderEventTypes type, EventHandlerDelegate handler);
    }
}
