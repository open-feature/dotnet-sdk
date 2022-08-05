using OpenFeature.SDK.Constant;
using OpenFeature.SDK.Extension;

namespace OpenFeature.SDK.Model
{
    /// <summary>
    /// The contract returned to the caller that describes the result of the flag evaluation process.
    /// </summary>
    /// <typeparam name="T">Flag value type</typeparam>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/types.md#resolution-details"/>
    public class FlagEvaluationDetails<T>
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
        public string ErrorType { get; }

        /// <summary>
        /// Describes the reason for the outcome of the evaluation process
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// A variant is a semantic identifier for a value. This allows for referral to particular values without
        /// necessarily including the value itself, which may be quite prohibitively large or otherwise unsuitable
        /// in some cases.
        /// </summary>
        public string Variant { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEvaluationDetails{T}"/> class.
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="value">Evaluated value</param>
        /// <param name="errorType">Error</param>
        /// <param name="reason">Reason</param>
        /// <param name="variant">Variant</param>
        public FlagEvaluationDetails(string flagKey, T value, ErrorType errorType, string reason, string variant)
        {
            this.Value = value;
            this.FlagKey = flagKey;
            this.ErrorType = errorType.GetDescription();
            this.Reason = reason;
            this.Variant = variant;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagEvaluationDetails{T}"/> class.
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="value">Evaluated value</param>
        /// <param name="errorType">Error</param>
        /// <param name="reason">Reason</param>
        /// <param name="variant">Variant</param>
        public FlagEvaluationDetails(string flagKey, T value, string errorType, string reason, string variant)
        {
            this.Value = value;
            this.FlagKey = flagKey;
            this.ErrorType = errorType;
            this.Reason = reason;
            this.Variant = variant;
        }
    }
}
