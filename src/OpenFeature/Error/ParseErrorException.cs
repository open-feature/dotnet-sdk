using System;
using System.Diagnostics.CodeAnalysis;
using OpenFeature.Constant;

namespace OpenFeature.Error;

/// <summary>
/// Provider failed to parse the flag response when evaluating a flag.
/// </summary>
[ExcludeFromCodeCoverage]
public class ParseErrorException : FeatureProviderException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="ParseErrorException"/> class
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Optional inner exception</param>
    public ParseErrorException(string? message = null, Exception? innerException = null)
        : base(ErrorType.ParseError, message, innerException)
    {
    }
}
