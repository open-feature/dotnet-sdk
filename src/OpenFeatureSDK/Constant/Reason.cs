namespace OpenFeatureSDK.Constant
{
    /// <summary>
    /// Common reasons used during flag resolution
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/providers.md#requirement-26">Reason Specification</seealso>
    public static class Reason
    {
        /// <summary>
        /// Use when the flag is matched based on the evaluation context user data
        /// </summary>
        public static string TargetingMatch = "TARGETING_MATCH";

        /// <summary>
        /// Use when the flag is matched based on a split rule in the feature flag provider
        /// </summary>
        public static string Split = "SPLIT";

        /// <summary>
        /// Use when the flag is disabled in the feature flag provider
        /// </summary>
        public static string Disabled = "DISABLED";

        /// <summary>
        /// Default reason when evaluating flag
        /// </summary>
        public static string Default = "DEFAULT";

        /// <summary>
        /// Use when an unknown reason is encountered when evaluating flag.
        /// An example of this is if the feature provider returns a reason that is not defined in the spec
        /// </summary>
        public static string Unknown = "UNKNOWN";

        /// <summary>
        /// Use this flag when abnormal execution is encountered.
        /// </summary>
        public static string Error = "ERROR";
    }
}
