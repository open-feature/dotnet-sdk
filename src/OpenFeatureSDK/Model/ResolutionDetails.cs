using OpenFeatureSDK.Constant;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// Defines the contract that the <see cref="FeatureProvider"/> is required to return
    /// Describes the details of the feature flag being evaluated
    /// </summary>
    /// <typeparam name="T">Flag value type</typeparam>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/types.md#resolution-details"/>
    public class ResolutionDetails<T>
    {
        /// <summary>
        /// Feature flag evaluated value
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Feature flag key
        /// </summary>
        public string FlagKey { get; }

        /// <summary>
        /// Error that occurred during evaluation
        /// <see cref="ErrorType"/>
        /// </summary>
        public ErrorType ErrorType { get; }

        /// <summary>
        /// Describes the reason for the outcome of the evaluation process
        /// <see cref="Reason"/>
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// A variant is a semantic identifier for a value. This allows for referral to particular values without
        /// necessarily including the value itself, which may be quite prohibitively large or otherwise unsuitable
        /// in some cases.
        /// </summary>
        public string Variant { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionDetails{T}"/> class.
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="value">Evaluated value</param>
        /// <param name="errorType">Error</param>
        /// <param name="reason">Reason</param>
        /// <param name="variant">Variant</param>
        public ResolutionDetails(string flagKey, T value, ErrorType errorType = ErrorType.None, string reason = null,
            string variant = null)
        {
            this.Value = value;
            this.FlagKey = flagKey;
            this.ErrorType = errorType;
            this.Reason = reason;
            this.Variant = variant;
        }
    }
}
