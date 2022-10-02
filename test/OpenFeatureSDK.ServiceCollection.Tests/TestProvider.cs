namespace OpenFeatureSDK.ServiceCollection.Tests;

public abstract class TestProvider : FeatureProvider
{ }

public class TestFeatures
{
    public string StringFeature { get; set; }
    public string StringFeature2 { get; set; }

    public int IntFeature { get; set; }
    public double DoubleFeature { get; set; }
    public bool BoolFeature { get; set; }
}
