using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable
namespace OpenFeature.Model;

public sealed class FlagMetadata : BaseMetadata
{
    public FlagMetadata()
    {
    }

    public FlagMetadata(Dictionary<string, object> metadata) : base(metadata)
    {
    }
}
