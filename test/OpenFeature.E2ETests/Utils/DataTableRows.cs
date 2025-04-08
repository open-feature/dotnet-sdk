using OpenFeature.E2ETests.Utils;

internal class DataTableRows
{
    public DataTableRows(string key, string value, string metadataType)
    {
        this.Key = key;
        this.Value = value;

        this.MetadataType = FlagTypesUtil.ToEnum(metadataType);
    }

    public string Key { get; }
    public string Value { get; }
    public FlagType MetadataType { get; }
}
