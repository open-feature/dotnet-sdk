using OpenFeature.Constant;

namespace OpenFeature.Model
{
    /// <summary>
    /// The contract returned to the caller that describes the result of the flag evaluation process.
    /// </summary>
    /// <typeparam name="T">Flag value type</typeparam>
    /// <seealso href="https://github.com/open-feature/spec/blob/v0.7.0/specification/types.md#evaluation-details"/>
    public sealed class FlagEvaluationDetails<T>
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
        /// </summary>
        public ErrorType ErrorType { get; }

        /// <summary>
        /// Message containing additional details about an error.
        /// <para>
        /// Will be <see langword="null" /> if there is no error or if the provider didn't provide any additional error
        /// details.
        /// </para>
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Describes the reason for the outcome of the evaluation process
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// A variant is a semantic identifier for a value. This allows for referral to particular values without
        /// necessarily including the value itself, which may be quite prohibitively large or otherwise unsuitable
        /// in some cases.
        /// </summary>
        public string? Variant { get; }

        /// <summary>
        /// A structure which supports definition of arbitrary properties, with keys of type string, and values of type boolean, string, or number.
        /// </summary>
        public FlagMetadata? FlagMetadata { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEvaluationDetails{T}"/> class.
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="value">Evaluated value</param>
        /// <param name="errorType">Error</param>
        /// <param name="reason">Reason</param>
        /// <param name="variant">Variant</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="flagMetadata">Flag metadata</param>
        public FlagEvaluationDetails(string flagKey, T value, ErrorType errorType, string? reason, string? variant,
            string? errorMessage = null, FlagMetadata? flagMetadata = null)
        {
            this.Value = value;
            this.FlagKey = flagKey;
            this.ErrorType = errorType;
            this.Reason = reason;
            this.Variant = variant;
            this.ErrorMessage = errorMessage;
            this.FlagMetadata = flagMetadata;
        }
    }
}
