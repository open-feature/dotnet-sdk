using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Xunit;

namespace OpenFeature.Tests
{
    public class InMemoryProviderTests
    {
        private FeatureProvider commonProvider;

        public InMemoryProviderTests()
        {
            var provider = new InMemoryProvider(new Dictionary<string, Flag>(){
                {
                    "boolean-flag", new Flag<bool>(
                        variants: new Dictionary<string, bool>(){
                            { "on", true },
                            { "off", false }
                        },
                        defaultVariant: "on"
                    )
                },
                {
                    "string-flag", new Flag<string>(
                        variants: new Dictionary<string, string>(){
                            { "greeting", "hi" },
                            { "parting", "bye" }
                        },
                        defaultVariant: "greeting"
                    )
                },
                {
                    "integer-flag", new Flag<int>(
                        variants: new Dictionary<string, int>(){
                            { "one", 1 },
                            { "ten", 10 }
                        },
                        defaultVariant: "ten"
                    )
                },
                {
                    "float-flag", new Flag<double>(
                        variants: new Dictionary<string, double>(){
                            { "tenth", 0.1 },
                            { "half", 0.5 }
                        },
                        defaultVariant: "half"
                    )
                },
                {
                    "object-flag", new Flag<Value>(
                        variants: new Dictionary<string, Value>(){
                            { "empty", new Value() },
                            { "template", new Value(Structure.Builder()
                                    .Set("showImages", true)
                                    .Set("title", "Check out these pics!")
                                    .Set("imagesPerPage", 100).Build()
                                )
                            }
                        },
                        defaultVariant: "template"
                    )
                }
            });

            this.commonProvider = provider;
        }

        [Fact]
        public async void GetBoolean_ShouldEvaluate()
        {
            ResolutionDetails<bool> details = await this.commonProvider.ResolveBooleanValue("boolean-flag", false, EvaluationContext.Empty).ConfigureAwait(false);
            Assert.True(details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("on", details.Variant);
        }

        [Fact]
        public async void GetString_ShouldEvaluate()
        {
            ResolutionDetails<string> details = await this.commonProvider.ResolveStringValue("string-flag", "nope", EvaluationContext.Empty).ConfigureAwait(false);
            Assert.Equal("hi", details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("greeting", details.Variant);
        }

        [Fact]
        public async void GetInt_ShouldEvaluate()
        {
            ResolutionDetails<int> details = await this.commonProvider.ResolveIntegerValue("integer-flag", 13, EvaluationContext.Empty).ConfigureAwait(false);
            Assert.Equal(10, details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("ten", details.Variant);
        }

        [Fact]
        public async void GetDouble_ShouldEvaluate()
        {
            ResolutionDetails<double> details = await this.commonProvider.ResolveDoubleValue("float-flag", 13, EvaluationContext.Empty).ConfigureAwait(false);
            Assert.Equal(0.5, details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("half", details.Variant);
        }

        [Fact]
        public async void GetStruct_ShouldEvaluate()
        {
            ResolutionDetails<Value> details = await this.commonProvider.ResolveStructureValue("object-flag", new Value(), EvaluationContext.Empty).ConfigureAwait(false);
            Assert.Equal(true, details.Value.AsStructure["showImages"].AsBoolean);
            Assert.Equal("Check out these pics!", details.Value.AsStructure["title"].AsString);
            Assert.Equal(100, details.Value.AsStructure["imagesPerPage"].AsInteger);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("template", details.Variant);
        }

        [Fact]
        public async void MissingFlag_ShouldThrow()
        {
            await Assert.ThrowsAsync<FlagNotFoundException>(() => commonProvider.ResolveBooleanValue("missing-flag", false, EvaluationContext.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async void MismatchedFlag_ShouldThrow()
        {
            await Assert.ThrowsAsync<TypeMismatchException>(() => commonProvider.ResolveStringValue("boolean-flag", "nope", EvaluationContext.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async void PutConfiguration_shouldUpdateConfigAndRunHandlers()
        {
            var provider = new InMemoryProvider(new Dictionary<string, Flag>(){
            {
                "old-flag", new Flag<bool>(
                    variants: new Dictionary<string, bool>(){
                        { "on", true },
                        { "off", false }
                    },
                    defaultVariant: "on"
                )
            }});

            ResolutionDetails<bool> details = await provider.ResolveBooleanValue("old-flag", false, EvaluationContext.Empty).ConfigureAwait(false);
            Assert.True(details.Value);

            // update flags
            await provider.UpdateFlags(new Dictionary<string, Flag>(){
            {
                "new-flag", new Flag<string>(
                    variants: new Dictionary<string, string>(){
                        { "greeting", "hi" },
                        { "parting", "bye" }
                    },
                    defaultVariant: "greeting"
                )
            }}).ConfigureAwait(false);

            var res = await provider.GetEventChannel().Reader.ReadAsync().ConfigureAwait(false) as ProviderEventPayload;
            Assert.Equal(ProviderEventTypes.ProviderConfigurationChanged, res.Type);

            await Assert.ThrowsAsync<FlagNotFoundException>(() => provider.ResolveBooleanValue("old-flag", false, EvaluationContext.Empty)).ConfigureAwait(false);

            // new flag should be present, old gone (defaults), handler run.
            ResolutionDetails<string> detailsAfter = await provider.ResolveStringValue("new-flag", "nope", EvaluationContext.Empty).ConfigureAwait(false);
            Assert.True(details.Value);
            Assert.Equal("hi", detailsAfter.Value);
        }
    }
}
