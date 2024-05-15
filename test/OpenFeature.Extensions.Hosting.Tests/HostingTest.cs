using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenFeature.Extensions.Hosting.Tests.TestingModels;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Extensions.Hosting.Tests;

public sealed class HostingTest
{
    [Fact]
    public async Task Can_register_no_op()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddOpenFeature();

        using var app = builder.Build();

#pragma warning disable xUnit1030
        await app.StartAsync().ConfigureAwait(false);
#pragma warning restore xUnit1030

        Assert.Equal(Api.Instance, app.Services.GetRequiredService<Api>());
        Assert.Equal(Api.Instance.GetProviderMetadata().Name,
            app.Services.GetRequiredService<IFeatureClient>().GetMetadata().Name);

        Assert.Empty(Api.Instance.GetContext().AsDictionary());
        Assert.Empty(app.Services.GetRequiredService<EvaluationContextBuilder>().Build().AsDictionary());
        Assert.Empty(app.Services.GetServices<EvaluationContext>());
        Assert.Empty(app.Services.GetServices<Hook>());
        Assert.Empty(app.Services.GetServices<FeatureProvider>());

#pragma warning disable xUnit1030
        await app.StopAsync().ConfigureAwait(false);
#pragma warning restore xUnit1030
    }

    [Fact]
    public async Task Can_register_some_feature_provider()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddOpenFeature(b =>
        {
            b.AddSomeFeatureProvider();
        });

        using var app = builder.Build();

        Assert.Equal(Api.Instance, app.Services.GetRequiredService<Api>());
        Assert.Equal("No-op Provider", app.Services.GetRequiredService<Api>().GetProviderMetadata().Name);

#pragma warning disable xUnit1030
        await app.StartAsync().ConfigureAwait(false);
#pragma warning restore xUnit1030

        Assert.Equal(Api.Instance, app.Services.GetRequiredService<Api>());
        Assert.Equal(SomeFeatureProvider.Name, app.Services.GetRequiredService<Api>().GetProviderMetadata().Name);
        Assert.Equal(SomeFeatureProvider.Name, app.Services.GetRequiredService<IFeatureClient>().GetMetadata().Name);

        Assert.Empty(Api.Instance.GetContext().AsDictionary());
        Assert.Empty(app.Services.GetRequiredService<EvaluationContextBuilder>().Build().AsDictionary());
        Assert.Empty(app.Services.GetServices<EvaluationContext>());
        Assert.Empty(app.Services.GetServices<Hook>());
        Assert.NotEmpty(app.Services.GetServices<FeatureProvider>());

#pragma warning disable xUnit1030
        await app.StopAsync().ConfigureAwait(false);
#pragma warning restore xUnit1030
    }
}
