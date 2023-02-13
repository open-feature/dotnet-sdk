using System;
using OpenFeature.Constant;

namespace OpenFeature.Error
{
    /// <summary>
    /// Provider has yet been initialized when evaluating a flag.
    /// </summary>
    public class ProviderNotReadyException : FeatureProviderException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="ProviderNotReadyException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Optional inner exception</param>
        public ProviderNotReadyException(string message = null, Exception innerException = null)
            : base(ErrorType.ProviderNotReady, message, innerException)
        {
        }
    }
}
