using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable
namespace OpenFeature.Model;

/// <summary>
/// Represents the metadata associated with a feature flag.
/// </summary>
/// <seealso href="https://github.com/open-feature/spec/blob/v0.7.0/specification/types.md#flag-metadata"/>
public sealed class FlagMetadata : BaseMetadata
{
    /// <summary>
    /// Constructor for the <see cref="BaseMetadata"/> class.
    /// </summary>
    public FlagMetadata() : this([])
    {
    }

    /// <summary>
    /// Constructor for the <see cref="BaseMetadata"/> class.
    /// </summary>
    /// <param name="metadata">The dictionary containing the metadata.</param>
    public FlagMetadata(Dictionary<string, object> metadata) : base(metadata)
    {
    }
}
