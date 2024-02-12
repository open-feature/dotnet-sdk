using System.Collections.Generic;
using OpenFeature.Constant;

namespace OpenFeature.Model
{
    /// <summary>
    /// The EventHandlerDelegate is an implementation of an Event Handler
    /// </summary>
    public delegate void EventHandlerDelegate(ProviderEventPayload eventDetails);

    /// <summary>
    /// Contains the payload of an OpenFeature Event.
    /// </summary>
    public class ProviderEventPayload
    {
        /// <summary>
        /// Name of the provider.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Type of the event
        /// </summary>
        public ProviderEventTypes Type { get; set; }

        /// <summary>
        /// A message providing more information about the event.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A List of flags that have been changed.
        /// </summary>
        public List<string> FlagsChanged { get; set; }

        /// <summary>
        /// Metadata information for the event.
        /// </summary>
        // TODO: This needs to be changed to a EventMetadata object
        public Dictionary<string, object> EventMetadata { get; set; }
    }
}
