namespace OpenFeature.Hosting;

/// <summary>
/// Options to configure the default feature client name.
/// </summary>
public class PolicyNameOptions
{
    /// <summary>
    /// A delegate to select the default feature client name.
    /// </summary>
    public Func<IServiceProvider, string?> DefaultNameSelector { get; set; } = null!;
}
