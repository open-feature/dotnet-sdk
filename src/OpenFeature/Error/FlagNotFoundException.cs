using System.Diagnostics.CodeAnalysis;
using OpenFeature.Constant;

namespace OpenFeature.Error;

/// <summary>
/// Provider was unable to find the flag error when evaluating a flag.
/// </summary>
[ExcludeFromCodeCoverage]
public class FlagNotFoundException : FeatureProviderException
{
    /// <summary>
    /// Initialize a new instance of the <see cref="FlagNotFoundException"/> class
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Optional inner exception</param>
    public FlagNotFoundException(string? message = null, Exception? innerException = null)
        : base(ErrorType.FlagNotFound, message, innerException)
    {
    }
}
