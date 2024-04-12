using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
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
            var apiHook = Substitute.For<Hook>();
            var clientHook = Substitute.For<Hook>();
            var invocationHook = Substitute.For<Hook>();
            var providerHook = Substitute.For<Hook>();

            // Sequence
            apiHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(EvaluationContext.Empty);
            clientHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(EvaluationContext.Empty);
            invocationHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(EvaluationContext.Empty);
            providerHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(EvaluationContext.Empty);
            providerHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);
            invocationHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);
            clientHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);
            apiHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);
            providerHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);
            invocationHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);
            clientHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);
            apiHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(Task.CompletedTask);

            var testProvider = new TestProvider();
            testProvider.AddHook(providerHook);
            Api.Instance.AddHooks(apiHook);
            await Api.Instance.SetProviderAsync(testProvider);
            var client = Api.Instance.GetClient(clientName, clientVersion);
            client.AddHooks(clientHook);

            await client.GetBooleanValue(flagName, defaultValue, EvaluationContext.Empty,
                new FlagEvaluationOptions(invocationHook, ImmutableDictionary<string, object>.Empty));

            Received.InOrder(() =>
            {
                apiHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                clientHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                invocationHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                providerHook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                providerHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                invocationHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                clientHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                apiHook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                providerHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                invocationHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                clientHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
                apiHook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            });

            _ = apiHook.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = clientHook.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = invocationHook.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = providerHook.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = providerHook.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = invocationHook.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = clientHook.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = apiHook.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = providerHook.Received(1).Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = invocationHook.Received(1).Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = clientHook.Received(1).Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
            _ = apiHook.Received(1).Finally(Arg.Any<HookContext<bool>>(), Arg.Any<IReadOnlyDictionary<string, object>>());
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
            var hook1 = Substitute.For<Hook>();
            var hook2 = Substitute.For<Hook>();
            var hookContext = new HookContext<bool>("test", false,
                FlagValueType.Boolean, new ClientMetadata("test", "1.0.0"), new Metadata(NoOpProvider.NoOpProviderName),
                evaluationContext);

            hook1.Before(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>()).Returns(evaluationContext);
            hook2.Before(hookContext, Arg.Any<ImmutableDictionary<string, object>>()).Returns(evaluationContext);

            var client = Api.Instance.GetClient("test", "1.0.0");
            await client.GetBooleanValue("test", false, EvaluationContext.Empty,
                new FlagEvaluationOptions(ImmutableList.Create(hook1, hook2), ImmutableDictionary<string, object>.Empty));

            _ = hook1.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>());
            _ = hook2.Received(1).Before(Arg.Is<HookContext<bool>>(a => a.EvaluationContext.GetValue("test").AsString == "test"), Arg.Any<ImmutableDictionary<string, object>>());
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

            var provider = Substitute.For<FeatureProvider>();

            provider.GetMetadata().Returns(new Metadata(null));

            provider.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            provider.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<bool>("test", true));

            await Api.Instance.SetProviderAsync(provider);

            var hook = Substitute.For<Hook>();
            hook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>()).Returns(hookContext);


            var client = Api.Instance.GetClient("test", "1.0.0", null, clientContext);
            await client.GetBooleanValue("test", false, invocationContext, new FlagEvaluationOptions(ImmutableList.Create(hook), ImmutableDictionary<string, object>.Empty));

            // after proper merging, all properties should equal true
            _ = provider.Received(1).ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Is<EvaluationContext>(y =>
                (y.GetValue(propGlobal).AsBoolean ?? false)
                && (y.GetValue(propClient).AsBoolean ?? false)
                && (y.GetValue(propGlobalToOverwrite).AsBoolean ?? false)
                && (y.GetValue(propInvocation).AsBoolean ?? false)
                && (y.GetValue(propClientToOverwrite).AsBoolean ?? false)
                && (y.GetValue(propHook).AsBoolean ?? false)
                && (y.GetValue(propInvocationToOverwrite).AsBoolean ?? false)
            ));
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
            var featureProvider = Substitute.For<FeatureProvider>();
            var hook = Substitute.For<Hook>();

            featureProvider.GetMetadata().Returns(new Metadata(null));
            featureProvider.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            // Sequence
            hook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>()).Returns(EvaluationContext.Empty);
            featureProvider.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<bool>("test", false));
            _ = hook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<Dictionary<string, object>>());
            _ = hook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());

            await Api.Instance.SetProviderAsync(featureProvider);
            var client = Api.Instance.GetClient();
            client.AddHooks(hook);

            await client.GetBooleanValue("test", false);

            Received.InOrder(() =>
            {
                hook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
                featureProvider.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>());
                hook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<Dictionary<string, object>>());
                hook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
            });

            _ = hook.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
            _ = hook.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<Dictionary<string, object>>());
            _ = hook.Received(1).Finally(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
            _ = featureProvider.Received(1).ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>());
        }

        [Fact]
        [Specification("4.4.1", "The API, Client, Provider, and invocation MUST have a method for registering hooks.")]
        public async Task Register_Hooks_Should_Be_Available_At_All_Levels()
        {
            var hook1 = Substitute.For<Hook>();
            var hook2 = Substitute.For<Hook>();
            var hook3 = Substitute.For<Hook>();
            var hook4 = Substitute.For<Hook>();

            var testProvider = new TestProvider();
            testProvider.AddHook(hook4);
            Api.Instance.AddHooks(hook1);
            await Api.Instance.SetProviderAsync(testProvider);
            var client = Api.Instance.GetClient();
            client.AddHooks(hook2);
            await client.GetBooleanValue("test", false, null,
                new FlagEvaluationOptions(hook3, ImmutableDictionary<string, object>.Empty));

            Assert.Single(Api.Instance.GetHooks());
            client.GetHooks().Count().Should().Be(1);
            testProvider.GetProviderHooks().Count.Should().Be(1);
        }

        [Fact]
        [Specification("4.4.3", "If a `finally` hook abnormally terminates, evaluation MUST proceed, including the execution of any remaining `finally` hooks.")]
        public async Task Finally_Hook_Should_Be_Executed_Even_If_Abnormal_Termination()
        {
            var featureProvider = Substitute.For<FeatureProvider>();
            var hook1 = Substitute.For<Hook>();
            var hook2 = Substitute.For<Hook>();

            featureProvider.GetMetadata().Returns(new Metadata(null));
            featureProvider.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            // Sequence
            hook1.Before(Arg.Any<HookContext<bool>>(), null).Returns(EvaluationContext.Empty);
            hook2.Before(Arg.Any<HookContext<bool>>(), null).Returns(EvaluationContext.Empty);
            featureProvider.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>()).Returns(new ResolutionDetails<bool>("test", false));
            hook2.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), null).Returns(Task.CompletedTask);
            hook1.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), null).Returns(Task.CompletedTask);
            hook2.Finally(Arg.Any<HookContext<bool>>(), null).Returns(Task.CompletedTask);
            hook1.Finally(Arg.Any<HookContext<bool>>(), null).Throws(new Exception());

            await Api.Instance.SetProviderAsync(featureProvider);
            var client = Api.Instance.GetClient();
            client.AddHooks(new[] { hook1, hook2 });
            client.GetHooks().Count().Should().Be(2);

            await client.GetBooleanValue("test", false);

            Received.InOrder(() =>
            {
                hook1.Before(Arg.Any<HookContext<bool>>(), null);
                hook2.Before(Arg.Any<HookContext<bool>>(), null);
                featureProvider.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>());
                hook2.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), null);
                hook1.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), null);
                hook2.Finally(Arg.Any<HookContext<bool>>(), null);
                hook1.Finally(Arg.Any<HookContext<bool>>(), null);
            });

            _ = hook1.Received(1).Before(Arg.Any<HookContext<bool>>(), null);
            _ = hook2.Received(1).Before(Arg.Any<HookContext<bool>>(), null);
            _ = featureProvider.Received(1).ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>());
            _ = hook2.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), null);
            _ = hook1.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), null);
            _ = hook2.Received(1).Finally(Arg.Any<HookContext<bool>>(), null);
            _ = hook1.Received(1).Finally(Arg.Any<HookContext<bool>>(), null);
        }

        [Fact]
        [Specification("4.4.4", "If an `error` hook abnormally terminates, evaluation MUST proceed, including the execution of any remaining `error` hooks.")]
        public async Task Error_Hook_Should_Be_Executed_Even_If_Abnormal_Termination()
        {
            var featureProvider1 = Substitute.For<FeatureProvider>();
            var hook1 = Substitute.For<Hook>();
            var hook2 = Substitute.For<Hook>();

            featureProvider1.GetMetadata().Returns(new Metadata(null));
            featureProvider1.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            // Sequence
            hook1.Before(Arg.Any<HookContext<bool>>(), null).Returns(EvaluationContext.Empty);
            hook2.Before(Arg.Any<HookContext<bool>>(), null).Returns(EvaluationContext.Empty);
            featureProvider1.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>()).Throws(new Exception());
            hook2.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null).Returns(Task.CompletedTask);
            hook1.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null).Returns(Task.CompletedTask);

            await Api.Instance.SetProviderAsync(featureProvider1);
            var client = Api.Instance.GetClient();
            client.AddHooks(new[] { hook1, hook2 });

            await client.GetBooleanValue("test", false);

            Received.InOrder(() =>
            {
                hook1.Before(Arg.Any<HookContext<bool>>(), null);
                hook2.Before(Arg.Any<HookContext<bool>>(), null);
                featureProvider1.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>());
                hook2.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
                hook1.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
            });

            _ = hook1.Received(1).Before(Arg.Any<HookContext<bool>>(), null);
            _ = hook2.Received(1).Before(Arg.Any<HookContext<bool>>(), null);
            _ = hook1.Received(1).Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
            _ = hook2.Received(1).Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
        }

        [Fact]
        [Specification("4.4.6", "If an error occurs during the evaluation of `before` or `after` hooks, any remaining hooks in the `before` or `after` stages MUST NOT be invoked.")]
        public async Task Error_Occurs_During_Before_After_Evaluation_Should_Not_Invoke_Any_Remaining_Hooks()
        {
            var featureProvider = Substitute.For<FeatureProvider>();
            var hook1 = Substitute.For<Hook>();
            var hook2 = Substitute.For<Hook>();

            featureProvider.GetMetadata().Returns(new Metadata(null));
            featureProvider.GetProviderHooks().Returns(ImmutableList<Hook>.Empty);

            // Sequence
            hook1.Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>()).ThrowsAsync(new Exception());
            _ = hook1.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
            _ = hook2.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);

            await Api.Instance.SetProviderAsync(featureProvider);
            var client = Api.Instance.GetClient();
            client.AddHooks(new[] { hook1, hook2 });

            await client.GetBooleanValue("test", false);

            Received.InOrder(() =>
            {
                hook1.Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
                hook2.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
                hook1.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
            });

            _ = hook1.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
            _ = hook2.DidNotReceive().Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
            _ = hook1.Received(1).Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
            _ = hook2.Received(1).Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
        }

        [Fact]
        [Specification("4.5.1", "`Flag evaluation options` MAY contain `hook hints`, a map of data to be provided to hook invocations.")]
        public async Task Hook_Hints_May_Be_Optional()
        {
            var featureProvider = Substitute.For<FeatureProvider>();
            var hook = Substitute.For<Hook>();
            var flagOptions = new FlagEvaluationOptions(hook);

            featureProvider.GetMetadata()
                .Returns(new Metadata(null));

            featureProvider.GetProviderHooks()
                .Returns(ImmutableList<Hook>.Empty);

            hook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>())
                .Returns(EvaluationContext.Empty);

            featureProvider.ResolveBooleanValue("test", false, Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<bool>("test", false));

            hook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<ImmutableDictionary<string, object>>())
                .Returns(Task.FromResult(Task.CompletedTask));

            hook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>())
                .Returns(Task.CompletedTask);

            await Api.Instance.SetProviderAsync(featureProvider);
            var client = Api.Instance.GetClient();

            await client.GetBooleanValue("test", false, EvaluationContext.Empty, flagOptions);

            Received.InOrder(() =>
            {
                hook.Received().Before(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>());
                featureProvider.Received().ResolveBooleanValue("test", false, Arg.Any<EvaluationContext>());
                hook.Received().After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<ImmutableDictionary<string, object>>());
                hook.Received().Finally(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>());
            });
        }

        [Fact]
        [Specification("4.4.5", "If an error occurs in the `before` or `after` hooks, the `error` hooks MUST be invoked.")]
        [Specification("4.4.7", "If an error occurs in the `before` hooks, the default value MUST be returned.")]
        public async Task When_Error_Occurs_In_Before_Hook_Should_Return_Default_Value()
        {
            var featureProvider = Substitute.For<FeatureProvider>();
            var hook = Substitute.For<Hook>();
            var exceptionToThrow = new Exception("Fails during default");

            featureProvider.GetMetadata().Returns(new Metadata(null));

            // Sequence
            hook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>()).ThrowsAsync(exceptionToThrow);
            hook.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null).Returns(Task.CompletedTask);
            hook.Finally(Arg.Any<HookContext<bool>>(), null).Returns(Task.CompletedTask);

            var client = Api.Instance.GetClient();
            client.AddHooks(hook);

            var resolvedFlag = await client.GetBooleanValue("test", true);

            Received.InOrder(() =>
            {
                hook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<Dictionary<string, object>>());
                hook.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), null);
                hook.Finally(Arg.Any<HookContext<bool>>(), null);
            });

            resolvedFlag.Should().BeTrue();
            _ = hook.Received(1).Before(Arg.Any<HookContext<bool>>(), null);
            _ = hook.Received(1).Error(Arg.Any<HookContext<bool>>(), exceptionToThrow, null);
            _ = hook.Received(1).Finally(Arg.Any<HookContext<bool>>(), null);
        }

        [Fact]
        [Specification("4.4.5", "If an error occurs in the `before` or `after` hooks, the `error` hooks MUST be invoked.")]
        public async Task When_Error_Occurs_In_After_Hook_Should_Invoke_Error_Hook()
        {
            var featureProvider = Substitute.For<FeatureProvider>();
            var hook = Substitute.For<Hook>();
            var flagOptions = new FlagEvaluationOptions(hook);
            var exceptionToThrow = new Exception("Fails during default");

            featureProvider.GetMetadata()
                .Returns(new Metadata(null));

            featureProvider.GetProviderHooks()
                .Returns(ImmutableList<Hook>.Empty);

            hook.Before(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>())
                .Returns(EvaluationContext.Empty);

            featureProvider.ResolveBooleanValue(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>())
                .Returns(new ResolutionDetails<bool>("test", false));

            hook.After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<ImmutableDictionary<string, object>>())
                .ThrowsAsync(exceptionToThrow);

            hook.Error(Arg.Any<HookContext<bool>>(), Arg.Any<Exception>(), Arg.Any<ImmutableDictionary<string, object>>())
                .Returns(Task.CompletedTask);

            hook.Finally(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>())
                .Returns(Task.CompletedTask);

            await Api.Instance.SetProviderAsync(featureProvider);
            var client = Api.Instance.GetClient();

            var resolvedFlag = await client.GetBooleanValue("test", true, config: flagOptions);

            resolvedFlag.Should().BeTrue();

            Received.InOrder(() =>
            {
                hook.Received(1).Before(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>());
                hook.Received(1).After(Arg.Any<HookContext<bool>>(), Arg.Any<FlagEvaluationDetails<bool>>(), Arg.Any<ImmutableDictionary<string, object>>());
                hook.Received(1).Finally(Arg.Any<HookContext<bool>>(), Arg.Any<ImmutableDictionary<string, object>>());
            });

            await featureProvider.DidNotReceive().ResolveBooleanValue("test", false, Arg.Any<EvaluationContext>());
        }

        [Fact]
        public void Add_hooks_should_accept_empty_enumerable()
        {
            Api.Instance.ClearHooks();
            Api.Instance.AddHooks(Enumerable.Empty<Hook>());
        }
    }
}
