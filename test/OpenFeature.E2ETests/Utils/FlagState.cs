namespace OpenFeature.E2ETests.Utils;

public class FlagState
{
    public FlagState(string Name, object DefaultValue, string Type)
    {
        this.Name = Name;
        this.DefaultValue = DefaultValue;
        this.Type = Type;
    }

    public string Name { get; set; }
    public object DefaultValue { get; set; }
    public string Type { get; set; }
}
