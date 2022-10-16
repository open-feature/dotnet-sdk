using System.ComponentModel;

namespace OpenFeature.Constant
{
    /// <summary>
    /// These errors are used to indicate abnormal execution when evaluation a flag
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/providers.md#requirement-28"/>
    public enum ErrorType
    {
        /// <summary>
        /// Default value, no error occured
        /// </summary>
        None,

        /// <summary>
        /// Provider has yet been initialized
        /// </summary>
        [Description("PROVIDER_NOT_READY")] ProviderNotReady,

        /// <summary>
        /// Provider was unable to find the flag
        /// </summary>
        [Description("FLAG_NOT_FOUND")] FlagNotFound,

        /// <summary>
        /// Provider failed to parse the flag response
        /// </summary>
        [Description("PARSE_ERROR")] ParseError,

        /// <summary>
        /// Request type does not match the expected type
        /// </summary>
        [Description("TYPE_MISMATCH")] TypeMismatch,

        /// <summary>
        /// Abnormal execution of the provider
        /// </summary>
        [Description("GENERAL")] General,

        /// <summary>
        /// Context does not satisfy provider requirements.
        /// </summary>
        [Description("INVALID_CONTEXT")] InvalidContext,

        /// <summary>
        /// Context does not contain a targeting key and the provider requires one.
        /// </summary>
        [Description("TARGETING_KEY_MISSING")] TargetingKeyMissing,
    }
}
