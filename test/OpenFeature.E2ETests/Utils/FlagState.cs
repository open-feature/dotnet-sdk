namespace OpenFeature.E2ETests.Utils;

public class FlagState
{
    public FlagState(string key, string defaultValue, FlagType type)
    {
        this.Key = key;
        this.DefaultValue = defaultValue;
        this.Type = type;
    }

    public string Key { get; private set; }
    public string DefaultValue { get; private set; }
    public FlagType Type { get; private set; }
}
