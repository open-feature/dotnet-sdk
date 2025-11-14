namespace OpenFeature.Hosting;

/// <summary>
///  Options to configure OpenFeature
/// </summary>
public class OpenFeatureOptions
{
    private readonly HashSet<string> _hookNames = [];

    internal IReadOnlyCollection<string> HookNames => _hookNames;

    internal void AddHookName(string name)
    {
        lock (_hookNames)
        {
            _hookNames.Add(name);
        }
    }
}
