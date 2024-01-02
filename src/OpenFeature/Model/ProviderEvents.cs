using System.Collections.Generic;
using OpenFeature.Constant;

namespace OpenFeature.Model
{
    public delegate void EventHandlerDelegate(ProviderEventPayload eventDetails);

    public class ProviderEventPayload
    {
        /// <summary>
        /// Name of the provider
        /// </summary>
        public string ProviderName { get; set; }
        public ProviderEventTypes Type { get; set; }
        public string Message { get; set; }
        public List<string> FlagChanges { get; set; }
        public Dictionary<string, object> EventMetadata { get; set; }
    }
}
