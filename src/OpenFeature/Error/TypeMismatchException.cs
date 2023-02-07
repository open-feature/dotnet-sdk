using System;
using OpenFeature.Constant;

namespace OpenFeature.Error
{
    /// <summary>
    /// Request type does not match the expected type when evaluating a flag.
    /// This exception should be thrown when evaluating a flag inside a IFeatureFlag provider
    /// </summary>
    public class TypeMismatchException : FeatureProviderException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="TypeMismatchException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Optional inner exception</param>
        public TypeMismatchException(string message = null, Exception innerException = null)
            : base(ErrorType.TypeMismatch, message, innerException)
        {
        }
    }
}
