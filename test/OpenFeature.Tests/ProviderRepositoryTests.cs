using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Model;
using Xunit;

// We intentionally do not await for purposes of validating behavior.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace OpenFeature.Tests
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public class ProviderRepositoryTests
    {
        [Fact]
        public async void Default_Provider_Is_Set_Without_Await()
        {
            var repository = new ProviderRepository();
            var provider = new NoOpFeatureProvider();
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(provider, context);
            Assert.Equal(provider, repository.GetProvider());
        }

        [Fact]
        public async void AfterSet_Is_Invoked_For_Setting_Default_Provider()
        {
            var repository = new ProviderRepository();
            var provider = new NoOpFeatureProvider();
            var context = new EvaluationContextBuilder().Build();
            var callCount = 0;
            // The setting of the provider is synchronous, so the afterSet should be as well.
            await repository.SetProvider(provider, context, afterSet: (theProvider) =>
            {
                callCount++;
                Assert.Equal(provider, theProvider);
            });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task Initialization_Provider_Method_Is_Invoked_For_Setting_Default_Provider()
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(providerMock, context);
            providerMock.Received(1).Initialize(context);
            providerMock.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task AfterInitialization_Is_Invoked_For_Setting_Default_Provider()
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            var callCount = 0;
            await repository.SetProvider(providerMock, context, afterInitialization: (theProvider) =>
            {
                Assert.Equal(providerMock, theProvider);
                callCount++;
            });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task AfterError_Is_Invoked_If_Initialization_Errors_Default_Provider()
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            providerMock.When(x => x.Initialize(context)).Throw(new Exception("BAD THINGS"));
            var callCount = 0;
            Exception receivedError = null;
            await repository.SetProvider(providerMock, context, afterError: (theProvider, error) =>
            {
                Assert.Equal(providerMock, theProvider);
                callCount++;
                receivedError = error;
            });
            Assert.Equal("BAD THINGS", receivedError.Message);
            Assert.Equal(1, callCount);
        }

        [Theory]
        [InlineData(ProviderStatus.Ready)]
        [InlineData(ProviderStatus.Stale)]
        [InlineData(ProviderStatus.Error)]
        public async Task Initialize_Is_Not_Called_For_Ready_Provider(ProviderStatus status)
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(status);
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(providerMock, context);
            providerMock.DidNotReceive().Initialize(context);
        }

        [Theory]
        [InlineData(ProviderStatus.Ready)]
        [InlineData(ProviderStatus.Stale)]
        [InlineData(ProviderStatus.Error)]
        public async Task AfterInitialize_Is_Not_Called_For_Ready_Provider(ProviderStatus status)
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(status);
            var context = new EvaluationContextBuilder().Build();
            var callCount = 0;
            await repository.SetProvider(providerMock, context, afterInitialization: provider => { callCount++; });
            Assert.Equal(0, callCount);
        }

        [Fact]
        public async Task Replaced_Default_Provider_Is_Shutdown()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(provider1, context);
            await repository.SetProvider(provider2, context);
            provider1.Received(1).Shutdown();
            provider2.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task AfterShutdown_Is_Called_For_Shutdown_Provider()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(provider1, context);
            var callCount = 0;
            await repository.SetProvider(provider2, context, afterShutdown: provider =>
            {
                Assert.Equal(provider, provider1);
                callCount++;
            });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task AfterError_Is_Called_For_Shutdown_That_Throws()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.Shutdown().Throws(new Exception("SHUTDOWN ERROR"));
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(provider1, context);
            var callCount = 0;
            Exception errorThrown = null;
            await repository.SetProvider(provider2, context, afterError: (provider, ex) =>
            {
                Assert.Equal(provider, provider1);
                errorThrown = ex;
                callCount++;
            });
            Assert.Equal(1, callCount);
            Assert.Equal("SHUTDOWN ERROR", errorThrown.Message);
        }

        [Fact]
        public async void Named_Provider_Provider_Is_Set_Without_Await()
        {
            var repository = new ProviderRepository();
            var provider = new NoOpFeatureProvider();
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider("the-name", provider, context);
            Assert.Equal(provider, repository.GetProvider("the-name"));
        }

        [Fact]
        public async void AfterSet_Is_Invoked_For_Setting_Named_Provider()
        {
            var repository = new ProviderRepository();
            var provider = new NoOpFeatureProvider();
            var context = new EvaluationContextBuilder().Build();
            var callCount = 0;
            // The setting of the provider is synchronous, so the afterSet should be as well.
            await repository.SetProvider("the-name", provider, context, afterSet: (theProvider) =>
            {
                callCount++;
                Assert.Equal(provider, theProvider);
            });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task Initialization_Provider_Method_Is_Invoked_For_Setting_Named_Provider()
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider("the-name", providerMock, context);
            providerMock.Received(1).Initialize(context);
            providerMock.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task AfterInitialization_Is_Invoked_For_Setting_Named_Provider()
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            var callCount = 0;
            await repository.SetProvider("the-name", providerMock, context, afterInitialization: (theProvider) =>
            {
                Assert.Equal(providerMock, theProvider);
                callCount++;
            });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task AfterError_Is_Invoked_If_Initialization_Errors_Named_Provider()
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            providerMock.When(x => x.Initialize(context)).Throw(new Exception("BAD THINGS"));
            var callCount = 0;
            Exception receivedError = null;
            await repository.SetProvider("the-provider", providerMock, context, afterError: (theProvider, error) =>
            {
                Assert.Equal(providerMock, theProvider);
                callCount++;
                receivedError = error;
            });
            Assert.Equal("BAD THINGS", receivedError.Message);
            Assert.Equal(1, callCount);
        }

        [Theory]
        [InlineData(ProviderStatus.Ready)]
        [InlineData(ProviderStatus.Stale)]
        [InlineData(ProviderStatus.Error)]
        public async Task Initialize_Is_Not_Called_For_Ready_Named_Provider(ProviderStatus status)
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(status);
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider("the-name", providerMock, context);
            providerMock.DidNotReceive().Initialize(context);
        }

        [Theory]
        [InlineData(ProviderStatus.Ready)]
        [InlineData(ProviderStatus.Stale)]
        [InlineData(ProviderStatus.Error)]
        public async Task AfterInitialize_Is_Not_Called_For_Ready_Named_Provider(ProviderStatus status)
        {
            var repository = new ProviderRepository();
            var providerMock = Substitute.For<FeatureProvider>();
            providerMock.GetStatus().Returns(status);
            var context = new EvaluationContextBuilder().Build();
            var callCount = 0;
            await repository.SetProvider("the-name", providerMock, context,
                afterInitialization: provider => { callCount++; });
            Assert.Equal(0, callCount);
        }

        [Fact]
        public async Task Replaced_Named_Provider_Is_Shutdown()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider("the-name", provider1, context);
            await repository.SetProvider("the-name", provider2, context);
            provider1.Received(1).Shutdown();
            provider2.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task AfterShutdown_Is_Called_For_Shutdown_Named_Provider()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider("the-provider", provider1, context);
            var callCount = 0;
            await repository.SetProvider("the-provider", provider2, context, afterShutdown: provider =>
            {
                Assert.Equal(provider, provider1);
                callCount++;
            });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task AfterError_Is_Called_For_Shutdown_Named_Provider_That_Throws()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.Shutdown().Throws(new Exception("SHUTDOWN ERROR"));
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider("the-name", provider1, context);
            var callCount = 0;
            Exception errorThrown = null;
            await repository.SetProvider("the-name", provider2, context, afterError: (provider, ex) =>
            {
                Assert.Equal(provider, provider1);
                errorThrown = ex;
                callCount++;
            });
            Assert.Equal(1, callCount);
            Assert.Equal("SHUTDOWN ERROR", errorThrown.Message);
        }

        [Fact]
        public async Task In_Use_Provider_Named_And_Default_Is_Not_Shutdown()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();

            await repository.SetProvider(provider1, context);
            await repository.SetProvider("A", provider1, context);
            // Provider one is replaced for "A", but not default.
            await repository.SetProvider("A", provider2, context);

            provider1.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task In_Use_Provider_Two_Named_Is_Not_Shutdown()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();

            await repository.SetProvider("B", provider1, context);
            await repository.SetProvider("A", provider1, context);
            // Provider one is replaced for "A", but not "B".
            await repository.SetProvider("A", provider2, context);

            provider1.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task When_All_Instances_Are_Removed_Shutdown_Is_Called()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();

            await repository.SetProvider("B", provider1, context);
            await repository.SetProvider("A", provider1, context);

            await repository.SetProvider("A", provider2, context);
            await repository.SetProvider("B", provider2, context);

            provider1.Received(1).Shutdown();
        }

        [Fact]
        public async Task Can_Get_Providers_By_Name()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();

            await repository.SetProvider("A", provider1, context);
            await repository.SetProvider("B", provider2, context);

            Assert.Equal(provider1, repository.GetProvider("A"));
            Assert.Equal(provider2, repository.GetProvider("B"));
        }

        [Fact]
        public async Task Replaced_Named_Provider_Gets_Latest_Set()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();

            await repository.SetProvider("A", provider1, context);
            await repository.SetProvider("A", provider2, context);

            Assert.Equal(provider2, repository.GetProvider("A"));
        }

        [Fact]
        public async Task Can_Shutdown_All_Providers()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);

            var provider3 = Substitute.For<FeatureProvider>();
            provider3.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();

            await repository.SetProvider(provider1, context);
            await repository.SetProvider("provider1", provider1, context);
            await repository.SetProvider("provider2", provider2, context);
            await repository.SetProvider("provider2a", provider2, context);
            await repository.SetProvider("provider3", provider3, context);

            await repository.Shutdown();

            provider1.Received(1).Shutdown();
            provider2.Received(1).Shutdown();
            provider3.Received(1).Shutdown();
        }

        [Fact]
        public async Task Errors_During_Shutdown_Propagate()
        {
            var repository = new ProviderRepository();
            var provider1 = Substitute.For<FeatureProvider>();
            provider1.GetStatus().Returns(ProviderStatus.NotReady);
            provider1.Shutdown().Throws(new Exception("SHUTDOWN ERROR 1"));

            var provider2 = Substitute.For<FeatureProvider>();
            provider2.GetStatus().Returns(ProviderStatus.NotReady);
            provider2.Shutdown().Throws(new Exception("SHUTDOWN ERROR 2"));

            var provider3 = Substitute.For<FeatureProvider>();
            provider3.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();

            await repository.SetProvider(provider1, context);
            await repository.SetProvider("provider1", provider1, context);
            await repository.SetProvider("provider2", provider2, context);
            await repository.SetProvider("provider2a", provider2, context);
            await repository.SetProvider("provider3", provider3, context);

            var callCountShutdown1 = 0;
            var callCountShutdown2 = 0;
            var totalCallCount = 0;
            await repository.Shutdown(afterError: (provider, exception) =>
            {
                totalCallCount++;
                if (provider == provider1)
                {
                    callCountShutdown1++;
                    Assert.Equal("SHUTDOWN ERROR 1", exception.Message);
                }

                if (provider == provider2)
                {
                    callCountShutdown2++;
                    Assert.Equal("SHUTDOWN ERROR 2", exception.Message);
                }
            });
            Assert.Equal(2, totalCallCount);
            Assert.Equal(1, callCountShutdown1);
            Assert.Equal(1, callCountShutdown2);

            provider1.Received(1).Shutdown();
            provider2.Received(1).Shutdown();
            provider3.Received(1).Shutdown();
        }

        [Fact]
        public async Task Setting_Same_Default_Provider_Has_No_Effect()
        {
            var repository = new ProviderRepository();
            var provider = Substitute.For<FeatureProvider>();
            provider.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(provider, context);
            await repository.SetProvider(provider, context);

            Assert.Equal(provider, repository.GetProvider());
            provider.Received(1).Initialize(context);
            provider.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task Setting_Null_Default_Provider_Has_No_Effect()
        {
            var repository = new ProviderRepository();
            var provider = Substitute.For<FeatureProvider>();
            provider.GetStatus().Returns(ProviderStatus.NotReady);
            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(provider, context);
            await repository.SetProvider(null, context);

            Assert.Equal(provider, repository.GetProvider());
            provider.Received(1).Initialize(context);
            provider.DidNotReceive().Shutdown();
        }

        [Fact]
        public async Task Setting_Null_Named_Provider_Removes_It()
        {
            var repository = new ProviderRepository();

            var namedProvider = Substitute.For<FeatureProvider>();
            namedProvider.GetStatus().Returns(ProviderStatus.NotReady);

            var defaultProvider = Substitute.For<FeatureProvider>();
            defaultProvider.GetStatus().Returns(ProviderStatus.NotReady);

            var context = new EvaluationContextBuilder().Build();
            await repository.SetProvider(defaultProvider, context);

            await repository.SetProvider("named-provider", namedProvider, context);
            await repository.SetProvider("named-provider", null, context);

            Assert.Equal(defaultProvider, repository.GetProvider("named-provider"));
        }

        [Fact]
        public async Task Setting_Named_Provider_With_Null_Name_Has_No_Effect()
        {
            var repository = new ProviderRepository();
            var context = new EvaluationContextBuilder().Build();

            var defaultProvider = Substitute.For<FeatureProvider>();
            defaultProvider.GetStatus().Returns(ProviderStatus.NotReady);
            await repository.SetProvider(defaultProvider, context);

            var namedProvider = Substitute.For<FeatureProvider>();
            namedProvider.GetStatus().Returns(ProviderStatus.NotReady);

            await repository.SetProvider(null, namedProvider, context);

            namedProvider.DidNotReceive().Initialize(context);
            namedProvider.DidNotReceive().Shutdown();

            Assert.Equal(defaultProvider, repository.GetProvider(null));
        }
    }
}
