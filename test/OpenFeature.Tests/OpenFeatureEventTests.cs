using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
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
    public class OpenFeatureEventTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        public async Task Event_Executor_Should_Propagate_Events_ToGlobal_Handler()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            eventHandler.Invoke(Arg.Any<ProviderEventPayload>());

            var eventExecutor = new EventExecutor();

            eventExecutor.AddApiLevelHandler(ProviderEventTypes.PROVIDER_READY, eventHandler);

            var eventPayload = new Event { EventPayload = new ProviderEventPayload { Type = ProviderEventTypes.PROVIDER_READY }};
            eventExecutor.eventChannel.Writer.TryWrite(eventPayload);


            // shut down the event executor
            await eventExecutor.SignalShutdownAsync();

            // the next event should not be propagated to the event handler
            var newEventPayload = new ProviderEventPayload { Type = ProviderEventTypes.PROVIDER_STALE };

            eventExecutor.eventChannel.Writer.TryWrite(newEventPayload);

            eventHandler.DidNotReceive().Invoke(newEventPayload);

            // verify the event handler received one event
            Received.InOrder(async () =>
            {
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
            });
        }

        [Fact]
        public async Task API_Level_Event_Handlers_Should_Be_Registered()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            eventHandler.Invoke(Arg.Any<ProviderEventPayload>());

            Api.Instance.AddHandler(ProviderEventTypes.PROVIDER_READY, eventHandler);

            var testProvider = new TestProvider();
            await Api.Instance.SetProvider(testProvider);

            Received.InOrder(async () =>
            {
                // first one due to NoOpProvider being in ready state
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
                // second one for the testProvider
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
            });

            Thread.Sleep(10000);
        }

        [Fact]
        public async Task API_Level_Event_Handlers_Should_Be_Informed_Aabout_Ready_State_After_Registering()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            eventHandler.Invoke(Arg.Any<ProviderEventPayload>());

            var testProvider = new TestProvider();
            await Api.Instance.SetProvider(testProvider);

            Api.Instance.AddHandler(ProviderEventTypes.PROVIDER_READY, eventHandler);

            Received.InOrder(async () =>
            {
                // first one due to NoOpProvider being in ready state
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
                // second one for the testProvider
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
            });
        }

        [Fact]
        public async Task API_Level_Event_Handlers_Should_Be_Exchangeable()
        {
            var eventHandler = Substitute.For<EventHandlerDelegate>();

            //eventHandler.Invoke(Arg.Any<ProviderEventPayload>());

            Api.Instance.AddHandler(ProviderEventTypes.PROVIDER_READY, eventHandler);

            var testProvider = new TestProvider();
            await Api.Instance.SetProvider(testProvider);

            var newTestProvider = new TestProvider();
            await Api.Instance.SetProvider(newTestProvider);

            Received.InOrder(async () =>
            {
                // first one due to NoOpProvider being in ready state
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
                // second one for the testProvider
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
                // third one for the new testProvider
                eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
            });
        }
    }
}