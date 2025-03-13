using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
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

            Assert.Equal(openFeature2, openFeature);
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
            providerA.GetEventChannel().Returns(Channel.CreateBounded<object>(1));

            await Api.Instance.SetProviderAsync(providerA);
            await providerA.Received(1).InitializeAsync(Api.Instance.GetContext());

            var providerB = Substitute.For<FeatureProvider>();
            providerB.Status.Returns(ProviderStatus.NotReady);
            providerB.GetEventChannel().Returns(Channel.CreateBounded<object>(1));

            await Api.Instance.SetProviderAsync(providerB);
            await providerB.Received(1).InitializeAsync(Api.Instance.GetContext());
            await providerA.Received(1).ShutdownAsync();

            var providerC = Substitute.For<FeatureProvider>();
            providerC.Status.Returns(ProviderStatus.NotReady);
            providerC.GetEventChannel().Returns(Channel.CreateBounded<object>(1));

            await Api.Instance.SetProviderAsync("named", providerC);
            await providerC.Received(1).InitializeAsync(Api.Instance.GetContext());

            var providerD = Substitute.For<FeatureProvider>();
            providerD.Status.Returns(ProviderStatus.NotReady);
            providerD.GetEventChannel().Returns(Channel.CreateBounded<object>(1));

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
            var domainScopedClient = openFeature.GetProviderMetadata(TestProvider.DefaultName);

            Assert.Equal(NoOpProvider.NoOpProviderName, defaultClient?.Name);
            Assert.Equal(TestProvider.DefaultName, domainScopedClient?.Name);
        }

        [Fact]
        [Specification("1.1.3", "The `API` MUST provide a function to bind a given `provider` to one or more client `name`s. If the client-name already has a bound provider, it is overwritten with the new mapping.")]
        public async Task OpenFeature_Should_Set_Default_Provide_When_No_Name_Provided()
        {
            var openFeature = Api.Instance;

            await openFeature.SetProviderAsync(new TestProvider());

            var defaultClient = openFeature.GetProviderMetadata();

            Assert.Equal(TestProvider.DefaultName, defaultClient?.Name);
        }

        [Fact]
        [Specification("1.1.3", "The `API` MUST provide a function to bind a given `provider` to one or more client `name`s. If the client-name already has a bound provider, it is overwritten with the new mapping.")]
        public async Task OpenFeature_Should_Assign_Provider_To_Existing_Client()
        {
            const string name = "new-client";
            var openFeature = Api.Instance;

            await openFeature.SetProviderAsync(name, new TestProvider());
            await openFeature.SetProviderAsync(name, new NoOpFeatureProvider());

            Assert.Equal(NoOpProvider.NoOpProviderName, openFeature.GetProviderMetadata(name)?.Name);
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

            Assert.Equal(clientB, clientA);
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

            Assert.Contains(hook1, openFeature.GetHooks());
            Assert.Single(openFeature.GetHooks());

            openFeature.AddHooks(hook2);
            var expectedHooks = new[] { hook1, hook2 }.AsEnumerable();
            Assert.Equal(expectedHooks, openFeature.GetHooks());

            openFeature.AddHooks(new[] { hook3, hook4 });
            expectedHooks = new[] { hook1, hook2, hook3, hook4 }.AsEnumerable();
            Assert.Equal(expectedHooks, openFeature.GetHooks());

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

            Assert.NotNull(metadata);
            Assert.Equal(NoOpProvider.NoOpProviderName, metadata?.Name);
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

            Assert.NotNull(client);
            Assert.Equal(name, client.GetMetadata().Name);
            Assert.Equal(version, client.GetMetadata().Version);
        }

        [Fact]
        public void Should_Set_Given_Context()
        {
            var context = EvaluationContext.Empty;

            Api.Instance.SetContext(context);

            Assert.Equal(context, Api.Instance.GetContext());

            context = EvaluationContext.Builder().Build();

            Api.Instance.SetContext(context);

            Assert.Equal(context, Api.Instance.GetContext());
        }

        [Fact]
        public void Should_Always_Have_Provider()
        {
            Assert.NotNull(Api.Instance.GetProvider());
        }

        [Fact]
        public async Task OpenFeature_Should_Allow_Multiple_Client_Mapping()
        {
            var openFeature = Api.Instance;

            await openFeature.SetProviderAsync("client1", new TestProvider());
            await openFeature.SetProviderAsync("client2", new NoOpFeatureProvider());

            var client1 = openFeature.GetClient("client1");
            var client2 = openFeature.GetClient("client2");

            Assert.Equal("client1", client1.GetMetadata().Name);
            Assert.Equal("client2", client2.GetMetadata().Name);

            Assert.True(await client1.GetBooleanValueAsync("test", false));
            Assert.False(await client2.GetBooleanValueAsync("test", false));
        }

        [Fact]
        public void SetTransactionContextPropagator_ShouldThrowArgumentNullException_WhenNullPropagatorIsPassed()
        {
            // Arrange
            var api = Api.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => api.SetTransactionContextPropagator(null!));
        }

        [Fact]
        public void SetTransactionContextPropagator_ShouldSetPropagator_WhenValidPropagatorIsPassed()
        {
            // Arrange
            var api = Api.Instance;
            var mockPropagator = Substitute.For<ITransactionContextPropagator>();

            // Act
            api.SetTransactionContextPropagator(mockPropagator);

            // Assert
            Assert.Equal(mockPropagator, api.GetTransactionContextPropagator());
        }

        [Fact]
        public void SetTransactionContext_ShouldThrowArgumentNullException_WhenEvaluationContextIsNull()
        {
            // Arrange
            var api = Api.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => api.SetTransactionContext(null!));
        }

        [Fact]
        public void SetTransactionContext_ShouldSetTransactionContext_WhenValidEvaluationContextIsProvided()
        {
            // Arrange
            var api = Api.Instance;
            var evaluationContext = EvaluationContext.Builder()
                .Set("initial", "yes")
                .Build();
            var mockPropagator = Substitute.For<ITransactionContextPropagator>();
            mockPropagator.GetTransactionContext().Returns(evaluationContext);
            api.SetTransactionContextPropagator(mockPropagator);
            api.SetTransactionContext(evaluationContext);

            // Act
            api.SetTransactionContext(evaluationContext);
            var result = api.GetTransactionContext();

            // Assert
            mockPropagator.Received().SetTransactionContext(evaluationContext);
            Assert.Equal(evaluationContext, result);
            Assert.Equal(evaluationContext.GetValue("initial"), result.GetValue("initial"));
        }

        [Fact]
        public void GetTransactionContext_ShouldReturnEmptyEvaluationContext_WhenNoPropagatorIsSet()
        {
            // Arrange
            var api = Api.Instance;
            var context = EvaluationContext.Builder().Set("status", "not-ready").Build();
            api.SetTransactionContext(context);

            // Act
            var result = api.GetTransactionContext();

            // Assert
            Assert.Equal(EvaluationContext.Empty, result);
        }
    }
}
