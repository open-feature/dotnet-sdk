using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Xunit;

namespace OpenFeature.Tests.Providers.Memory
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
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
                    "context-aware", new Flag<string>(
                        variants: new Dictionary<string, string>(){
                            { "internal", "INTERNAL" },
                            { "external", "EXTERNAL" }
                        },
                        defaultVariant: "external",
                        (context) => {
                            if (context.GetValue("email").AsString?.Contains("@faas.com") == true)
                            {
                                return "internal";
                            }
                            else return "external";
                        }
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
                },
                {
                    "invalid-flag", new Flag<bool>(
                        variants: new Dictionary<string, bool>(){
                            { "on", true },
                            { "off", false }
                        },
                        defaultVariant: "missing"
                    )
                },
                {
                    "invalid-evaluator-flag", new Flag<bool>(
                        variants: new Dictionary<string, bool>(){
                            { "on", true },
                            { "off", false }
                        },
                        defaultVariant: "on",
                        (context) => {
                            return "missing";
                        }
                    )
                }
            });

            this.commonProvider = provider;
        }

        [Fact]
        public async Task GetBoolean_ShouldEvaluateWithReasonAndVariant()
        {
            ResolutionDetails<bool> details = await this.commonProvider.ResolveBooleanValueAsync("boolean-flag", false, EvaluationContext.Empty);
            Assert.True(details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("on", details.Variant);
        }

        [Fact]
        public async Task GetString_ShouldEvaluateWithReasonAndVariant()
        {
            ResolutionDetails<string> details = await this.commonProvider.ResolveStringValueAsync("string-flag", "nope", EvaluationContext.Empty);
            Assert.Equal("hi", details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("greeting", details.Variant);
        }

        [Fact]
        public async Task GetInt_ShouldEvaluateWithReasonAndVariant()
        {
            ResolutionDetails<int> details = await this.commonProvider.ResolveIntegerValueAsync("integer-flag", 13, EvaluationContext.Empty);
            Assert.Equal(10, details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("ten", details.Variant);
        }

        [Fact]
        public async Task GetDouble_ShouldEvaluateWithReasonAndVariant()
        {
            ResolutionDetails<double> details = await this.commonProvider.ResolveDoubleValueAsync("float-flag", 13, EvaluationContext.Empty);
            Assert.Equal(0.5, details.Value);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("half", details.Variant);
        }

        [Fact]
        public async Task GetStruct_ShouldEvaluateWithReasonAndVariant()
        {
            ResolutionDetails<Value> details = await this.commonProvider.ResolveStructureValueAsync("object-flag", new Value(), EvaluationContext.Empty);
            Assert.Equal(true, details.Value.AsStructure?["showImages"].AsBoolean);
            Assert.Equal("Check out these pics!", details.Value.AsStructure?["title"].AsString);
            Assert.Equal(100, details.Value.AsStructure?["imagesPerPage"].AsInteger);
            Assert.Equal(Reason.Static, details.Reason);
            Assert.Equal("template", details.Variant);
        }

        [Fact]
        public async Task GetString_ContextSensitive_ShouldEvaluateWithReasonAndVariant()
        {
            EvaluationContext context = EvaluationContext.Builder().Set("email", "me@faas.com").Build();
            ResolutionDetails<string> details = await this.commonProvider.ResolveStringValueAsync("context-aware", "nope", context);
            Assert.Equal("INTERNAL", details.Value);
            Assert.Equal(Reason.TargetingMatch, details.Reason);
            Assert.Equal("internal", details.Variant);
        }

        [Fact]
        public async Task EmptyFlags_ShouldWork()
        {
            var provider = new InMemoryProvider();
            await provider.UpdateFlags();
            Assert.Equal("InMemory", provider.GetMetadata().Name);
        }

        [Fact]
        public async Task MissingFlag_ShouldThrow()
        {
            await Assert.ThrowsAsync<FlagNotFoundException>(() => this.commonProvider.ResolveBooleanValueAsync("missing-flag", false, EvaluationContext.Empty));
        }

        [Fact]
        public async Task MismatchedFlag_ShouldThrow()
        {
            await Assert.ThrowsAsync<TypeMismatchException>(() => this.commonProvider.ResolveStringValueAsync("boolean-flag", "nope", EvaluationContext.Empty));
        }

        [Fact]
        public async Task MissingDefaultVariant_ShouldThrow()
        {
            await Assert.ThrowsAsync<GeneralException>(() => this.commonProvider.ResolveBooleanValueAsync("invalid-flag", false, EvaluationContext.Empty));
        }

        [Fact]
        public async Task MissingEvaluatedVariant_ShouldThrow()
        {
            await Assert.ThrowsAsync<GeneralException>(() => this.commonProvider.ResolveBooleanValueAsync("invalid-evaluator-flag", false, EvaluationContext.Empty));
        }

        [Fact]
        public async Task PutConfiguration_shouldUpdateConfigAndRunHandlers()
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

            ResolutionDetails<bool> details = await provider.ResolveBooleanValueAsync("old-flag", false, EvaluationContext.Empty);
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
            }});

            var res = await provider.GetEventChannel().Reader.ReadAsync() as ProviderEventPayload;
            Assert.Equal(ProviderEventTypes.ProviderConfigurationChanged, res?.Type);

            await Assert.ThrowsAsync<FlagNotFoundException>(() => provider.ResolveBooleanValueAsync("old-flag", false, EvaluationContext.Empty));

            // new flag should be present, old gone (defaults), handler run.
            ResolutionDetails<string> detailsAfter = await provider.ResolveStringValueAsync("new-flag", "nope", EvaluationContext.Empty);
            Assert.True(details.Value);
            Assert.Equal("hi", detailsAfter.Value);
        }
    }
}
