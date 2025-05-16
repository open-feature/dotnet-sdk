using System.Collections.Generic;

namespace OpenFeature.Telemetry;

/// <summary>
/// Represents an evaluation event for feature flags.
/// </summary>
public class EvaluationEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationEvent"/> class.
    /// </summary>
    /// <param name="name">The name of the event.</param>
    /// <param name="attributes">The attributes of the event.</param>
    public EvaluationEvent(string name, Dictionary<string, object?> attributes)
    {
        this.Name = name;
        this.Attributes = attributes;
    }

    /// <summary>
    /// Gets or sets the name of the event.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the attributes of the event.
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; }
}
