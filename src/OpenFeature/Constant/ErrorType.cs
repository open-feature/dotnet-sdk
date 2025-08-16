namespace OpenFeature.Constant;

/// <summary>
/// These errors are used to indicate abnormal execution when evaluation a flag
/// </summary>
/// <seealso href="https://github.com/open-feature/spec/blob/v0.5.2/specification/sections/02-providers.md#requirement-227"/>
public enum ErrorType
{
    /// <summary>
    /// Default value, no error occured
    /// </summary>
    None,

    /// <summary>
    /// Provider has yet been initialized
    /// </summary>
    ProviderNotReady,

    /// <summary>
    /// Provider was unable to find the flag
    /// </summary>
    FlagNotFound,

    /// <summary>
    /// Provider failed to parse the flag response
    /// </summary>
    ParseError,

    /// <summary>
    /// Request type does not match the expected type
    /// </summary>
    TypeMismatch,

    /// <summary>
    /// Abnormal execution of the provider
    /// </summary>
    General,

    /// <summary>
    /// Context does not satisfy provider requirements.
    /// </summary>
    InvalidContext,

    /// <summary>
    /// Context does not contain a targeting key and the provider requires one.
    /// </summary>
    TargetingKeyMissing,

    /// <summary>
    /// The provider has entered an irrecoverable error state.
    /// </summary>
    ProviderFatal,
}
