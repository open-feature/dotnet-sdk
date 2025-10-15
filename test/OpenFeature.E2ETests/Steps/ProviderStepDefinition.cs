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

    [Given("a not ready provider")]
    public async Task GivenANotReadyProvider()
    {
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns(new Metadata("NSubstituteProvider"));
        provider.Status.Returns(ProviderStatus.Ready, ProviderStatus.NotReady);

        await Api.Instance.SetProviderAsync(provider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
        this.State.Provider = provider;
    }

    [Given("a fatal provider")]
    public async Task GivenAFatalProvider()
    {
        var provider = Substitute.For<FeatureProvider>();
        provider.GetMetadata().Returns(new Metadata("NSubstituteProvider"));
        provider.Status.Returns(ProviderStatus.Fatal);

        await Api.Instance.SetProviderAsync(provider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
        this.State.Provider = provider;
    }
}
