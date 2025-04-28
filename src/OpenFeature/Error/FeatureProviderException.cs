using System;
using OpenFeature.Constant;

namespace OpenFeature.Error;

/// <summary>
/// Used to represent an abnormal error when evaluating a flag.
/// This exception should be thrown when evaluating a flag inside a IFeatureFlag provider
/// </summary>
public class FeatureProviderException : Exception
{
    /// <summary>
    /// Error that occurred during evaluation
    /// </summary>
    public ErrorType ErrorType { get; }

    /// <summary>
    /// Initialize a new instance of the <see cref="FeatureProviderException"/> class
    /// </summary>
    /// <param name="errorType">Common error types <see cref="ErrorType"/></param>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Optional inner exception</param>
    public FeatureProviderException(ErrorType errorType, string? message = null, Exception? innerException = null)
        : base(message, innerException)
    {
        this.ErrorType = errorType;
    }
}
