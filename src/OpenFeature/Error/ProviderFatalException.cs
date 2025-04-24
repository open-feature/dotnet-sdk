using System;
using System.Diagnostics.CodeAnalysis;
using OpenFeature.Constant;

namespace OpenFeature.Error;

/// <summary>
/// An exception that signals the provider has entered an irrecoverable error state.
/// </summary>
[ExcludeFromCodeCoverage]
public class ProviderFatalException : FeatureProviderException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="ProviderFatalException"/> class
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Optional inner exception</param>
    public ProviderFatalException(string? message = null, Exception? innerException = null)
        : base(ErrorType.ProviderFatal, message, innerException)
    {
    }
}
