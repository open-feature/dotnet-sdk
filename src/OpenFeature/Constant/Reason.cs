namespace OpenFeature.Constant
{
    /// <summary>
    /// Common reasons used during flag resolution
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/v0.5.2/specification/sections/02-providers.md#requirement-225">Reason Specification</seealso>
    public static class Reason
    {
        /// <summary>
        /// Use when the flag is matched based on the evaluation context user data
        /// </summary>
        public const string TargetingMatch = "TARGETING_MATCH";

        /// <summary>
        /// Use when the flag is matched based on a split rule in the feature flag provider
        /// </summary>
        public const string Split = "SPLIT";

        /// <summary>
        /// Use when the flag is disabled in the feature flag provider
        /// </summary>
        public const string Disabled = "DISABLED";

        /// <summary>
        /// Default reason when evaluating flag
        /// </summary>
        public const string Default = "DEFAULT";

        /// <summary>
        /// The resolved value is static (no dynamic evaluation)
        /// </summary>
        public const string Static = "STATIC";

        /// <summary>
        /// The resolved value was retrieved from cache
        /// </summary>
        public const string Cached = "CACHED";

        /// <summary>
        /// Use when an unknown reason is encountered when evaluating flag.
        /// An example of this is if the feature provider returns a reason that is not defined in the spec
        /// </summary>
        public const string Unknown = "UNKNOWN";

        /// <summary>
        /// Use this flag when abnormal execution is encountered.
        /// </summary>
        public const string Error = "ERROR";
    }
}
