namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Tag = "evaluation-options")]
[Scope(Tag = "immutability")]
[Scope(Tag = "async")]
[Scope(Tag = "reason-codes-cached")]
[Scope(Tag = "reason-codes-disabled")]
[Scope(Tag = "deprecated")]
public class ExcludedTagsStep
{
    [BeforeScenario]
    public static void BeforeScenario()
    {
        Skip.If(true, "Tag is not supported");
    }
}
