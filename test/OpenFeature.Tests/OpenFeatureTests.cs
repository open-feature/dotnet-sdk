using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public class OpenFeatureTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("1.1.1", "The `API`, and any state it maintains SHOULD exist as a global singleton, even in cases wherein multiple versions of the `API` are present at runtime.")]
        public void OpenFeature_Should_Be_Singleton()
        {
            var openFeature = Api.Instance;
            var openFeature2 = Api.Instance;

            openFeature.Should().BeSameAs(openFeature2);
        }

        [Fact]
        [Specification("1.1.2.2", "The provider mutator function MUST invoke the initialize function on the newly registered provider before using it to resolve flag values.")]
        public async Task OpenFeature_Should_Initialize_Provider()
        {
            var providerMockDefault = Substitute.For<FeatureProvider>();
            providerMockDefault.Status.Returns(ProviderStatus.NotReady);

            await Api.Instance.SetProviderAsync(providerMockDefault);
            await providerMockDefault.Received(1).InitializeAsync(Api.Instance.GetContext());

            var providerMockNamed = Substitute.For<FeatureProvider>();
            providerMockNamed.Status.Returns(ProviderStatus.NotReady);

            await Api.Instance.SetProviderAsync("the-name", providerMockNamed);
            await providerMockNamed.Received(1).InitializeAsync(Api.Instance.GetContext());
        }

        [Fact]
        [Specification("1.1.2.3",
            "The provider mutator function MUST invoke the shutdown function on the previously registered provider once it's no longer being used to resolve flag values.")]
        public async Task OpenFeature_Should_Shutdown_Unused_Provider()
        {
            var providerA = Substitute.For<FeatureProvider>();
            providerA.Status.Returns(ProviderStatus.NotReady);

            await Api.Instance.SetProviderAsync(providerA);
            await providerA.Received(1).InitializeAsync(Api.Instance.GetContext());

            var providerB = Substitute.For<FeatureProvider>();
            providerB.Status.Returns(ProviderStatus.NotReady);

            await Api.Instance.SetProviderAsync(providerB);
            await providerB.Received(1).InitializeAsync(Api.Instance.GetContext());
            await providerA.Received(1).ShutdownAsync();

            var providerC = Substitute.For<FeatureProvider>();
            providerC.Status.Returns(ProviderStatus.NotReady);

            await Api.Instance.SetProviderAsync("named", providerC);
            await providerC.Received(1).InitializeAsync(Api.Instance.GetContext());

            var providerD = Substitute.For<FeatureProvider>();
            providerD.Status.Returns(ProviderStatus.NotReady);

            await Api.Instance.SetProviderAsync("named", providerD);
            await providerD.Received(1).InitializeAsync(Api.Instance.GetContext());
            await providerC.Received(1).ShutdownAsync();
        }

        [Fact]
        [Specification("1.6.1", "The API MUST define a mechanism to propagate a shutdown request to active providers.")]
        public async Task OpenFeature_Should_Support_Shutdown()
        {
            var providerA = Substitute.For<FeatureProvider>();
            providerA.Status.Returns(ProviderStatus.NotReady);

            var providerB = Substitute.For<FeatureProvider>();
            providerB.Status.Returns(ProviderStatus.NotReady);

            await Api.Instance.SetProviderAsync(providerA);
            await Api.Instance.SetProviderAsync("named", providerB);

            await Api.Instance.ShutdownAsync();

            await providerA.Received(1).ShutdownAsync();
            await providerB.Received(1).ShutdownAsync();
        }

        [Fact]
        [Specification("1.1.3", "The `API` MUST provide a function to bind a given `provider` to one or more client `name`s. If the client-name already has a bound provider, it is overwritten with the new mapping.")]
        public async Task OpenFeature_Should_Not_Change_Named_Providers_When_Setting_Default_Provider()
        {
            var openFeature = Api.Instance;

            await openFeature.SetProviderAsync(new NoOpFeatureProvider());
            await openFeature.SetProviderAsync(TestProvider.DefaultName, new TestProvider());

            var defaultClient = openFeature.GetProviderMetadata();
            var namedClient = openFeature.GetProviderMetadata(TestProvider.DefaultName);

            defaultClient?.Name.Should().Be(NoOpProvider.NoOpProviderName);
            namedClient?.Name.Should().Be(TestProvider.DefaultName);
        }

        [Fact]
        [Specification("1.1.3", "The `API` MUST provide a function to bind a given `provider` to one or more client `name`s. If the client-name already has a bound provider, it is overwritten with the new mapping.")]
        public async Task OpenFeature_Should_Set_Default_Provide_When_No_Name_Provided()
        {
            var openFeature = Api.Instance;

            await openFeature.SetProviderAsync(new TestProvider());

            var defaultClient = openFeature.GetProviderMetadata();

            defaultClient?.Name.Should().Be(TestProvider.DefaultName);
        }

        [Fact]
        [Specification("1.1.3", "The `API` MUST provide a function to bind a given `provider` to one or more client `name`s. If the client-name already has a bound provider, it is overwritten with the new mapping.")]
        public async Task OpenFeature_Should_Assign_Provider_To_Existing_Client()
        {
            const string name = "new-client";
            var openFeature = Api.Instance;

            await openFeature.SetProviderAsync(name, new TestProvider());
            await openFeature.SetProviderAsync(name, new NoOpFeatureProvider());

            openFeature.GetProviderMetadata(name)?.Name.Should().Be(NoOpProvider.NoOpProviderName);
        }

        [Fact]
        [Specification("1.1.3", "The `API` MUST provide a function to bind a given `provider` to one or more client `name`s. If the client-name already has a bound provider, it is overwritten with the new mapping.")]
        public async Task OpenFeature_Should_Allow_Multiple_Client_Names_Of_Same_Instance()
        {
            var openFeature = Api.Instance;
            var provider = new TestProvider();

            await openFeature.SetProviderAsync("a", provider);
            await openFeature.SetProviderAsync("b", provider);

            var clientA = openFeature.GetProvider("a");
            var clientB = openFeature.GetProvider("b");

            clientA.Should().Be(clientB);
        }

        [Fact]
        [Specification("1.1.4", "The `API` MUST provide a function to add `hooks` which accepts one or more API-conformant `hooks`, and appends them to the collection of any previously added hooks. When new hooks are added, previously added hooks are not removed.")]
        public void OpenFeature_Should_Add_Hooks()
        {
            var openFeature = Api.Instance;
            var hook1 = Substitute.For<Hook>();
            var hook2 = Substitute.For<Hook>();
            var hook3 = Substitute.For<Hook>();
            var hook4 = Substitute.For<Hook>();

            openFeature.ClearHooks();

            openFeature.AddHooks(hook1);

            openFeature.GetHooks().Should().Contain(hook1);
            Assert.Single(openFeature.GetHooks());

            openFeature.AddHooks(hook2);
            openFeature.GetHooks().Should().ContainInOrder(hook1, hook2);
            openFeature.GetHooks().Count().Should().Be(2);

            openFeature.AddHooks(new[] { hook3, hook4 });
            openFeature.GetHooks().Should().ContainInOrder(hook1, hook2, hook3, hook4);
            openFeature.GetHooks().Count().Should().Be(4);

            openFeature.ClearHooks();
            Assert.Empty(openFeature.GetHooks());
        }

        [Fact]
        [Specification("1.1.5", "The API MUST provide a function for retrieving the metadata field of the configured `provider`.")]
        public async Task OpenFeature_Should_Get_Metadata()
        {
            await Api.Instance.SetProviderAsync(new NoOpFeatureProvider());
            var openFeature = Api.Instance;
            var metadata = openFeature.GetProviderMetadata();

            metadata.Should().NotBeNull();
            metadata?.Name.Should().Be(NoOpProvider.NoOpProviderName);
        }

        [Theory]
        [InlineData("client1", "version1")]
        [InlineData("client2", null)]
        [InlineData(null, null)]
        [Specification("1.1.6", "The `API` MUST provide a function for creating a `client` which accepts the following options: - name (optional): A logical string identifier for the client.")]
        public void OpenFeature_Should_Create_Client(string? name = null, string? version = null)
        {
            var openFeature = Api.Instance;
            var client = openFeature.GetClient(name, version);

            client.Should().NotBeNull();
            client.GetMetadata().Name.Should().Be(name);
            client.GetMetadata().Version.Should().Be(version);
        }

        [Fact]
        public void Should_Set_Given_Context()
        {
            var context = EvaluationContext.Empty;

            Api.Instance.SetContext(context);

            Api.Instance.GetContext().Should().BeSameAs(context);

            context = EvaluationContext.Builder().Build();

            Api.Instance.SetContext(context);

            Api.Instance.GetContext().Should().BeSameAs(context);
        }

        [Fact]
        public void Should_Always_Have_Provider()
        {
            Api.Instance.GetProvider().Should().NotBeNull();
        }

        [Fact]
        public async Task OpenFeature_Should_Allow_Multiple_Client_Mapping()
        {
            var openFeature = Api.Instance;

            await openFeature.SetProviderAsync("client1", new TestProvider());
            await openFeature.SetProviderAsync("client2", new NoOpFeatureProvider());

            var client1 = openFeature.GetClient("client1");
            var client2 = openFeature.GetClient("client2");

            client1.GetMetadata().Name.Should().Be("client1");
            client2.GetMetadata().Name.Should().Be("client2");

            (await client1.GetBooleanValueAsync("test", false)).Should().BeTrue();
            (await client2.GetBooleanValueAsync("test", false)).Should().BeFalse();
        }
    }
}
