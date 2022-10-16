using System.Linq;
using FluentAssertions;
using Moq;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
    public class OpenFeatureTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("1.1.1", "The API, and any state it maintains SHOULD exist as a global singleton, even in cases wherein multiple versions of the API are present at runtime.")]
        public void OpenFeature_Should_Be_Singleton()
        {
            var openFeature = Api.Instance;
            var openFeature2 = Api.Instance;

            openFeature.Should().BeSameAs(openFeature2);
        }

        [Fact]
        [Specification("1.1.3", "The API MUST provide a function to add hooks which accepts one or more API-conformant hooks, and appends them to the collection of any previously added hooks. When new hooks are added, previously added hooks are not removed.")]
        public void OpenFeature_Should_Add_Hooks()
        {
            var openFeature = Api.Instance;
            var hook1 = new Mock<Hook>(MockBehavior.Strict).Object;
            var hook2 = new Mock<Hook>(MockBehavior.Strict).Object;
            var hook3 = new Mock<Hook>(MockBehavior.Strict).Object;
            var hook4 = new Mock<Hook>(MockBehavior.Strict).Object;

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
        [Specification("1.1.4", "The API MUST provide a function for retrieving the metadata field of the configured `provider`.")]
        public void OpenFeature_Should_Get_Metadata()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var openFeature = Api.Instance;
            var metadata = openFeature.GetProviderMetadata();

            metadata.Should().NotBeNull();
            metadata.Name.Should().Be(NoOpProvider.NoOpProviderName);
        }

        [Theory]
        [InlineData("client1", "version1")]
        [InlineData("client2", null)]
        [InlineData(null, null)]
        [Specification("1.1.5", "The `API` MUST provide a function for creating a `client` which accepts the following options:  - name (optional): A logical string identifier for the client.")]
        public void OpenFeature_Should_Create_Client(string name = null, string version = null)
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
    }
}
