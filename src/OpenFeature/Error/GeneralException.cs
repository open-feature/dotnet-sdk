using System;
using OpenFeature.Constant;

namespace OpenFeature.Error
{
    /// <summary>
    /// Abnormal execution of the provider when evaluating a flag.
    /// This exception should be thrown when evaluating a flag inside a IFeatureFlag provider
    /// </summary>
    public class GeneralException : FeatureProviderException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="GeneralException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Optional inner exception</param>
        public GeneralException(string message = null, Exception innerException = null)
            : base(ErrorType.General, message, innerException)
        {
        }
    }
}
