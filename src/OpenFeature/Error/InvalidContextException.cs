using System;
using System.Diagnostics.CodeAnalysis;
using OpenFeature.Constant;

namespace OpenFeature.Error
{
    /// <summary>
    /// Context does not satisfy provider requirements when evaluating a flag.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidContextException : FeatureProviderException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="InvalidContextException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Optional inner exception</param>
        public InvalidContextException(string? message = null, Exception? innerException = null)
            : base(ErrorType.InvalidContext, message, innerException)
        {
        }
    }
}
