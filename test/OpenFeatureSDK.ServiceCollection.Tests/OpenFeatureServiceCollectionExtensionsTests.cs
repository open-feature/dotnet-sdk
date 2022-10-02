using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenFeature.ServiceCollection;
using OpenFeature.ServiceCollection.Feature;
using OpenFeatureSDK.Model;
using Xunit;

namespace OpenFeatureSDK.ServiceCollection.Tests;

public class OpenFeatureServiceCollectionExtensionsTests

{
    [Fact]
    public void AddOpenFeatureServiceCollectionExtensionsTest()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddOpenFeautre(options => { });
        var serviceProvider = services.BuildServiceProvider();
        var openFeatureServiceCollectionExtensions = serviceProvider.GetService<FeatureClient>();
        Assert.NotNull(openFeatureServiceCollectionExtensions);
    }

    [Fact]
    public async Task InjectedFeatureClientShouldUseTheConfiguredProvider()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        var mock = new Mock<TestProvider>();

        mock.Setup(x => x.ResolveStringValue(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EvaluationContext>()))
            .ReturnsAsync(() => new ResolutionDetails<string>("key","test"));

        mock.Setup(x => x.GetProviderHooks())
            .Returns(() => new List<Hook>());

        mock.Setup(x => x.GetMetadata())
            .Returns(() => new Metadata("metadata"));

        services.AddTransient<TestProvider>(_=>mock.Object);
        services.AddOpenFeautre(options => { options.FeatureProvider = typeof(TestProvider); });

        var serviceProvider = services.BuildServiceProvider();
        var client=serviceProvider.GetService<FeatureClient>();
        var result = await client.GetStringValue("key", String.Empty);

        mock.Verify(obj =>obj.ResolveStringValue(
            "key ",
            It.IsAny<string>(),
            It.IsAny<EvaluationContext>()), Times.Once);

        result.Should().Be("test");
    }
    [Fact]
    public async Task InjectedIFeaturesShouldReturnTheValuesFromProvider()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        var mock = new Mock<TestProvider>();
        mock.Setup(x => x.GetProviderHooks())
            .Returns(() => new List<Hook>());

        mock.Setup(x => x.GetMetadata())
            .Returns(() => new Metadata("metadata"));

        mock.Setup(x => x.ResolveStringValue(
                "StringFeature",
                It.IsAny<string>(),
                It.IsAny<EvaluationContext>()))
            .ReturnsAsync(() => new ResolutionDetails<string>("StringFeature","test1"));

        mock.Setup(x => x.ResolveStringValue(
                "StringFeature2",
                It.IsAny<string>(),
                It.IsAny<EvaluationContext>()))
            .ReturnsAsync(() => new ResolutionDetails<string>("StringFeature2","test2"));

        mock.Setup(x => x.ResolveIntegerValue(
                "IntFeature",
                It.IsAny<int>(),
                It.IsAny<EvaluationContext>()))
            .ReturnsAsync(() => new ResolutionDetails<int>("IntFeature",10));


        mock.Setup(x => x.ResolveDoubleValue(
                "DoubleFeature",
                It.IsAny<double>(),
                It.IsAny<EvaluationContext>()))
            .ReturnsAsync(() => new ResolutionDetails<Double>("DoubleFeature",5.5));


        mock.Setup(x => x.ResolveBooleanValue(
                "BoolFeature",
                It.IsAny<bool>(),
                It.IsAny<EvaluationContext>()))
            .ReturnsAsync(() => new ResolutionDetails<bool>("BoolFeature",true));

        services.AddTransient<TestProvider>(_=>mock.Object);
        services.AddOpenFeautre(options => { options.FeatureProvider = typeof(TestProvider); });

        var serviceProvider = services.BuildServiceProvider();
        var features=serviceProvider.GetRequiredService<IFeatures<TestFeatures>>();
        var result = await features.GetValueAsync();

       result.Should().BeEquivalentTo(new TestFeatures
        {
            StringFeature = "test1",
            StringFeature2 = "test2",
            IntFeature = 10,
            DoubleFeature = 5.5,
            BoolFeature = true
        });
    }
}
