using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
    public class OpenFeatureHookTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("1.5.1", "The `evaluation options` structure's `hooks` field denotes an ordered collection of hooks that the client MUST execute for the respective flag evaluation, in addition to those already configured.")]
        [Specification("2.3.1", "The provider interface MUST define a `provider hook` mechanism which can be optionally implemented in order to add `hook` instances to the evaluation life-cycle.")]
        [Specification("4.4.2", "Hooks MUST be evaluated in the following order: - before: API, Client, Invocation, Provider - after: Provider, Invocation, Client, API - error (if applicable): Provider, Invocation, Client, API - finally: Provider, Invocation, Client, API")]
        public async Task Hooks_Should_Be_Called_In_Order()
        {
            var fixture = new Fixture();
            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var flagName = fixture.Create<string>();
            var defaultValue = fixture.Create<bool>();
            var apiHook = new Mock<Hook>(MockBehavior.Strict);
            var clientHook = new Mock<Hook>(MockBehavior.Strict);
            var invocationHook = new Mock<Hook>(MockBehavior.Strict);
            var providerHook = new Mock<Hook>(MockBehavior.Strict);

            var sequence = new MockSequence();

            apiHook.InSequence(sequence).Setup(x => x.Before(It.IsAny<HookContext<bool>>(),
                    It.IsAny<IReadOnlyDictionary<string, object>>()))
                .ReturnsAsync(EvaluationContext.Empty);

            clientHook.InSequence(sequence).Setup(x => x.Before(It.IsAny<HookContext<bool>>(),
                    It.IsAny<IReadOnlyDictionary<string, object>>()))
                .ReturnsAsync(EvaluationContext.Empty);

            invocationHook.InSequence(sequence).Setup(x => x.Before(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>()))
                .ReturnsAsync(EvaluationContext.Empty);

            providerHook.InSequence(sequence).Setup(x => x.Before(It.IsAny<HookContext<bool>>(),
                    It.IsAny<IReadOnlyDictionary<string, object>>()))
                .ReturnsAsync(EvaluationContext.Empty);

            providerHook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<bool>>(),
                It.IsAny<FlagEvaluationDetails<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            invocationHook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<bool>>(),
                It.IsAny<FlagEvaluationDetails<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            clientHook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<bool>>(),
                It.IsAny<FlagEvaluationDetails<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            apiHook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<bool>>(),
                It.IsAny<FlagEvaluationDetails<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            providerHook.InSequence(sequence).Setup(x => x.Finally(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            invocationHook.InSequence(sequence).Setup(x => x.Finally(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            clientHook.InSequence(sequence).Setup(x => x.Finally(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            apiHook.InSequence(sequence).Setup(x => x.Finally(It.IsAny<HookContext<bool>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(Task.CompletedTask);

            var testProvider = new TestProvider();
            testProvider.AddHook(providerHook.Object);
            Api.Instance.AddHooks(apiHook.Object);
            Api.Instance.SetProvider(testProvider);
            var client = Api.Instance.GetClient(clientName, clientVersion);
            client.AddHooks(clientHook.Object);

            await client.GetBooleanValue(flagName, defaultValue, EvaluationContext.Empty,
                new FlagEvaluationOptions(invocationHook.Object, ImmutableDictionary<string, object>.Empty));

            apiHook.Verify(x => x.Before(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            clientHook.Verify(x => x.Before(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            invocationHook.Verify(x => x.Before(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            providerHook.Verify(x => x.Before(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            providerHook.Verify(x => x.After(
                It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            invocationHook.Verify(x => x.After(
                It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            clientHook.Verify(x => x.After(
                It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            apiHook.Verify(x => x.After(
                It.IsAny<HookContext<bool>>(), It.IsAny<FlagEvaluationDetails<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            providerHook.Verify(x => x.Finally(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            invocationHook.Verify(x => x.Finally(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            clientHook.Verify(x => x.Finally(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);

            apiHook.Verify(x => x.Finally(
                It.IsAny<HookContext<bool>>(), It.IsAny<IReadOnlyDictionary<string, object>>()), Times.Once);
        }

        [Fact]
        [Specification("4.1.1", "Hook context MUST provide: the `flag key`, `flag value type`, `evaluation context`, and the `default value`.")]
        public void Hook_Context_Should_Not_Allow_Nulls()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<Structure>(null, Structure.Empty, FlagValueType.Object, new ClientMetadata(null, null),
                    new Metadata(null), EvaluationContext.Empty));

            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<Structure>("test", Structure.Empty, FlagValueType.Object, null,
                    new Metadata(null), EvaluationContext.Empty));

            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<Structure>("test", Structure.Empty, FlagValueType.Object, new ClientMetadata(null, null),
                    null, EvaluationContext.Empty));

            Assert.Throws<ArgumentNullException>(() =>
                new HookContext<Structure>("test", Structure.Empty, FlagValueType.Object, new ClientMetadata(null, null),
                    new Metadata(null), null));
        }

        [Fact]
        [Specification("4.1.2", "The `hook context` SHOULD provide: access to the `client metadata` and the `provider metadata` fields.")]
        [Specification("4.1.3", "The `flag key`, `flag type`, and `default value` properties MUST be immutable. If the language does not support immutability, the hook MUST NOT modify these properties.")]
        public void Hook_Context_Should_Have_Properties_And_Be_Immutable()
        {
            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var testStructure = Structure.Empty;
            var context = new HookContext<Structure>("test", testStructure, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            context.ClientMetadata.Should().BeSameAs(clientMetadata);
            context.ProviderMetadata.Should().BeSameAs(providerMetadata);
            context.FlagKey.Should().Be("test");
            context.DefaultValue.Should().BeSameAs(testStructure);
            context.FlagValueType.Should().Be(FlagValueType.Object);
        }

        [Fact]
        [Specification("4.1.4", "The evaluation context MUST be mutable only within the `before` hook.")]
        [Specification("4.3.3", "Any `evaluation context` returned from a `before` hook MUST be passed to subsequent `before` hooks (via `HookContext`).")]
        public async Task Evaluation_Context_Must_Be_Mutable_Before_Hook()
        {
            var evaluationContext = new EvaluationContextBuilder().Set("test", "test").Build();
            var hook1 = new Mock<Hook>(MockBehavior.Strict);
            var hook2 = new Mock<Hook>(MockBehavior.Strict);
            var hookContext = new HookContext<bool>("test", false,
                FlagValueType.Boolean, new ClientMetadata("test", "1.0.0"), new Metadata(NoOpProvider.NoOpProviderName),
                evaluationContext);

            hook1.Setup(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<ImmutableDictionary<string, object>>()))
                .ReturnsAsync(evaluationContext);

            hook2.Setup(x =>
                    x.Before(hookContext, It.IsAny<ImmutableDictionary<string, object>>()))
                .ReturnsAsync(evaluationContext);

            var client = Api.Instance.GetClient("test", "1.0.0");
            await client.GetBooleanValue("test", false, EvaluationContext.Empty,
                new FlagEvaluationOptions(ImmutableList.Create(hook1.Object, hook2.Object), ImmutableDictionary<string, object>.Empty));

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<ImmutableDictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.Before(It.Is<HookContext<bool>>(a => a.EvaluationContext.GetValue("test").AsString == "test"), It.IsAny<ImmutableDictionary<string, object>>()), Times.Once);
        }

        [Fact]
        [Specification("3.2.2", "Evaluation context MUST be merged in the order: API (global; lowest precedence) - client - invocation - before hooks (highest precedence), with duplicate values being overwritten.")]
        [Specification("4.3.4", "When `before` hooks have finished executing, any resulting `evaluation context` MUST be merged with the existing `evaluation context`.")]
        public async Task Evaluation_Context_Must_Be_Merged_In_Correct_Order()
        {
            var propGlobal = "4.3.4global";
            var propGlobalToOverwrite = "4.3.4globalToOverwrite";

            var propClient = "4.3.4client";
            var propClientToOverwrite = "4.3.4clientToOverwrite";

            var propInvocation = "4.3.4invocation";
            var propInvocationToOverwrite = "4.3.4invocationToOverwrite";

            var propHook = "4.3.4hook";

            // setup a cascade of overwriting properties
            Api.Instance.SetContext(new EvaluationContextBuilder()
                .Set(propGlobal, true)
                .Set(propGlobalToOverwrite, false)
                .Build());

            var clientContext = new EvaluationContextBuilder()
                .Set(propClient, true)
                .Set(propGlobalToOverwrite, true)
                .Set(propClientToOverwrite, false)
                .Build();

            var invocationContext = new EvaluationContextBuilder()
                .Set(propInvocation, true)
                .Set(propClientToOverwrite, true)
                .Set(propInvocationToOverwrite, false)
                .Build();

            var hookContext = new EvaluationContextBuilder()
                .Set(propHook, true)
                .Set(propInvocationToOverwrite, true)
                .Build();

            var provider = new Mock<FeatureProvider>(MockBehavior.Strict);

            provider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            provider.Setup(x => x.GetProviderHooks())
                .Returns(ImmutableList<Hook>.Empty);

            provider.Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()))
            .ReturnsAsync(new ResolutionDetails<bool>("test", true));

            Api.Instance.SetProvider(provider.Object);

            var hook = new Mock<Hook>(MockBehavior.Strict);
            hook.Setup(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<ImmutableDictionary<string, object>>()))
                .ReturnsAsync(hookContext);


            var client = Api.Instance.GetClient("test", "1.0.0", null, clientContext);
            await client.GetBooleanValue("test", false, invocationContext, new FlagEvaluationOptions(ImmutableList.Create(hook.Object), ImmutableDictionary<string, object>.Empty));

            // after proper merging, all properties should equal true
            provider.Verify(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.Is<EvaluationContext>(y =>
                (y.GetValue(propGlobal).AsBoolean ?? false)
                && (y.GetValue(propClient).AsBoolean ?? false)
                && (y.GetValue(propGlobalToOverwrite).AsBoolean ?? false)
                && (y.GetValue(propInvocation).AsBoolean ?? false)
                && (y.GetValue(propClientToOverwrite).AsBoolean ?? false)
                && (y.GetValue(propHook).AsBoolean ?? false)
                && (y.GetValue(propInvocationToOverwrite).AsBoolean ?? false)
            )), Times.Once);
        }

        [Fact]
        [Specification("4.2.1", "`hook hints` MUST be a structure supports definition of arbitrary properties, with keys of type `string`, and values of type `boolean | string | number | datetime | structure`..")]
        [Specification("4.2.2.1", "Condition: `Hook hints` MUST be immutable.")]
        [Specification("4.2.2.2", "Condition: The client `metadata` field in the `hook context` MUST be immutable.")]
        [Specification("4.2.2.3", "Condition: The provider `metadata` field in the `hook context` MUST be immutable.")]
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
                ["structure"] = Structure.Empty
            };
            var hookContext = new HookContext<bool>("test", false, FlagValueType.Boolean,
                new ClientMetadata(null, null), new Metadata(null), EvaluationContext.Empty);

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
            var featureProvider = new Mock<FeatureProvider>(MockBehavior.Strict);
            var hook = new Mock<Hook>(MockBehavior.Strict);

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            featureProvider.Setup(x => x.GetProviderHooks())
                .Returns(ImmutableList<Hook>.Empty);

            hook.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(EvaluationContext.Empty);

            featureProvider.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<bool>("test", false));

            hook.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(),
                It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()));

            hook.InSequence(sequence).Setup(x =>
                x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()));

            Api.Instance.SetProvider(featureProvider.Object);
            var client = Api.Instance.GetClient();
            client.AddHooks(hook.Object);

            await client.GetBooleanValue("test", false);

            hook.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            featureProvider.Verify(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()), Times.Once);
        }

        [Fact]
        [Specification("4.4.1", "The API, Client, Provider, and invocation MUST have a method for registering hooks.")]
        public async Task Register_Hooks_Should_Be_Available_At_All_Levels()
        {
            var hook1 = new Mock<Hook>(MockBehavior.Strict);
            var hook2 = new Mock<Hook>(MockBehavior.Strict);
            var hook3 = new Mock<Hook>(MockBehavior.Strict);
            var hook4 = new Mock<Hook>(MockBehavior.Strict);

            var testProvider = new TestProvider();
            testProvider.AddHook(hook4.Object);
            Api.Instance.AddHooks(hook1.Object);
            Api.Instance.SetProvider(testProvider);
            var client = Api.Instance.GetClient();
            client.AddHooks(hook2.Object);
            await client.GetBooleanValue("test", false, null,
                new FlagEvaluationOptions(hook3.Object, ImmutableDictionary<string, object>.Empty));

            Assert.Single(Api.Instance.GetHooks());
            client.GetHooks().Count().Should().Be(1);
            testProvider.GetProviderHooks().Count.Should().Be(1);
        }

        [Fact]
        [Specification("4.4.3", "If a `finally` hook abnormally terminates, evaluation MUST proceed, including the execution of any remaining `finally` hooks.")]
        public async Task Finally_Hook_Should_Be_Executed_Even_If_Abnormal_Termination()
        {
            var featureProvider = new Mock<FeatureProvider>(MockBehavior.Strict);
            var hook1 = new Mock<Hook>(MockBehavior.Strict);
            var hook2 = new Mock<Hook>(MockBehavior.Strict);

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            featureProvider.Setup(x => x.GetProviderHooks())
                .Returns(ImmutableList<Hook>.Empty);

            hook1.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null))
                .ReturnsAsync(EvaluationContext.Empty);

            hook2.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null))
                .ReturnsAsync(EvaluationContext.Empty);

            featureProvider.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<bool>("test", false));

            hook2.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(),
                It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), null))
                .Returns(Task.CompletedTask);

            hook1.InSequence(sequence).Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(),
                It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), null))
                .Returns(Task.CompletedTask);

            hook2.InSequence(sequence).Setup(x =>
                    x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), null))
                .Returns(Task.CompletedTask);

            hook1.InSequence(sequence).Setup(x =>
                    x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), null))
                .Throws(new Exception());

            Api.Instance.SetProvider(featureProvider.Object);
            var client = Api.Instance.GetClient();
            client.AddHooks(new[] { hook1.Object, hook2.Object });
            client.GetHooks().Count().Should().Be(2);

            await client.GetBooleanValue("test", false);

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
            hook2.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
            featureProvider.Verify(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()), Times.Once);
            hook2.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), null), Times.Once);
            hook1.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), null), Times.Once);
            hook2.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
            hook1.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
        }

        [Fact]
        [Specification("4.4.4", "If an `error` hook abnormally terminates, evaluation MUST proceed, including the execution of any remaining `error` hooks.")]
        public async Task Error_Hook_Should_Be_Executed_Even_If_Abnormal_Termination()
        {
            var featureProvider1 = new Mock<FeatureProvider>(MockBehavior.Strict);
            var hook1 = new Mock<Hook>(MockBehavior.Strict);
            var hook2 = new Mock<Hook>(MockBehavior.Strict);

            var sequence = new MockSequence();

            featureProvider1.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));
            featureProvider1.Setup(x => x.GetProviderHooks())
                .Returns(ImmutableList<Hook>.Empty);

            hook1.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null))
                .ReturnsAsync(EvaluationContext.Empty);

            hook2.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null))
                .ReturnsAsync(EvaluationContext.Empty);

            featureProvider1.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()))
                .Throws(new Exception());

            hook2.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null))
                .Returns(Task.CompletedTask);

            hook1.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null))
                .Returns(Task.CompletedTask);

            Api.Instance.SetProvider(featureProvider1.Object);
            var client = Api.Instance.GetClient();
            client.AddHooks(new[] { hook1.Object, hook2.Object });

            await client.GetBooleanValue("test", false);

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
            hook2.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
            hook1.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
            hook2.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
        }

        [Fact]
        [Specification("4.4.6", "If an error occurs during the evaluation of `before` or `after` hooks, any remaining hooks in the `before` or `after` stages MUST NOT be invoked.")]
        public async Task Error_Occurs_During_Before_After_Evaluation_Should_Not_Invoke_Any_Remaining_Hooks()
        {
            var featureProvider = new Mock<FeatureProvider>(MockBehavior.Strict);
            var hook1 = new Mock<Hook>(MockBehavior.Strict);
            var hook2 = new Mock<Hook>(MockBehavior.Strict);

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));
            featureProvider.Setup(x => x.GetProviderHooks())
                .Returns(ImmutableList<Hook>.Empty);

            hook1.InSequence(sequence).Setup(x =>
                    x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ThrowsAsync(new Exception());

            hook1.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null));

            hook2.InSequence(sequence).Setup(x =>
                    x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null));

            Api.Instance.SetProvider(featureProvider.Object);
            var client = Api.Instance.GetClient();
            client.AddHooks(new[] { hook1.Object, hook2.Object });

            await client.GetBooleanValue("test", false);

            hook1.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            hook2.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
            hook1.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
            hook2.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null), Times.Once);
        }

        [Fact]
        [Specification("4.5.1", "`Flag evaluation options` MAY contain `hook hints`, a map of data to be provided to hook invocations.")]
        public async Task Hook_Hints_May_Be_Optional()
        {
            var featureProvider = new Mock<FeatureProvider>(MockBehavior.Strict);
            var hook = new Mock<Hook>(MockBehavior.Strict);
            var defaultEmptyHookHints = new Dictionary<string, object>();
            var flagOptions = new FlagEvaluationOptions(hook.Object);
            EvaluationContext evaluationContext = null;

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            featureProvider.Setup(x => x.GetProviderHooks())
                .Returns(ImmutableList<Hook>.Empty);

            hook.InSequence(sequence)
                .Setup(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints))
                .ReturnsAsync(evaluationContext);

            featureProvider.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<bool>("test", false));

            hook.InSequence(sequence)
                .Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), defaultEmptyHookHints))
                .Returns(Task.CompletedTask);

            hook.InSequence(sequence)
                .Setup(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints))
                .Returns(Task.CompletedTask);

            Api.Instance.SetProvider(featureProvider.Object);
            var client = Api.Instance.GetClient();

            await client.GetBooleanValue("test", false, config: flagOptions);

            hook.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints), Times.Once);
            hook.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), defaultEmptyHookHints), Times.Once);
            hook.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints), Times.Once);
            featureProvider.Verify(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()), Times.Once);
        }

        [Fact]
        [Specification("4.4.5", "If an error occurs in the `before` or `after` hooks, the `error` hooks MUST be invoked.")]
        [Specification("4.4.7", "If an error occurs in the `before` hooks, the default value MUST be returned.")]
        public async Task When_Error_Occurs_In_Before_Hook_Should_Return_Default_Value()
        {
            var featureProvider = new Mock<FeatureProvider>(MockBehavior.Strict);
            var hook = new Mock<Hook>(MockBehavior.Strict);
            var exceptionToThrow = new Exception("Fails during default");

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            hook.InSequence(sequence)
                .Setup(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Dictionary<string, object>>()))
                .ThrowsAsync(exceptionToThrow);

            hook.InSequence(sequence)
                .Setup(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), null)).Returns(Task.CompletedTask);

            hook.InSequence(sequence)
                .Setup(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), null)).Returns(Task.CompletedTask);

            var client = Api.Instance.GetClient();
            client.AddHooks(hook.Object);

            var resolvedFlag = await client.GetBooleanValue("test", true);

            resolvedFlag.Should().BeTrue();
            hook.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
            hook.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), exceptionToThrow, null), Times.Once);
            hook.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), null), Times.Once);
        }

        [Fact]
        [Specification("4.4.5", "If an error occurs in the `before` or `after` hooks, the `error` hooks MUST be invoked.")]
        public async Task When_Error_Occurs_In_After_Hook_Should_Invoke_Error_Hook()
        {
            var featureProvider = new Mock<FeatureProvider>(MockBehavior.Strict);
            var hook = new Mock<Hook>(MockBehavior.Strict);
            var defaultEmptyHookHints = new Dictionary<string, object>();
            var flagOptions = new FlagEvaluationOptions(hook.Object);
            var exceptionToThrow = new Exception("Fails during default");
            EvaluationContext evaluationContext = null;

            var sequence = new MockSequence();

            featureProvider.Setup(x => x.GetMetadata())
                .Returns(new Metadata(null));

            featureProvider.Setup(x => x.GetProviderHooks())
                .Returns(ImmutableList<Hook>.Empty);

            hook.InSequence(sequence)
                .Setup(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints))
                .ReturnsAsync(evaluationContext);

            featureProvider.InSequence(sequence)
                .Setup(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()))
                .ReturnsAsync(new ResolutionDetails<bool>("test", false));

            hook.InSequence(sequence)
                .Setup(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), defaultEmptyHookHints))
                .ThrowsAsync(exceptionToThrow);

            hook.InSequence(sequence)
                .Setup(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<Exception>(), defaultEmptyHookHints))
                .Returns(Task.CompletedTask);

            hook.InSequence(sequence)
                .Setup(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints))
                .Returns(Task.CompletedTask);

            Api.Instance.SetProvider(featureProvider.Object);
            var client = Api.Instance.GetClient();

            var resolvedFlag = await client.GetBooleanValue("test", true, config: flagOptions);

            resolvedFlag.Should().BeTrue();
            hook.Verify(x => x.Before(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints), Times.Once);
            hook.Verify(x => x.After(It.IsAny<HookContext<It.IsAnyType>>(), It.IsAny<FlagEvaluationDetails<It.IsAnyType>>(), defaultEmptyHookHints), Times.Once);
            hook.Verify(x => x.Error(It.IsAny<HookContext<It.IsAnyType>>(), exceptionToThrow, defaultEmptyHookHints), Times.Once);
            hook.Verify(x => x.Finally(It.IsAny<HookContext<It.IsAnyType>>(), defaultEmptyHookHints), Times.Once);
            featureProvider.Verify(x => x.ResolveBooleanValue(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<EvaluationContext>()), Times.Once);
        }
    }
}
