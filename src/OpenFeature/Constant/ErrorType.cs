using System.ComponentModel;

namespace OpenFeature.Constant
{
    public enum ErrorType
    {
        None,
        [Description("PROVIDER_NOT_READY")]
        ProviderNotReady,
        [Description("FLAG_NOT_FOUND")]
        FlagNotFound,
        [Description("PARSE_ERROR")]
        ParseError,
        [Description("TYPE_MISMATCH")]
        TypeMismatch,
        [Description("GENERAL")]
        General
    }
}
