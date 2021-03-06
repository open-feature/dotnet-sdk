using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenFeature.SDK.Constant;
using OpenFeature.SDK.Model;
using OpenFeature.SDK.Tests.Internal;
using Xunit;

namespace OpenFeature.SDK.Tests
{
    public class OpenFeatureHookTests
    {
        [Fact]
        [Specification("1.5.1", "The `evaluation options` structure's `hooks` field denotes an ordered collection of hooks that the client MUST execute for the respective flag evaluation, in addition to those already configured.")]
        [Specification("4.4.2", "Hooks MUST be evaluated in the following order:  - before: API, Client, Invocation - after: Invocation, Client, API - error (if applicable): Invocation, Client, API - finally: Invocation, Client, API")]
        public async Task Hooks_Should_Be_Called_In_Order()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<bool>();
            var clientHook = new Mock<Hook>();
            var invocationHook = new Mock<Hook>();

            var sequence = new MockSequence();

            invocationHook.InSequence(sequence).Setup(x => x.Before(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>()))
                .ReturnsAsync(new EvaluationContext());

            clientHook.InSequence(sequence).Setup(x => x.Before(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>()))
                .ReturnsAsync(new EvaluationContext());

            invocationHook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<bool>>(),
                It.IsAny<FlagEvaluationDetails<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>()));

            clientHook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<bool>>(),
                It.IsAny<FlagEvaluationDetails<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>()));

            invocationHook.InSequence(sequence).Setup(x => x.Finally(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>()));

            clientHook.InSequence(sequence).Setup(x => x.Finally(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>()));

            OpenFeature.Instance.SetProvider(new NoOpFeatureProvider());
            var client = OpenFeature.Instance.GetClient(clientName, clientVersion);
            client.AddHooks(clientHook.Object);

            await client.GetBooleanValue(flagName, defaultValue, new EvaluationContext(),
                new FlagEvaluationOptions(invocationHook.Object, new Dictionary<string, object>()));

            invocationHook.Verify(x => x.Before(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            clientHook.Verify(x => x.Before(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            invocationHook.Verify(x => x.After(
                It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            clientHook.Verify(x => x.After(
                It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            invocationHook.Verify(x => x.Finally(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            clientHook.Verify(x => x.Finally(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);
        }

        [Fact]
        [Specification("4.1.1", "Hook context MUST provide: the `flag key`, `flag value type`, `evaluation context`, and the `default value`.")]
        public void Hook_Context_Should_Not_Allow_Nulls()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<TestStructure>(null, new TestStructure(), FlagValueType.Object, new ClientMetadata(null, null),
                    new Metadata(null), new EvaluationContext()));

            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<TestStructure>("test", new TestStructure(), FlagValueType.Object, null,
                    new Metadata(null), new EvaluationContext()));

            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<TestStructure>("test", new TestStructure(), FlagValueType.Object, new ClientMetadata(null, null),
                    null, new EvaluationContext()));

            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<TestStructure>("test", new TestStructure(), FlagValueType.Object, new ClientMetadata(null, null),
                    new Metadata(null), null));
        }

        [Fact]
        [Specification("4.1.2", "The `hook context` SHOULD provide: access to the `client metadata` and the `provider metadata` fields.")]
        [Specification("4.1.3", "The `flag key`, `flag type`, and `default value` properties MUST be immutable. If the language does not support immutability, the hook MUST NOT modify these properties.")]
        public void Hook_Context_Should_Have_Properties_And_Be_Immutable()
        {
            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var testStructure = new TestStructure();
            var context = new HookContext<TestStructure>("test", testStructure, FlagValueType.Object, clientMetadata,
                providerMetadata, new EvaluationContext());

            context.ClientMetadata.Should().BeSameAs(clientMetadata);
            context.ProviderMetadata.Should().BeSameAs(providerMetadata);
            context.FlagKey.Should().Be("test");
            context.DefaultValue.Should().BeSameAs(testStructure);
            context.FlagValueType.Should().Be(FlagValueType.Object);
        }

        [Fact]
        [Specification("4.1.4", "The evaluation context MUST be mutable only within the `before` hook.")]
        [Specification("4.3.3", "Any `evaluation context` returned from a `before` hook MUST be passed to subsequent `before` hooks (via `HookContext`).")]
        [Specification("4.3.4", "When `before` hooks have finished executing, any resulting `evaluation context` MUST be merged with the invocation `evaluation context` with the invocation `evaluation context` taking precedence in the case of any conflicts.")]
        public async Task Evaluation_Context_Must_Be_Mutable_Before_Hook()
        {
            var evaluationContext = new EvaluationContext { ["test"] = "test" };
            var hook1 = new Mock<Hook>();
            var hook2 = new Mock<Hook>();
            var hookContext = new HookContext<bool>("test", false,
                FlagValueType.Boolean, new ClientMetadata("test", "1.0.0"), new Metadata(NoOpProvider.NoOpProviderName),
                evaluationContext);

            hook1.Setup(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(evaluationContext);

            hook2.Setup(x =>
                    x.Before(hookContext, It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(evaluationContext);

            var client = OpenFeature.Instance.GetClient("test", "1.0.0");
            await client.GetBooleanValue("test", false, new EvaluationContext(),
                new FlagEvaluationOptions(new[] { hook1.Object, hook2.Object }, new Dictionary<string, object>()));

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.Before(It.Is<HookContext<bool>>(a => a.EvaluationContext.Get<string>("test") == "test"), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        [Specification("4.2.1", "`hook hints` MUST be a structure supports definition of arbitrary properties, with keys of type `string`, and values of type `boolean | string | number | datetime | structure`..")]
        [Specification("4.2.2.1", "Condition: `Hook hints` MUST be immutable.")]
        [Specification("4.2.2.2", "Condition: The client `metadata` field in the `hook context` MUST be immutable.")]
        [Specification("4.2.2.3", "Condition: The provider `metadata` field in the `hook context` MUST be immutable")]
        [Specification("4.3.1", "Hooks MUST specify at least one stage.")]
        public async Task Hook_Should_Return_No_Errors()
        {
            var hook = new TestHookNoOverride();
            var hookHints = new Dictionary<string, object>
            {
                ["string"] = "test",
                ["number"] = 1,
                ["boolean"] = true,
                ["datetime"] = DateTime.Now,
                ["structure"] = new TestStructure()
            };
            var hookContext = new HookContext<bool>("test", false, FlagValueType.Boolean,
                new ClientMetadata(null, null), new Metadata(null), new EvaluationContext());

            await hook.Before(hookContext, hookHints);
            await hook.After(hookContext, new FlagEvaluationDetails<bool>("test", false, ErrorType.None, "testing", "testing"), hookHints);
            await hook.Finally(hookContext, hookHints);
            await hook.Error(hookContext, new Exception(), hookHints);

            hookContext.ClientMetadata.Name.Should().BeNull();
            hookContext.ClientMetadata.Version.Should().BeNull();
            hookContext.ProviderMetadata.Name.Should().BeNull();
        }

        [Fact]
        [Specification("4.3.5", "The `after` stage MUST run after flag resolution occurs. It accepts a `hook context` (required), `flag evaluation details` (required) and `hook hints` (optional). It has no return value.")]
        [Specification("4.3.6", "The `error` hook MUST run when errors are encountered in the `before` stage, the `after` stage or during flag resolution. It accepts `hook context` (required), `exception` representing what went wrong (required), and `hook hints` (optional). It has no return value.")]
        [Specification("4.3.7", "The `finally` hook MUST run after the `before`, `after`, and `error` stages. It accepts a `hook context` (required) and `hook hints` (optional). There is no return value.")]
        [Specification("4.5.1", "`Flag evaluation options` MAY contain `hook hints`, a map of data to be provided to hook invocations.")]
        [Specification("4.5.2", "`hook hints` MUST be passed to each hook.")]
        [Specification("4.5.3", "The hook MUST NOT alter the `hook hints` structure.")]
        public async Task Hook_Should_Execute_In_Correct_Order()
        {
            var featureProvider = new Mock<IFeatureProvider>();
            var hook = new Mock<Hook>();

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            hook.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new EvaluationContext());

            featureProvider.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>(), null))
                .ReturnsAsync(new ResolutionDetails<bool>("test", false));

            hook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(),
                It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()));

            hook.InSequence(sequence).Setup(x =>
                x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()));

            OpenFeature.Instance.SetProvider(featureProvider.Object);
            var client = OpenFeature.Instance.GetClient();
            client.AddHooks(hook.Object);

            await client.GetBooleanValue("test", false);

            hook.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            featureProvider.Verify(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>(), null), Times.Once);

        }

        [Fact]
        [Specification("4.4.1", "The API, Client and invocation MUST have a method for registering hooks which accepts `flag evaluation options`")]
        public async Task Register_Hooks_Should_Be_Available_At_All_Levels()
        {
            var hook1 = new Mock<Hook>();
            var hook2 = new Mock<Hook>();
            var hook3 = new Mock<Hook>();

            OpenFeature.Instance.AddHooks(hook1.Object);
            var client = OpenFeature.Instance.GetClient();
            client.AddHooks(hook2.Object);
            await client.GetBooleanValue("test", false, null,
                new FlagEvaluationOptions(hook3.Object, new Dictionary<string, object>()));

            client.ClearHooks();
        }

        [Fact]
        [Specification("4.4.3", "If a `finally` hook abnormally terminates, evaluation MUST proceed, including the execution of any remaining `finally` hooks.")]
        public async Task Finally_Hook_Should_Be_Executed_Even_If_Abnormal_Termination()
        {
            var featureProvider = new Mock<IFeatureProvider>();
            var hook1 = new Mock<Hook>();
            var hook2 = new Mock<Hook>();

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            hook1.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new EvaluationContext());

            hook2.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new EvaluationContext());

            featureProvider.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>(),
                    null))
                .ReturnsAsync(new ResolutionDetails<bool>("test", false));

            hook1.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(),
                It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()));

            hook2.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(),
                It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()));

            hook1.Setup(x =>
                    x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .Throws(new Exception());

            hook2.InSequence(sequence).Setup(x =>
                    x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()));

            OpenFeature.Instance.SetProvider(featureProvider.Object);
            var client = OpenFeature.Instance.GetClient();
            client.AddHooks(new[] { hook1.Object, hook2.Object });

            await client.GetBooleanValue("test", false);

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook1.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook1.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            featureProvider.Verify(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>(), null), Times.Once);
        }

        [Fact]
        [Specification("4.4.4", "If a `finally` hook abnormally terminates, evaluation MUST proceed, including the execution of any remaining `finally` hooks.")]
        public async Task Error_Hook_Should_Be_Executed_Even_If_Abnormal_Termination()
        {
            var featureProvider = new Mock<IFeatureProvider>();
            var hook1 = new Mock<Hook>();
            var hook2 = new Mock<Hook>();

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            hook1.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new EvaluationContext());

            hook2.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new EvaluationContext());

            featureProvider.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>(),
                    null))
                .Throws(new Exception());

            hook1.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null))
                .ThrowsAsync(new Exception());

            hook2.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null));

            OpenFeature.Instance.SetProvider(featureProvider.Object);
            var client = OpenFeature.Instance.GetClient();
            client.AddHooks(new[] { hook1.Object, hook2.Object });

            await client.GetBooleanValue("test", false);

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook1.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
            hook2.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
        }

        [Fact]
        [Specification("4.4.6", "If an error occurs during the evaluation of `before` or `after` hooks, any remaining hooks in the `before` or `after` stages MUST NOT be invoked.")]
        public async Task Error_Occurs_During_Before_After_Evaluation_Should_Not_Invoke_Any_Remaining_Hooks()
        {
            var featureProvider = new Mock<IFeatureProvider>();
            var hook1 = new Mock<Hook>();
            var hook2 = new Mock<Hook>();

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            hook1.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ThrowsAsync(new Exception());

            hook1.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null));

            hook2.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null));

            OpenFeature.Instance.SetProvider(featureProvider.Object);
            var client = OpenFeature.Instance.GetClient();
            client.AddHooks(new[] { hook1.Object, hook2.Object });

            await client.GetBooleanValue("test", false);

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
            hook1.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
            hook2.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
        }
    }
}
