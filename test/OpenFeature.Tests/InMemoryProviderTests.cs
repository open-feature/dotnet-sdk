using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Xunit;

namespace OpenFeature.Tests
{
    // most of the in-memory tests are handled in the e2e suite
    public class InMemoryProviderTests
    {
        [Fact]
        public async void PutConfiguration_shouldUpdateConfigAndRunHandlers()
        {
            var handlerRuns = 0;
            var provider = new InMemoryProvider(new Dictionary<string, Flag>(){
            {
                "boolean-flag", new Flag<bool>(
                    variants: new Dictionary<string, bool>(){
                        { "on", true },
                        { "off", false }
                    },
                    defaultVariant: "on"
                )
            }});

            // setup client and handler and run initial eval
            await Api.Instance.SetProviderAsync("mem-test", provider).ConfigureAwait(false);
            var client = Api.Instance.GetClient("mem-test");
            client.AddHandler(ProviderEventTypes.ProviderConfigurationChanged, (details) =>
            {
                handlerRuns++;
            });
            Assert.True(await client.GetBooleanValue("boolean-flag", false).ConfigureAwait(false));

            // update flags
            await provider.UpdateFlags(new Dictionary<string, Flag>(){
            {
                "string-flag", new Flag<string>(
                    variants: new Dictionary<string, string>(){
                        { "greeting", "hi" },
                        { "parting", "bye" }
                    },
                    defaultVariant: "greeting"
                )
            }}).ConfigureAwait(false);

            // new flag should be present, old gone (defaults), handler run.
            Assert.Equal("hi", await client.GetStringValue("string-flag", "nope").ConfigureAwait(false));
            Assert.False(await client.GetBooleanValue("boolean-flag", false).ConfigureAwait(false));
            Assert.Equal(1, handlerRuns);
        }
    }
}
