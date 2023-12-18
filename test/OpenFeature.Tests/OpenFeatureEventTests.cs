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
    public class OpenFeatureEventTests : ClearOpenFeatureInstanceFixture
    {
        [Fact]
        public async Task Event_Handlers_Should_Be_Added()
        {
            var fixture = new Fixture();
            
            var eventHandler = Substitute.For<EventHandlerDelegate>();
            
            eventHandler.Invoke(Arg.Any<ProviderEventPayload>());
            
            Api.Instance.AddHandler(ProviderEventTypes.PROVIDER_READY, eventHandler);
            
            var testProvider = new TestProvider();
            await Api.Instance.SetProvider(testProvider);
            
            var eventExecutor = new EventExecutor();
            
            eventExecutor.AddGlobalHandler(ProviderEventTypes.PROVIDER_READY, eventHandler);
            
            var eventPayload = new ProviderEventPayload { Type = ProviderEventTypes.PROVIDER_READY };
            eventExecutor.eventChannel.Writer.TryWrite(eventPayload);
            
            
            Received.InOrder(async () =>
            {
                eventHandler.Invoke(eventPayload);
            });
            foreach (var call in eventHandler.ReceivedCalls())
            {
                Assert.Equal(call.GetArguments()[0], eventPayload);
            }
            
            // shut down the event executor
            await eventExecutor.SignalShutdownAsync();
            
            // the next event should not be propagated to the event handler
            var newEventPayload = new ProviderEventPayload { Type = ProviderEventTypes.PROVIDER_STALE };
            
            eventExecutor.eventChannel.Writer.TryWrite(newEventPayload);
            
            eventHandler.DidNotReceive().Invoke(newEventPayload);
        }
    }
}