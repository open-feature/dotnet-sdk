using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Steps;

[Binding]
public class ProviderStepDefinition
{
    private State State { get; }

    public ProviderStepDefinition(State state)
    {
        this.State = state;
    }

    [Given("a error provider")]
    public async Task GivenAErrorProvider()
    {
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns(new Metadata("NSubstituteProvider"));
        provider.Status.Returns(ProviderStatus.Error);

        await Api.Instance.SetProviderAsync(provider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given("a stable provider")]
    public async Task GivenAStableProvider()
    {
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns(new Metadata("NSubstituteProvider"));
        provider.Status.Returns(ProviderStatus.Ready);

        await Api.Instance.SetProviderAsync(provider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given("a stale provider")]
    public async Task GivenAStaleProvider()
    {
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns(new Metadata("NSubstituteProvider"));
        provider.Status.Returns(ProviderStatus.Stale);

        await Api.Instance.SetProviderAsync(provider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given("a not ready provider")]
    public async Task GivenANotReadyProvider()
    {
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns(new Metadata("NSubstituteProvider"));
        provider.Status.Returns(ProviderStatus.Ready, ProviderStatus.NotReady);

        await Api.Instance.SetProviderAsync(provider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given("a fatal provider")]
    public async Task GivenAFatalProvider()
    {
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns(new Metadata("NSubstituteProvider"));
        provider.Status.Returns(ProviderStatus.Fatal);

        await Api.Instance.SetProviderAsync(provider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Then(@"the provider status should be ""(.*)""")]
    public void ThenTheProviderStatusShouldBe(string status)
    {
        var expectedStatus = EnumHelpers.ParseFromDescription<ProviderStatus>(status);
        var provider = Api.Instance.GetProvider();
        Assert.Equal(expectedStatus, provider.Status);
    }
}
