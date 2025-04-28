using System;
using System.Diagnostics.CodeAnalysis;
using OpenFeature.Constant;

namespace OpenFeature.Error;

/// <summary>
/// Context does not contain a targeting key and the provider requires one when evaluating a flag.
/// </summary>
[ExcludeFromCodeCoverage]
public class TargetingKeyMissingException : FeatureProviderException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="TargetingKeyMissingException"/> class
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Optional inner exception</param>
    public TargetingKeyMissingException(string? message = null, Exception? innerException = null)
        : base(ErrorType.TargetingKeyMissing, message, innerException)
    {
    }
}
