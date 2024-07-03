using System.ComponentModel;

namespace OpenFeature.Constant
{
    /// <summary>
    /// The state of the provider.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/sections/02-providers.md#requirement-242" />
    public enum ProviderStatus
    {
        /// <summary>
        /// The provider has not been initialized and cannot yet evaluate flags.
        /// </summary>
        [Description("NOT_READY")] NotReady,

        /// <summary>
        /// The provider is ready to resolve flags.
        /// </summary>
        [Description("READY")] Ready,

        /// <summary>
        /// The provider's cached state is no longer valid and may not be up-to-date with the source of truth.
        /// </summary>
        [Description("STALE")] Stale,

        /// <summary>
        /// The provider is in an error state and unable to evaluate flags.
        /// </summary>
        [Description("ERROR")] Error,

        /// <summary>
        /// The provider has entered an irrecoverable error state.
        /// </summary>
        [Description("FATAL")] Fatal,
    }
}
