using System;

namespace OpenFeature.E2ETests.Utils;

public class FlagTypesUtil
{
    internal static FlagType ToEnum(string flagType)
    {
        return flagType.ToLowerInvariant() switch
        {
            "boolean" => FlagType.Boolean,
            "float" => FlagType.Float,
            "integer" => FlagType.Integer,
            "string" => FlagType.String,
            _ => throw new ArgumentException("Invalid flag type")
        };
    }
}

internal enum FlagType
{
    Integer,
    Float,
    String,
    Boolean
}
