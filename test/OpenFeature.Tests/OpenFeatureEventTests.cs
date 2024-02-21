using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Tests.Internal;
using Xunit;

namespace OpenFeature.Tests
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public class OpenFeatureEventTest : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        public async Task Event_Executor_Should_Propagate_Events_ToGlobal_Handler()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var eventExecutor = new EventExecutor();

            eventExecutor.AddApiLevelHandler(ProviderEventTypes.ProviderConfigurationChanged, eventHandler);

            var eventMetadata = new ImmutableMetadata(new Dictionary<string, object> { { "foo", "bar" } });
            var myEvent = new Event
            {
                EventPayload = new ProviderEventPayload
                {
                    Type = ProviderEventTypes.ProviderConfigurationChanged,
                    Message = "The provider is ready",
                    EventMetadata = eventMetadata,
                    FlagsChanged = new List<string>
                    {
                        "flag1", "flag2"
                    }
                }
            };
            eventExecutor.EventChannel.Writer.TryWrite(myEvent);

            Thread.Sleep(1000);

            eventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload == myEvent.EventPayload));

            // shut down the event executor
            await eventExecutor.Shutdown();

            // the next event should not be propagated to the event handler
            var newEventPayload = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderStale
            };

            eventExecutor.EventChannel.Writer.TryWrite(newEventPayload);

            eventHandler.DidNotReceive().Invoke(newEventPayload);

            eventHandler.DidNotReceive().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.Type == ProviderEventTypes.ProviderStale));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.2", "The `API` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        public async Task API_Level_Event_Handlers_Should_Be_Registered()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);
            Api.Instance.AddHandler(ProviderEventTypes.ProviderConfigurationChanged, eventHandler);
            Api.Instance.AddHandler(ProviderEventTypes.ProviderError, eventHandler);
            Api.Instance.AddHandler(ProviderEventTypes.ProviderStale, eventHandler);

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(testProvider);

            testProvider.SendEvent(ProviderEventTypes.ProviderConfigurationChanged);
            testProvider.SendEvent(ProviderEventTypes.ProviderError);
            testProvider.SendEvent(ProviderEventTypes.ProviderStale);

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderReady
                    )));

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderConfigurationChanged
                    )));

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderError
                    )));

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderStale
                    )));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.2", "The `API` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.3.1", "If the provider's `initialize` function terminates normally, `PROVIDER_READY` handlers MUST run.")]
        [Specification("5.3.3", "Handlers attached after the provider is already in the associated state, MUST run immediately.")]
        public async Task API_Level_Event_Handlers_Should_Be_Informed_About_Ready_State_After_Registering_Provider_Ready()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(testProvider);

            Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderReady
                    )));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.2", "The `API` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.3.1", "If the provider's `initialize` function terminates normally, `PROVIDER_READY` handlers MUST run.")]
        [Specification("5.3.3", "Handlers attached after the provider is already in the associated state, MUST run immediately.")]
        public async Task API_Level_Event_Handlers_Should_Be_Informed_About_Ready_State_After_Registering_Provider_Ready_Sync()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var testProvider = new TestProvider();
#pragma warning disable CS0618// Type or member is obsolete
            Api.Instance.SetProvider(testProvider);
#pragma warning restore CS0618// Type or member is obsolete

            Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderReady
                    )));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.2", "The `API` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.3.2", "If the provider's `initialize` function terminates abnormally, `PROVIDER_ERROR` handlers MUST run.")]
        [Specification("5.3.3", "Handlers attached after the provider is already in the associated state, MUST run immediately.")]
        public async Task API_Level_Event_Handlers_Should_Be_Informed_About_Error_State_After_Registering_Provider_Error()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(testProvider);

            testProvider.SetStatus(ProviderStatus.Error);

            Api.Instance.AddHandler(ProviderEventTypes.ProviderError, eventHandler);

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderError
                    )));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.2", "The `API` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.3.3", "Handlers attached after the provider is already in the associated state, MUST run immediately.")]
        public async Task API_Level_Event_Handlers_Should_Be_Informed_About_Stale_State_After_Registering_Provider_Stale()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(testProvider);

            testProvider.SetStatus(ProviderStatus.Stale);

            Api.Instance.AddHandler(ProviderEventTypes.ProviderStale, eventHandler);

            await Utils.AssertUntilAsync(_ => eventHandler
                .Received()
                .Invoke(
                    Arg.Is<ProviderEventPayload>(
                        payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderStale
                    )));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.2", "The `API` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.2.6", "Event handlers MUST persist across `provider` changes.")]
        public async Task API_Level_Event_Handlers_Should_Be_Exchangeable()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);
            Api.Instance.AddHandler(ProviderEventTypes.ProviderConfigurationChanged, eventHandler);

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(testProvider);

            testProvider.SendEvent(ProviderEventTypes.ProviderConfigurationChanged);

            var newTestProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(newTestProvider);

            newTestProvider.SendEvent(ProviderEventTypes.ProviderConfigurationChanged);

            await Utils.AssertUntilAsync(
                _ => eventHandler.Received(2).Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderReady))
            );
            await Utils.AssertUntilAsync(
                _ => eventHandler.Received(2).Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderConfigurationChanged))
            );
        }

        [Fact]
        [Specification("5.2.2", "The `API` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.2.7", "The `API` and `client` MUST provide a function allowing the removal of event handlers.")]
        public async Task API_Level_Event_Handlers_Should_Be_Removable()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(testProvider);

            Thread.Sleep(1000);
            Api.Instance.RemoveHandler(ProviderEventTypes.ProviderReady, eventHandler);

            var newTestProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(newTestProvider);

            eventHandler.Received(1).Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.1", "The `client` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.2.5", "If a `handler function` terminates abnormally, other `handler` functions MUST run.")]
        public async Task API_Level_Event_Handlers_Should_Be_Executed_When_Other_Handler_Fails()
        {
            var fixture = new Fixture();

            var failingEventHandler = Substitute.For<EventHandlerDelegate>();
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            failingEventHandler.When(x => x.Invoke(Arg.Any<ProviderEventPayload>()))
                .Do(x => throw new Exception());

            Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, failingEventHandler);
            Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            var testProvider = new TestProvider(fixture.Create<string>());
            await Api.Instance.SetProviderAsync(testProvider);

            await Utils.AssertUntilAsync(
                _ => failingEventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name))
            );
            await Utils.AssertUntilAsync(
                _ => eventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name))
            );
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.1", "The `client` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        public async Task Client_Level_Event_Handlers_Should_Be_Registered()
        {
            var fixture = new Fixture();
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var myClient = Api.Instance.GetClient(clientName, clientVersion);

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(myClient.GetMetadata().Name!, testProvider);

            myClient.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            eventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.1", "The `client` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.2.5", "If a `handler function` terminates abnormally, other `handler` functions MUST run.")]
        public async Task Client_Level_Event_Handlers_Should_Be_Executed_When_Other_Handler_Fails()
        {
            var fixture = new Fixture();

            var failingEventHandler = Substitute.For<EventHandlerDelegate>();
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            failingEventHandler.When(x => x.Invoke(Arg.Any<ProviderEventPayload>()))
                .Do(x => throw new Exception());

            var clientName = fixture.Create<string>();
            var clientVersion = fixture.Create<string>();
            var myClient = Api.Instance.GetClient(clientName, clientVersion);

            myClient.AddHandler(ProviderEventTypes.ProviderReady, failingEventHandler);
            myClient.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(myClient.GetMetadata().Name!, testProvider);

            await Utils.AssertUntilAsync(
                _ => failingEventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name))
            );
            await Utils.AssertUntilAsync(
                _ => eventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name))
            );
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.1.3", "When a `provider` signals the occurrence of a particular `event`, event handlers on clients which are not associated with that provider MUST NOT run.")]
        [Specification("5.2.1", "The `client` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        public async Task Client_Level_Event_Handlers_Should_Be_Registered_To_Default_Provider()
        {
            var fixture = new Fixture();
            var eventHandler = Substitute.For<EventHandlerDelegate>();
            var clientEventHandler = Substitute.For<EventHandlerDelegate>();

            var myClientWithNoBoundProvider = Api.Instance.GetClient(fixture.Create<string>(), fixture.Create<string>());
            var myClientWithBoundProvider = Api.Instance.GetClient(fixture.Create<string>(), fixture.Create<string>());

            var apiProvider = new TestProvider(fixture.Create<string>());
            var clientProvider = new TestProvider(fixture.Create<string>());

            // set the default provider on API level, but not specifically to the client
            await Api.Instance.SetProviderAsync(apiProvider);
            // set the other provider specifically for the client
            await Api.Instance.SetProviderAsync(myClientWithBoundProvider.GetMetadata().Name!, clientProvider);

            myClientWithNoBoundProvider.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);
            myClientWithBoundProvider.AddHandler(ProviderEventTypes.ProviderReady, clientEventHandler);

            eventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == apiProvider.GetMetadata().Name));
            eventHandler.DidNotReceive().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == clientProvider.GetMetadata().Name));

            clientEventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == clientProvider.GetMetadata().Name));
            clientEventHandler.DidNotReceive().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == apiProvider.GetMetadata().Name));
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.1.3", "When a `provider` signals the occurrence of a particular `event`, event handlers on clients which are not associated with that provider MUST NOT run.")]
        [Specification("5.2.1", "The `client` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.2.6", "Event handlers MUST persist across `provider` changes.")]
        public async Task Client_Level_Event_Handlers_Should_Be_Receive_Events_From_Named_Provider_Instead_of_Default()
        {
            var fixture = new Fixture();
            var clientEventHandler = Substitute.For<EventHandlerDelegate>();

            var client = Api.Instance.GetClient(fixture.Create<string>(), fixture.Create<string>());

            var defaultProvider = new TestProvider(fixture.Create<string>());
            var clientProvider = new TestProvider(fixture.Create<string>());

            // set the default provider
            await Api.Instance.SetProviderAsync(defaultProvider);

            client.AddHandler(ProviderEventTypes.ProviderConfigurationChanged, clientEventHandler);

            defaultProvider.SendEvent(ProviderEventTypes.ProviderConfigurationChanged);

            // verify that the client received the event from the default provider as there is no named provider registered yet
            await Utils.AssertUntilAsync(
                _ => clientEventHandler.Received(1)
                    .Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == defaultProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderConfigurationChanged))
            );

            // set the other provider specifically for the client
            await Api.Instance.SetProviderAsync(client.GetMetadata().Name!, clientProvider);

            // now, send another event for the default handler
            defaultProvider.SendEvent(ProviderEventTypes.ProviderConfigurationChanged);
            clientProvider.SendEvent(ProviderEventTypes.ProviderConfigurationChanged);

            // now the client should have received only the event from the named provider
            await Utils.AssertUntilAsync(
                _ => clientEventHandler.Received(1).Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == clientProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderConfigurationChanged))
            );
            // for the default provider, the number of received events should stay unchanged
            await Utils.AssertUntilAsync(
                _ => clientEventHandler.Received(1)
                    .Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == defaultProvider.GetMetadata().Name && payload.Type == ProviderEventTypes.ProviderConfigurationChanged))
            );
        }

        [Fact]
        [Specification("5.1.2", "When a `provider` signals the occurrence of a particular `event`, the associated `client` and `API` event handlers MUST run.")]
        [Specification("5.2.1", "The `client` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.3.1", "If the provider's `initialize` function terminates normally, `PROVIDER_READY` handlers MUST run.")]
        [Specification("5.3.3", "Handlers attached after the provider is already in the associated state, MUST run immediately.")]
        public async Task Client_Level_Event_Handlers_Should_Be_Informed_About_Ready_State_After_Registering()
        {
            var fixture = new Fixture();
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var myClient = Api.Instance.GetClient(fixture.Create<string>(), fixture.Create<string>());

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(myClient.GetMetadata().Name!, testProvider);

            // add the event handler after the provider has already transitioned into the ready state
            myClient.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            eventHandler.Received().Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name));
        }

        [Fact]
        [Specification("5.1.3", "When a `provider` signals the occurrence of a particular `event`, event handlers on clients which are not associated with that provider MUST NOT run.")]
        [Specification("5.2.1", "The `client` MUST provide a function for associating `handler functions` with a particular `provider event type`.")]
        [Specification("5.2.3", "The `event details` MUST contain the `provider name` associated with the event.")]
        [Specification("5.2.4", "The `handler function` MUST accept a `event details` parameter.")]
        [Specification("5.2.7", "The `API` and `client` MUST provide a function allowing the removal of event handlers.")]
        public async Task Client_Level_Event_Handlers_Should_Be_Removable()
        {
            var fixture = new Fixture();

            var eventHandler = Substitute.For<EventHandlerDelegate>();

            var myClient = Api.Instance.GetClient(fixture.Create<string>(), fixture.Create<string>());

            myClient.AddHandler(ProviderEventTypes.ProviderReady, eventHandler);

            var testProvider = new TestProvider();
            await Api.Instance.SetProviderAsync(myClient.GetMetadata().Name!, testProvider);

            // wait for the first event to be received
            await Utils.AssertUntilAsync(_ => myClient.RemoveHandler(ProviderEventTypes.ProviderReady, eventHandler));

            // send another event from the provider - this one should not be received
            testProvider.SendEvent(ProviderEventTypes.ProviderReady);

            // wait a bit and make sure we only have received the first event, but nothing after removing the event handler
            await Utils.AssertUntilAsync(
                _ => eventHandler.Received(1).Invoke(Arg.Is<ProviderEventPayload>(payload => payload.ProviderName == testProvider.GetMetadata().Name))
            );
        }

        [Fact]
        public void RegisterClientFeatureProvider_WhenCalledWithNullProvider_DoesNotThrowException()
        {
            // Arrange
            var eventExecutor = new EventExecutor();
            string client = "testClient";
            FeatureProvider? provider = null;

            // Act
            var exception = Record.Exception(() => eventExecutor.RegisterClientFeatureProvider(client, provider));

            // Assert
            Assert.Null(exception);
        }
    }
}
