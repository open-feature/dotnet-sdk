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
    public EvaluationEvent(string name, IDictionary<string, object?> attributes)
    {
        Name = name;
        Attributes = new Dictionary<string, object?>(attributes);
    }

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the attributes of the event.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Attributes { get; }
}
