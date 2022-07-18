using System;
using OpenFeature.SDK.Constant;
using OpenFeature.SDK.Extension;

namespace OpenFeature.SDK.Error
{
    /// <summary>
    /// Used to represent an abnormal error when evaluating a flag. This exception should be thrown
    /// when evaluating a flag inside a IFeatureFlag provider
    /// </summary>
    public class FeatureProviderException : Exception
    {
        /// <summary>
        /// Description of error that occured when evaluating a flag
        /// </summary>
        public string ErrorDescription { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="FeatureProviderException"/> class
        /// </summary>
        /// <param name="errorType">Common error types <see cref="ErrorType"/></param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Optional inner exception</param>
        public FeatureProviderException(ErrorType errorType, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            this.ErrorDescription = errorType.GetDescription();
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="FeatureProviderException"/> class
        /// </summary>
        /// <param name="errorCode">A string representation describing the error that occured</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Optional inner exception</param>
        public FeatureProviderException(string errorCode, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            this.ErrorDescription = errorCode;
        }
    }
}
